using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClinicAssistant.Domain.Scheduling;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppOutgoingMessageProcessor(ClinicAssistantDbContext dbContext, IWhatsAppGateway gateway, IWhatsAppConversationWindowPolicy conversationWindowPolicy, IWhatsAppTemplateVariableValidator templateVariableValidator, IWhatsAppChannelResolver channelResolver, ILogger<WhatsAppOutgoingMessageProcessor>? logger = null) : IWhatsAppOutgoingMessageProcessor
{
    private static readonly Action<ILogger, WhatsAppInteractionType, Exception?> InteractiveRendererSelected = LoggerMessage.Define<WhatsAppInteractionType>(LogLevel.Information, new EventId(4101, "InteractiveRendererSelected"), "WhatsApp renderer selected: interaction={InteractionType}, interactive=true");
    private static readonly Action<ILogger, WhatsAppInteractionType, Exception?> TextFallbackSelected = LoggerMessage.Define<WhatsAppInteractionType>(LogLevel.Information, new EventId(4102, "TextFallbackSelected"), "WhatsApp renderer selected: interaction={InteractionType}, interactive=false, fallback=text, reason=capability_unavailable");

    public async Task<WhatsAppOutgoingMessageProcessingResult> ProcessAsync(SendWhatsAppMessageCommand command, CancellationToken cancellationToken)
    {
        using var activity = WhatsAppTelemetry.ActivitySource.StartActivity("whatsapp.outgoing.process");
        if (command.TenantId == Guid.Empty || command.IntegrationId == Guid.Empty || command.ConversationId == Guid.Empty || command.ConversationMessageId == Guid.Empty || string.IsNullOrWhiteSpace(command.IdempotencyKey)) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        if (command.Type == WhatsAppOutgoingMessageType.Text && string.IsNullOrWhiteSpace(command.Text)) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        if (command.Type == WhatsAppOutgoingMessageType.Interactive && (string.IsNullOrWhiteSpace(command.Text) || command.Interaction is null || command.Interaction.Choices.Count == 0)) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        if (command.Type == WhatsAppOutgoingMessageType.Template && string.IsNullOrWhiteSpace(command.ContentSid)) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        if (command.Type is not WhatsAppOutgoingMessageType.Text and not WhatsAppOutgoingMessageType.Template and not WhatsAppOutgoingMessageType.Interactive) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.IntegrationId && item.TenantId == command.TenantId && item.Status == WhatsAppIntegrationStatus.Connected, cancellationToken);
        if (integration is null) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        var conversation = await dbContext.Conversations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.ConversationId && item.TenantId == command.TenantId && item.IntegrationId == command.IntegrationId, cancellationToken);
        if (conversation is null) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        var channel = await channelResolver.ResolveOutboundAsync(command.TenantId, command.WhatsAppChannelId ?? conversation.WhatsAppChannelId, cancellationToken);
        if (channel is null) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        var message = await dbContext.ConversationMessages.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.ConversationMessageId && item.TenantId == command.TenantId && item.ConversationId == command.ConversationId && item.Direction == ConversationMessageDirection.Outbound, cancellationToken);
        if (message is null) return WhatsAppOutgoingMessageProcessingResult.Rejected;
        if (!string.IsNullOrWhiteSpace(message.ExternalMessageId) || message.Status is ConversationMessageStatus.Accepted or ConversationMessageStatus.Sent or ConversationMessageStatus.Delivered or ConversationMessageStatus.Read) return WhatsAppOutgoingMessageProcessingResult.Duplicate;
        var result = await SendAsync(command, message, channel.SenderPhone, cancellationToken);
        if (result.Success && !string.IsNullOrWhiteSpace(result.ExternalMessageId))
        {
            message.MarkAccepted(result.ExternalMessageId, result.ProviderStatus);
            await MarkAppointmentReminderAsync(command, sent: true, result.Failure?.ProviderCode, result.Failure?.SafeMessage, cancellationToken);
            integration.MarkSuccessfulSend();
            await dbContext.SaveChangesAsync(cancellationToken);
            WhatsAppTelemetry.OutgoingMessages.Add(1);
            WhatsAppTelemetry.SendSuccess.Add(1);
            if (command.CorrelationId.StartsWith("integration-test:", StringComparison.Ordinal)) WhatsAppTelemetry.TestMessagesSent.Add(1);
            return WhatsAppOutgoingMessageProcessingResult.Sent;
        }

        message.MarkFailed(result.Failure?.ProviderCode, result.Failure?.SafeMessage ?? "The WhatsApp provider rejected the message.");
        await MarkAppointmentReminderAsync(command, sent: false, result.Failure?.ProviderCode, result.Failure?.SafeMessage, cancellationToken);
        integration.MarkSendFailure(result.Failure?.SafeMessage ?? "The WhatsApp provider rejected the message.");
        await dbContext.SaveChangesAsync(cancellationToken);
        WhatsAppTelemetry.OutgoingMessages.Add(1);
        WhatsAppTelemetry.SendFailure.Add(1);
        if (command.CorrelationId.StartsWith("integration-test:", StringComparison.Ordinal)) WhatsAppTelemetry.TestMessagesFailed.Add(1);
        return WhatsAppOutgoingMessageProcessingResult.Failed;
    }

    private async Task MarkAppointmentReminderAsync(SendWhatsAppMessageCommand command, bool sent, string? providerCode, string? reason, CancellationToken ct)
    {
        const string prefix = "appointment-reminder:";
        if (!command.IdempotencyKey.StartsWith(prefix, StringComparison.Ordinal) || !Guid.TryParse(command.IdempotencyKey[prefix.Length..], out var id)) return;
        var reminder = await dbContext.AppointmentReminders.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == command.TenantId, ct);
        if (reminder is null) return;
        if (sent) reminder.MarkSent(); else reminder.MarkFailed(providerCode, reason ?? "WhatsApp provider rejected the reminder.");
    }

    private async Task<SendWhatsAppMessageResult> SendAsync(SendWhatsAppMessageCommand command, ConversationMessage message, string senderPhone, CancellationToken cancellationToken)
    {
        if (command.Type is WhatsAppOutgoingMessageType.Text or WhatsAppOutgoingMessageType.Interactive)
        {
            if (message.Type != ConversationMessageType.Text) return PermanentFailure("The conversation message type does not match the command.");
            var lastInboundMessageAt = await dbContext.ConversationMessages.IgnoreQueryFilters().Where(item => item.ConversationId == command.ConversationId && item.Direction == ConversationMessageDirection.Inbound).MaxAsync(item => item.ReceivedAt, cancellationToken);
            if (!conversationWindowPolicy.Evaluate(lastInboundMessageAt, DateTimeOffset.UtcNow).AllowsFreeFormText) return PermanentFailure("A template is required outside the WhatsApp conversation window.");
            var supportsInteraction = command.Interaction is not null && (command.Interaction.Type == WhatsAppInteractionType.List
                ? gateway.Capabilities.SupportsInteractiveLists
                : gateway.Capabilities.SupportsReplyButtons);
            if (command.Type == WhatsAppOutgoingMessageType.Interactive && supportsInteraction)
            {
                if (logger is not null) InteractiveRendererSelected(logger, command.Interaction!.Type, null);
                return await gateway.SendInteractiveAsync(new(command.TenantId, command.IntegrationId, command.ConversationId, command.ConversationMessageId, command.RecipientPhone, command.Text!, command.Interaction!, command.IdempotencyKey, command.CorrelationId, senderPhone), cancellationToken);
            }
            if (command.Type == WhatsAppOutgoingMessageType.Interactive)
                if (logger is not null) TextFallbackSelected(logger, command.Interaction?.Type ?? WhatsAppInteractionType.List, null);
            return await gateway.SendTextAsync(new(command.TenantId, command.IntegrationId, command.ConversationId, command.ConversationMessageId, command.RecipientPhone, command.Text!, command.IdempotencyKey, command.CorrelationId, senderPhone), cancellationToken);
        }

        if (message.Type != ConversationMessageType.Template) return PermanentFailure("The conversation message type does not match the command.");
        var template = await dbContext.WhatsAppTemplates.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == command.TenantId && item.IntegrationId == command.IntegrationId && item.ContentSid == command.ContentSid, cancellationToken);
        var variables = command.ContentVariables ?? new Dictionary<string, string>();
        if (template is null || template.Status != WhatsAppTemplateStatus.Approved || !templateVariableValidator.IsValid(template.ParametersSchema, variables)) return PermanentFailure("The WhatsApp template is not available for this message.");
        return await gateway.SendTemplateAsync(new(command.TenantId, command.IntegrationId, command.ConversationId, command.ConversationMessageId, command.RecipientPhone, template.ContentSid, variables, command.IdempotencyKey, command.CorrelationId, senderPhone), cancellationToken);
    }

    private static SendWhatsAppMessageResult PermanentFailure(string message) => new(false, null, "failed", new(WhatsAppFailureType.Permanent, "policy_validation", message, false));
}
