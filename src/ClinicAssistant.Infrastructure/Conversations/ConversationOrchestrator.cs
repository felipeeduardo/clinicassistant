using System.Text.Json;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationOrchestrator(
    ClinicAssistantDbContext dbContext,
    IConversationLockManager lockManager,
    IConversationStateMachine stateMachine,
    IConversationResponseComposer responseComposer,
    IOperationalEventPublisher events,
    Microsoft.Extensions.Options.IOptions<ConversationOptions> options) : IConversationOrchestrator
{
    private readonly ConversationOptions _options = options.Value;

    public async Task<ConversationOrchestrationResult> ProcessAsync(ProcessConversationMessageCommand command, CancellationToken cancellationToken)
    {
        if (command.TenantId == Guid.Empty || command.IntegrationId == Guid.Empty || command.ConversationId == Guid.Empty || command.ConversationMessageId == Guid.Empty)
            return ConversationOrchestrationResult.Rejected;

        await using var conversationLock = await lockManager.TryAcquireAsync(command.TenantId, command.ConversationId, cancellationToken);
        if (conversationLock is null)
        {
            ConversationTelemetry.LockUnavailable.Add(1);
            return ConversationOrchestrationResult.LockUnavailable;
        }

        using var activity = ConversationTelemetry.ActivitySource.StartActivity("conversation.orchestrate");
        try
        {
            var conversation = await dbContext.Conversations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.ConversationId && item.TenantId == command.TenantId && item.IntegrationId == command.IntegrationId, cancellationToken);
            if (conversation is null) return ConversationOrchestrationResult.Rejected;
            var incomingMessage = await dbContext.ConversationMessages.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.ConversationMessageId && item.TenantId == command.TenantId && item.ConversationId == command.ConversationId && item.Direction == ConversationMessageDirection.Inbound, cancellationToken);
            if (incomingMessage is null) return ConversationOrchestrationResult.Rejected;
            if (await dbContext.ConversationProcessedMessages.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ConversationMessageId == command.ConversationMessageId, cancellationToken))
            {
                ConversationTelemetry.Duplicates.Add(1);
                return ConversationOrchestrationResult.Duplicate;
            }
            if (conversation.AutomationMode == ConversationAutomationMode.Human)
            {
                dbContext.ConversationProcessedMessages.Add(new ConversationProcessedMessage(command.TenantId, command.ConversationId, command.ConversationMessageId));
                await dbContext.SaveChangesAsync(cancellationToken);
                ConversationTelemetry.Processed.Add(1);
                return ConversationOrchestrationResult.Processed;
            }
            if (conversation.Status == ConversationStatus.Closed)
            {
                if (!_options.ReopenClosedConversations) return ConversationOrchestrationResult.Rejected;
                conversation.Reopen();
            }

            var patient = await dbContext.Patients.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == conversation.PatientId && item.TenantId == command.TenantId, cancellationToken);
            var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.IntegrationId && item.TenantId == command.TenantId && item.Status == WhatsAppIntegrationStatus.Connected, cancellationToken);
            if (patient is null || integration is null) return ConversationOrchestrationResult.Rejected;

            var state = await dbContext.ConversationStates.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.ConversationId == command.ConversationId && item.TenantId == command.TenantId, cancellationToken);
            if (state is null)
            {
                state = new ConversationState(command.TenantId, command.ConversationId, incomingMessage.ReceivedAt?.AddMinutes(_options.StateExpirationMinutes) ?? DateTimeOffset.UtcNow.AddMinutes(_options.StateExpirationMinutes));
                dbContext.ConversationStates.Add(state);
            }

            var transition = stateMachine.Transition(new(incomingMessage.ContentSanitized, state.FlowState, state.Status, state.Intent, state.InvalidAttempts, state.ExpiresAt, incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow));
            var response = responseComposer.Compose(new(transition.ResponseKey, transition.Options, _options.DefaultLanguage));
            if (response.Text.Length > _options.MaxMessageLength) return ConversationOrchestrationResult.Rejected;

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.StateExpirationMinutes);
            state.Apply(transition.FlowState, transition.Status, transition.Intent, transition.InvalidAttempts, expiresAt);
            if (transition.Action == ConversationAction.Handoff) conversation.ApplyAutomationMode(ConversationAutomationMode.Human);
            else if (transition.Action == ConversationAction.CloseConversation) conversation.Close();
            else conversation.ApplyAutomationMode(ConversationAutomationMode.Automated);
            var queueItemCreated = false;
            if (transition.Action == ConversationAction.Handoff)
            {
                var queueItem = await dbContext.HumanQueueItems.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.ConversationId == command.ConversationId && item.TenantId == command.TenantId, cancellationToken);
                if (queueItem is null)
                {
                    dbContext.HumanQueueItems.Add(new HumanQueueItem(command.TenantId, command.ConversationId, conversation.Priority, "Patient requested human assistance."));
                    queueItemCreated = true;
                }
            }

            var existingOptions = await dbContext.ConversationOptions.IgnoreQueryFilters().Where(item => item.TenantId == command.TenantId && item.ConversationStateId == state.Id).ToListAsync(cancellationToken);
            dbContext.ConversationOptions.RemoveRange(existingOptions);
            foreach (var option in transition.Options.Take(_options.MaxOptionsPerMessage))
                dbContext.ConversationOptions.Add(new ConversationOption(command.TenantId, state.Id, option.Key, option.Value, option.DisplayOrder, expiresAt));

            var outgoingMessage = new ConversationMessage(command.TenantId, command.ConversationId, ConversationMessageType.Text, response.Text, integration.Provider);
            var outgoingCommand = new SendWhatsAppMessageCommand(command.TenantId, command.IntegrationId, command.ConversationId, outgoingMessage.Id,
                WhatsAppOutgoingMessageType.Text, patient.Phone, response.Text, null, null, null, $"conversation:{command.ConversationMessageId:N}", command.CorrelationId);
            var outboxMessage = new OutboxMessage(command.TenantId, nameof(SendWhatsAppMessageCommand), JsonSerializer.Serialize(outgoingCommand));
            var processedMessage = new ConversationProcessedMessage(command.TenantId, command.ConversationId, command.ConversationMessageId);
            processedMessage.SetResponse(outgoingMessage.Id, outboxMessage.Id);

            dbContext.ConversationMessages.Add(outgoingMessage);
            dbContext.OutboxMessages.Add(outboxMessage);
            dbContext.ConversationProcessedMessages.Add(processedMessage);
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transition.Action == ConversationAction.Handoff)
            {
                var eventName = queueItemCreated ? "queue.item.created" : "queue.item.updated";
                await events.PublishAsync(command.TenantId, eventName, new { ConversationId = conversation.Id, Priority = conversation.Priority.ToString(), conversation.Version }, cancellationToken);
            }
            await events.PublishAsync(command.TenantId, "conversation.updated", new { conversation.Id, conversation.Version }, cancellationToken);
            await events.PublishAsync(command.TenantId, "dashboard.invalidated", new { }, cancellationToken);
            ConversationTelemetry.Processed.Add(1);
            return ConversationOrchestrationResult.Processed;
        }
        catch (DbUpdateConcurrencyException)
        {
            ConversationTelemetry.ConcurrencyConflicts.Add(1);
            return ConversationOrchestrationResult.ConcurrencyConflict;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            ConversationTelemetry.Duplicates.Add(1);
            return ConversationOrchestrationResult.Duplicate;
        }
    }
}
