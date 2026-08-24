using System.Text.Json;
using System.Globalization;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationOrchestrator(
    ClinicAssistantDbContext dbContext,
    IConversationLockManager lockManager,
    IConversationStateMachine stateMachine,
    IConversationResponseComposer responseComposer,
    IOperationalEventPublisher events,
    Microsoft.Extensions.Options.IOptions<ConversationOptions> options,
    ILogger<ConversationOrchestrator>? logger = null,
    IOperationalNotificationService? notifications = null) : IConversationOrchestrator
{
    private readonly ConversationOptions _options = options.Value;
    private static readonly Action<ILogger, string, bool, int, Exception?> InboundStateTrace = LoggerMessage.Define<string, bool, int>(LogLevel.Debug, new EventId(4201, "ConversationInboundState"), "Conversation inbound state: state={State}, selectedSlot={SelectedSlot}, currentActions={CurrentActions}");
    private static readonly Action<ILogger, string, string, Exception?> ActionResolutionTrace = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(4202, "ConversationActionResolution"), "Conversation action resolved: input={Input}, action={Action}");
    private static readonly Action<ILogger, string, string, Exception?> HandlerTrace = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(4203, "ConversationHandler"), "Conversation handler: state={State}, handler={Handler}");
    private static readonly Action<ILogger, string, Exception?> NextStateTrace = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4204, "ConversationNextState"), "Conversation next state: state={State}");
    private static readonly Action<ILogger, Exception?> NotificationFailureTrace = LoggerMessage.Define(LogLevel.Warning, new EventId(4205, "HumanQueueNotificationFailure"), "Human queue notification could not be persisted; handoff remains successful.");

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
                await events.PublishAsync(command.TenantId, "conversation.updated", new
                {
                    ConversationId = command.ConversationId,
                    MessageId = command.ConversationMessageId,
                    Status = conversation.Status.ToString(),
                    AutomationMode = conversation.AutomationMode.ToString(),
                    conversation.AssignedUserId,
                    conversation.Version
                }, cancellationToken);
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

            var stateContext = ReadContext(state.ContextJson, state.Intent, state.FlowState, state.InvalidAttempts);
            var previousStep = state.FlowState;
            var existingOptions = await dbContext.ConversationOptions.IgnoreQueryFilters()
                .Where(item => item.TenantId == command.TenantId && item.ConversationStateId == state.Id && item.ExpiresAt > (incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow))
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new ConversationOptionDefinition(item.Key, item.Value, item.DisplayOrder, item.ActionId))
                .ToListAsync(cancellationToken);
            if (logger is not null) InboundStateTrace(logger, state.FlowState.ToString(), stateContext.PendingConfirmation && stateContext.SelectedSlotStartsAt.HasValue, existingOptions.Count, null);
            stateContext = ApplyContextualDate(ApplyContextualSelection(stateContext, incomingMessage.ContentSanitized, existingOptions), incomingMessage.ContentSanitized, incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow);
            var normalizedInput = ConversationIntentResolver.Normalize(incomingMessage.ContentSanitized);
            var selectedOptionValue = ResolveSelectedOptionValue(incomingMessage.ContentSanitized, existingOptions);
            activity?.SetTag("conversation.displayed_option", int.TryParse(incomingMessage.ContentSanitized?.Trim(), out var displayedOption) ? displayedOption : null);
            activity?.SetTag("conversation.selected_slot_start_utc", stateContext.SelectedSlotStartsAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            if (IsOtherDayRequest(normalizedInput) || selectedOptionValue == "other_days")
                stateContext = stateContext with { AwaitingAvailableDaySelection = true, AwaitingDateSelection = false, SelectedDate = null, AvailabilityCursor = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null, PendingConfirmation = false };
            else if (IsDateRequest(normalizedInput) || selectedOptionValue == "date_request")
                stateContext = stateContext with { AwaitingDateSelection = true, AwaitingAvailableDaySelection = false, SelectedDate = null, AvailabilityCursor = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
            else if (IsDateInput(normalizedInput))
                stateContext = stateContext with { AwaitingDateSelection = false, AwaitingAvailableDaySelection = false, AvailabilityCursor = null };
            else if (normalizedInput is "mais horarios" or "mais opcoes" or "ver mais" || selectedOptionValue == "more_slots")
                stateContext = stateContext with { AwaitingAvailableDaySelection = false, AwaitingDateSelection = false, SelectedSlotId = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null, PendingConfirmation = false };
            else if (selectedOptionValue?.StartsWith("day:", StringComparison.Ordinal) == true && DateOnly.TryParse(selectedOptionValue[4..], CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedAvailabilityDate))
                stateContext = stateContext with { SelectedDate = selectedAvailabilityDate, AwaitingAvailableDaySelection = false, AwaitingDateSelection = false, AvailabilityCursor = null };
            if (stateContext.CurrentIntent == ConversationIntent.RescheduleAppointment && stateContext.SelectedAppointmentId.HasValue)
                stateContext = await EnrichRescheduleContextAsync(stateContext, command.TenantId, patient.Id, cancellationToken);
            AvailabilityTargetResolution? availabilityTarget = null;
            if (stateContext.CurrentIntent == ConversationIntent.CheckAvailability
                && !stateContext.SelectedSpecialtyId.HasValue
                && !stateContext.SelectedProfessionalId.HasValue
                && !int.TryParse(incomingMessage.ContentSanitized?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
                && !string.IsNullOrWhiteSpace(incomingMessage.ContentSanitized))
            {
                availabilityTarget = await ResolveAvailabilityTargetAsync(incomingMessage.ContentSanitized!, command.TenantId, cancellationToken);
                if (availabilityTarget.Kind == AvailabilityTargetKind.Specialty && availabilityTarget.Id.HasValue)
                    stateContext = stateContext with { SelectedSpecialtyId = availabilityTarget.Id, SelectedSpecialtyName = availabilityTarget.Name, CurrentIntent = ConversationIntent.CheckAvailability, CurrentStep = ConversationFlowState.AwaitingSelection };
                else if (availabilityTarget.Kind == AvailabilityTargetKind.Professional && availabilityTarget.Id.HasValue)
                    stateContext = stateContext with { SelectedProfessionalId = availabilityTarget.Id, SelectedProfessionalName = availabilityTarget.Name, CurrentIntent = ConversationIntent.CheckAvailability, CurrentStep = ConversationFlowState.AwaitingSelection, AwaitingAvailableDaySelection = true };
            }
            if (stateContext.SelectedProfessionalId.HasValue && !stateContext.SelectedDate.HasValue && !stateContext.AwaitingDateSelection && !stateContext.PendingConfirmation && selectedOptionValue is not "more_slots")
                stateContext = stateContext with { AwaitingAvailableDaySelection = true };
            if (stateContext.SelectedProfessionalId.HasValue && !stateContext.SelectedSpecialtyId.HasValue)
            {
                var professionalSpecialties = await GetProfessionalSpecialtiesAsync(stateContext.SelectedProfessionalId.Value, command.TenantId, cancellationToken);
                if (professionalSpecialties.Count == 1)
                    stateContext = stateContext with
                    {
                        SelectedSpecialtyId = professionalSpecialties[0].Id,
                        SelectedSpecialtyName = professionalSpecialties[0].Name,
                        CurrentIntent = ConversationIntent.CheckAvailability,
                        CurrentStep = ConversationFlowState.AwaitingSelection,
                        AwaitingAvailableDaySelection = true
                    };
            }
            var transition = stateMachine.Transition(new(incomingMessage.ContentSanitized, state.FlowState, state.Status, state.Intent, state.InvalidAttempts, state.ExpiresAt, incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow, existingOptions, stateContext));
            if (availabilityTarget is { Kind: AvailabilityTargetKind.Specialty or AvailabilityTargetKind.Professional }
                && transition.Intent is (ConversationIntent.Unknown or ConversationIntent.Unsupported))
            {
                transition = new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.CheckAvailability, ConversationAction.None, 0, "conversation.availability", []);
            }
            if (logger is not null) ActionResolutionTrace(logger, incomingMessage.ContentSanitized ?? string.Empty, transition.Intent.ToString(), null);
            if (logger is not null) HandlerTrace(logger, state.FlowState.ToString(), transition.Intent.ToString(), null);
            ConversationTelemetry.RecordIntent(transition.Intent, transition.FlowState);
            if (transition.Intent is ConversationIntent.Unknown or ConversationIntent.Unsupported) ConversationTelemetry.InvalidInput.Add(1);
            if (transition.Action == ConversationAction.Handoff) ConversationTelemetry.Handoff.Add(1);
            if (transition.ResponseKey == "conversation.expired") ConversationTelemetry.FlowTimeout.Add(1);
            if (transition.Action == ConversationAction.CancelFlow) ConversationTelemetry.FlowAbandoned.Add(1);
            if (transition.Action == ConversationAction.CloseConversation) ConversationTelemetry.FlowCompleted.Add(1);
            if (state.FlowState == ConversationFlowState.Initial && transition.FlowState != ConversationFlowState.Initial) ConversationTelemetry.FlowStarted.Add(1);
            var appointmentCreated = false;
            var (responseOptions, responseText) = availabilityTarget is { Kind: AvailabilityTargetKind.NoMatch or AvailabilityTargetKind.Ambiguous }
                && transition.Action != ConversationAction.Handoff
                ? BuildAvailabilityTargetFallback(availabilityTarget, state.InvalidAttempts)
                : await BuildInformationalResponseAsync(transition, stateContext, command.TenantId, patient.Id, cancellationToken);
            if (transition.Intent is (ConversationIntent.ConfirmSelectedSlot or ConversationIntent.ConfirmAppointment) && stateContext.PendingConfirmation && stateContext.CurrentIntent == ConversationIntent.ScheduleAppointment && stateContext.SelectedSlotStartsAt.HasValue)
            {
                var schedulingResult = await TryCreateScheduledAppointmentAsync(command, patient.Id, stateContext, cancellationToken);
                responseText = schedulingResult.Message;
                if (schedulingResult.Success)
                {
                    appointmentCreated = true;
                    stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedSpecialtyId = null, SelectedProfessionalId = null, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
                }
                else
                {
                    stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.CheckAvailability, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
                    var retry = await BuildInformationalResponseAsync(
                        new(ConversationFlowState.AwaitingSlotSelection, ConversationStateStatus.Active, ConversationIntent.CheckAvailability, ConversationAction.None, 0, "conversation.availability", []),
                        stateContext, command.TenantId, patient.Id, cancellationToken);
                    responseOptions = retry.Options;
                    responseText = $"{schedulingResult.Message}\n\n{retry.Text}";
                }
            }
            else if (transition.Intent is (ConversationIntent.ConfirmSelectedSlot or ConversationIntent.ConfirmAppointment) && stateContext.PendingConfirmation && stateContext.CurrentIntent == ConversationIntent.ScheduleAppointment)
            {
                activity?.SetTag("conversation.state_inconsistency", true);
                stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.CheckAvailability, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
                var retry = await BuildInformationalResponseAsync(
                    new(ConversationFlowState.AwaitingSlotSelection, ConversationStateStatus.Active, ConversationIntent.CheckAvailability, ConversationAction.None, 0, "conversation.availability", []),
                    stateContext, command.TenantId, patient.Id, cancellationToken);
                responseOptions = retry.Options;
                responseText = $"Não consegui continuar com esse horário. Vou mostrar as opções novamente.\n\n{retry.Text}";
            }
            else if (transition.Intent == ConversationIntent.ConfirmReschedule && stateContext.PendingConfirmation && stateContext.CurrentIntent == ConversationIntent.RescheduleAppointment && stateContext.SelectedAppointmentId.HasValue)
            {
                var operation = await TryRescheduleAppointmentAsync(command, patient.Id, stateContext, cancellationToken);
                responseText = operation.Message;
                if (operation.Success)
                    stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedAppointmentId = null, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
            }
            else if (transition.Intent is (ConversationIntent.CancelAppointment or ConversationIntent.ConfirmExistingAppointment or ConversationIntent.ConfirmAppointment) && stateContext.PendingConfirmation && stateContext.SelectedAppointmentId.HasValue)
            {
                var operation = transition.Intent == ConversationIntent.CancelAppointment
                    ? await TryCancelAppointmentAsync(command, patient.Id, stateContext.SelectedAppointmentId.Value, cancellationToken)
                    : await TryConfirmAppointmentAsync(command, patient.Id, stateContext.SelectedAppointmentId.Value, cancellationToken);
                responseText = operation.Message;
                if (operation.Success) stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedAppointmentId = null };
            }
            var optionsAlreadyRendered = responseOptions.Any(option =>
                option.Value.StartsWith("slot:", StringComparison.Ordinal) ||
                option.Value.StartsWith("day:", StringComparison.Ordinal))
                || availabilityTarget is { Kind: AvailabilityTargetKind.NoMatch or AvailabilityTargetKind.Ambiguous };
            var response = responseComposer.Compose(new(transition.ResponseKey, responseOptions, _options.DefaultLanguage, responseText, optionsAlreadyRendered));
            if (response.Text.Length > _options.MaxMessageLength) return ConversationOrchestrationResult.Rejected;

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.StateExpirationMinutes);
            if (transition.Action is ConversationAction.ShowMenu or ConversationAction.CancelFlow)
                stateContext = ClearTransientSelection(stateContext);
            else if (transition.Action == ConversationAction.GoBack)
                stateContext = stateContext with
                {
                    SelectedSpecialtyId = transition.Intent == ConversationIntent.ListSpecialties ? null : stateContext.SelectedSpecialtyId,
                    SelectedSpecialtyName = transition.Intent == ConversationIntent.ListSpecialties ? null : stateContext.SelectedSpecialtyName,
                    SelectedProfessionalId = transition.Intent == ConversationIntent.ViewProfessionals ? null : stateContext.SelectedProfessionalId,
                    SelectedProfessionalName = transition.Intent == ConversationIntent.ViewProfessionals ? null : stateContext.SelectedProfessionalName,
                    SelectedSlotId = null,
                    SelectedSlotStartsAt = null,
                    SelectedSlotEndsAt = null,
                    PendingConfirmation = false,
                    SelectedDate = transition.Intent == ConversationIntent.CheckAvailability && previousStep == ConversationFlowState.AwaitingSlotSelection ? null : stateContext.SelectedDate,
                    AwaitingAvailableDaySelection = transition.Intent == ConversationIntent.CheckAvailability && previousStep == ConversationFlowState.AwaitingSlotSelection
                };
            stateContext = AdvanceAvailabilityCursor(stateContext, responseOptions);
            var persistedIntent = stateContext.PendingConfirmation && stateContext.SelectedSlotStartsAt.HasValue
                ? stateContext.CurrentIntent == ConversationIntent.RescheduleAppointment
                    ? ConversationIntent.RescheduleAppointment
                    : ConversationIntent.ScheduleAppointment
                : stateContext.CurrentIntent == ConversationIntent.CheckAvailability
                    ? ConversationIntent.CheckAvailability
                : appointmentCreated ? ConversationIntent.MainMenu : transition.Intent;
            var persistedFlowState = appointmentCreated
                ? ConversationFlowState.Menu
                : stateContext.CurrentIntent == ConversationIntent.CheckAvailability && !stateContext.SelectedSlotStartsAt.HasValue
                    ? ConversationFlowState.AwaitingSlotSelection
                    : transition.FlowState;
            state.Apply(persistedFlowState, transition.Status, persistedIntent, transition.InvalidAttempts, expiresAt);
            activity?.SetTag("conversation.resolved_action", transition.Intent.ToString());
            activity?.SetTag("conversation.next_state", transition.FlowState.ToString());
            activity?.SetTag("conversation.selected_slot_start_utc", stateContext.SelectedSlotStartsAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            state.UpdateContext(JsonSerializer.Serialize(stateContext with
            {
                CurrentIntent = persistedIntent,
                CurrentStep = persistedFlowState,
                PreviousStep = previousStep,
                InvalidAttemptCount = transition.InvalidAttempts,
                LastUserMessage = incomingMessage.ContentSanitized,
                LastBotMessage = response.Text,
                LastInteractionAt = incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow,
                FlowStartedAt = stateContext.FlowStartedAt ?? incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow
            }));
            if (logger is not null) NextStateTrace(logger, persistedFlowState.ToString(), null);
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
                dbContext.AuditRecords.Add(new AuditRecord(command.TenantId, null, "conversation.handoff_requested", "Conversation", command.ConversationId, "Succeeded", "Conversation queued for human assistance."));
            }

            var optionsToReplace = await dbContext.ConversationOptions.IgnoreQueryFilters().Where(item => item.TenantId == command.TenantId && item.ConversationStateId == state.Id).ToListAsync(cancellationToken);
            dbContext.ConversationOptions.RemoveRange(optionsToReplace);
            foreach (var option in responseOptions.Take(_options.MaxOptionsPerMessage))
                dbContext.ConversationOptions.Add(new ConversationOption(command.TenantId, state.Id, option.Key, option.Value, option.DisplayOrder, expiresAt, option.ActionId));

            var outgoingMessage = new ConversationMessage(command.TenantId, command.ConversationId, ConversationMessageType.Text, response.Text, integration.Provider);
            var interaction = ToWhatsAppInteraction(response.Interaction);
            var outgoingCommand = new SendWhatsAppMessageCommand(command.TenantId, command.IntegrationId, command.ConversationId, outgoingMessage.Id,
                interaction is null ? WhatsAppOutgoingMessageType.Text : WhatsAppOutgoingMessageType.Interactive, patient.Phone, response.Text, null, null, null, $"conversation:{command.ConversationMessageId:N}", command.CorrelationId, interaction);
            var outboxMessage = new OutboxMessage(command.TenantId, nameof(SendWhatsAppMessageCommand), JsonSerializer.Serialize(outgoingCommand));
            var processedMessage = new ConversationProcessedMessage(command.TenantId, command.ConversationId, command.ConversationMessageId);
            processedMessage.SetResponse(outgoingMessage.Id, outboxMessage.Id);

            dbContext.ConversationMessages.Add(outgoingMessage);
            dbContext.OutboxMessages.Add(outboxMessage);
            dbContext.ConversationProcessedMessages.Add(processedMessage);
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transition.Action == ConversationAction.Handoff)
            {
                try { if (notifications is not null) await notifications.CreateInitialAsync(command.TenantId, command.ConversationId, command.CorrelationId, cancellationToken); }
                catch (Exception exception) { if (logger is not null) NotificationFailureTrace(logger, exception); }
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

    private static ConversationContext ReadContext(string json, ConversationIntent intent, ConversationFlowState step, int invalidAttempts)
    {
        try { return JsonSerializer.Deserialize<ConversationContext>(json) ?? new(intent, step, null, invalidAttempts); }
        catch (JsonException) { return new(intent, step, null, invalidAttempts); }
    }

    private static WhatsAppInteraction? ToWhatsAppInteraction(ConversationInteraction? interaction)
    {
        if (interaction is null || interaction.Choices.Count == 0) return null;
        var type = interaction.Type == ConversationInteractionType.ReplyButtons
            ? WhatsAppInteractionType.ReplyButtons
            : WhatsAppInteractionType.List;
        return new(type, interaction.Choices.Select(choice => new WhatsAppChoice(choice.ActionId, choice.Label, choice.Description)).ToArray());
    }

    internal static ConversationContext ApplyContextualSelection(ConversationContext context, string? message, IReadOnlyCollection<ConversationOptionDefinition> options)
    {
        if (!int.TryParse(message?.Trim(), out var number))
        {
            var normalized = CanonicalOptionText(message);
            var textOption = options.FirstOrDefault(option =>
                CanonicalOptionText(option.Value.Split("||", 2).ElementAtOrDefault(1)) == normalized
                || CanonicalOptionText(option.Value.Split("||", 2).ElementAtOrDefault(1)).StartsWith(normalized + " ", StringComparison.Ordinal));
            return textOption is null ? context : ApplyOptionValue(context, textOption.Value);
        }

        var value = options.SingleOrDefault(item => item.Key == number.ToString(CultureInfo.InvariantCulture))?.Value;
        return value is null ? context : ApplyOptionValue(context, value);
    }

    private static ConversationContext ApplyOptionValue(ConversationContext context, string value)
    {
        var machineValue = value.Split("||", 2, StringSplitOptions.None)[0];
        var displayName = value.Split("||", 2, StringSplitOptions.None).ElementAtOrDefault(1);
        if (machineValue.StartsWith("specialty:", StringComparison.Ordinal) && Guid.TryParse(machineValue[10..], out var specialtyId))
            return context with
            {
                SelectedSpecialtyId = specialtyId,
                SelectedSpecialtyName = displayName,
                CurrentIntent = context.SelectedProfessionalId.HasValue ? ConversationIntent.CheckAvailability : ConversationIntent.ViewProfessionals,
                CurrentStep = ConversationFlowState.AwaitingSelection,
                AwaitingAvailableDaySelection = context.SelectedProfessionalId.HasValue,
                AwaitingDateSelection = false
            };
        if (machineValue.StartsWith("professional:", StringComparison.Ordinal))
        {
            var professionalParts = machineValue[13..].Split('|', StringSplitOptions.TrimEntries);
            if (Guid.TryParse(professionalParts[0], out var professionalId))
                return context with
                {
                    SelectedProfessionalId = professionalId,
                    SelectedUnitId = professionalParts.Length > 1 && Guid.TryParse(professionalParts[1], out var unitId) ? unitId : context.SelectedUnitId,
                    SelectedProfessionalName = displayName,
                    CurrentIntent = ConversationIntent.CheckAvailability,
                    CurrentStep = ConversationFlowState.AwaitingSelection,
                    AwaitingAvailableDaySelection = true,
                    AwaitingDateSelection = false,
                    SelectedDate = null,
                    SelectedSlotId = null,
                    SelectedSlotStartsAt = null,
                    SelectedSlotEndsAt = null,
                    PendingConfirmation = false
                };
        }
        if (machineValue.StartsWith("appointment:", StringComparison.Ordinal))
        {
            var appointmentParts = machineValue[12..].Split('|', StringSplitOptions.TrimEntries);
            if (appointmentParts.Length > 0 && Guid.TryParse(appointmentParts[0], out var appointmentId))
                return context with { SelectedAppointmentId = appointmentId, SelectedAppointmentVersion = appointmentParts.Length > 1 && int.TryParse(appointmentParts[1], CultureInfo.InvariantCulture, out var version) ? version : null, PendingConfirmation = context.CurrentIntent is ConversationIntent.CancelAppointment or ConversationIntent.ConfirmExistingAppointment or ConversationIntent.ConfirmAppointment };
        }
        if (machineValue.StartsWith("day:", StringComparison.Ordinal) && DateOnly.TryParse(machineValue[4..].Split("||", 2)[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
            return context with { SelectedDate = selectedDate, AwaitingAvailableDaySelection = false, AwaitingDateSelection = false, AvailabilityCursor = null, CurrentIntent = context.CurrentIntent == ConversationIntent.RescheduleAppointment ? ConversationIntent.RescheduleAppointment : ConversationIntent.ScheduleAppointment, CurrentStep = ConversationFlowState.AwaitingSlotSelection };
        if (machineValue.StartsWith("slot:", StringComparison.Ordinal))
        {
            var parts = machineValue.Split('|', StringSplitOptions.TrimEntries);
            var startIndex = parts.Length >= 4 ? 2 : 1;
            if (parts.Length > startIndex + 1
                && DateTimeOffset.TryParse(parts[startIndex], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var startsAt)
                && DateTimeOffset.TryParse(parts[startIndex + 1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var endsAt))
                return context with
                {
                    CurrentIntent = context.CurrentIntent == ConversationIntent.RescheduleAppointment ? ConversationIntent.RescheduleAppointment : ConversationIntent.ScheduleAppointment,
                    CurrentStep = ConversationFlowState.AwaitingScheduleConfirmation,
                    SelectedProfessionalId = Guid.TryParse(parts[0][5..], out var professionalId) ? professionalId : context.SelectedProfessionalId,
                    SelectedUnitId = startIndex == 2 && Guid.TryParse(parts[1], out var unitId) ? unitId : context.SelectedUnitId,
                    SelectedSlotStartsAt = startsAt.ToUniversalTime(),
                    SelectedSlotEndsAt = endsAt.ToUniversalTime(),
                    PendingConfirmation = true
                };
        }
        return context;
    }

    private static string CanonicalOptionText(string? value) =>
        string.Concat(ConversationIntentResolver.Normalize(value).Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character)));

    private static ConversationContext ClearTransientSelection(ConversationContext context) => context with
    {
        CurrentIntent = ConversationIntent.MainMenu,
        CurrentStep = ConversationFlowState.Menu,
        SelectedSpecialtyId = null,
        SelectedSpecialtyName = null,
        SelectedProfessionalId = null,
        SelectedProfessionalName = null,
        SelectedUnitId = null,
        SelectedDate = null,
        SelectedSlotId = null,
        SelectedSlotStartsAt = null,
        SelectedSlotEndsAt = null,
        SelectedAppointmentId = null,
        PendingConfirmation = false,
        AvailabilityCursor = null,
        AwaitingDateSelection = false,
        AwaitingAvailableDaySelection = false
    };

    private static bool IsDateRequest(string normalizedInput) =>
        normalizedInput is "outra data" or "consultar outra data";

    private static bool IsOtherDayRequest(string normalizedInput) =>
        normalizedInput is "outros dias" or "outro dia";

    private static string? ResolveSelectedOptionValue(string? message, IReadOnlyCollection<ConversationOptionDefinition> options)
    {
        if (!int.TryParse(message?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return null;
        return options.SingleOrDefault(option => option.Key == number.ToString(CultureInfo.InvariantCulture))?.Value.Split("||", 2, StringSplitOptions.None)[0];
    }

    private static bool IsDateInput(string normalizedInput) =>
        normalizedInput.Contains("hoje", StringComparison.Ordinal)
        || normalizedInput.Contains("amanha", StringComparison.Ordinal)
        || normalizedInput.Contains("segunda", StringComparison.Ordinal)
        || normalizedInput.Contains("terca", StringComparison.Ordinal)
        || normalizedInput.Contains("quarta", StringComparison.Ordinal)
        || normalizedInput.Contains("quinta", StringComparison.Ordinal)
        || normalizedInput.Contains("sexta", StringComparison.Ordinal)
        || normalizedInput.Contains("sabado", StringComparison.Ordinal)
        || normalizedInput.Contains("domingo", StringComparison.Ordinal)
        || System.Text.RegularExpressions.Regex.IsMatch(normalizedInput, "\\b\\d{1,2}/\\d{1,2}\\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private static ConversationContext AdvanceAvailabilityCursor(ConversationContext context, IReadOnlyCollection<ConversationOptionDefinition> options)
    {
        var ends = options.Select(option => option.Value.Split('|', StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length >= 3 && parts[0].StartsWith("slot:", StringComparison.Ordinal))
            .Select(parts => DateTimeOffset.TryParse(parts.Length >= 4 ? parts[3] : parts[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : (DateTimeOffset?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .OrderByDescending(value => value)
            .FirstOrDefault();
        return ends == default ? context : context with { AvailabilityCursor = ends };
    }

    private static ConversationContext ApplyContextualDate(ConversationContext context, string? message, DateTimeOffset receivedAt)
    {
        var normalized = ConversationIntentResolver.Normalize(message);
        var explicitDate = System.Text.RegularExpressions.Regex.Match(normalized, "\\b(?<day>\\d{1,2})/(?<month>\\d{1,2})\\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (explicitDate.Success
            && int.TryParse(explicitDate.Groups["day"].Value, CultureInfo.InvariantCulture, out var day)
            && int.TryParse(explicitDate.Groups["month"].Value, CultureInfo.InvariantCulture, out var month)
            && DateOnly.TryParse($"{receivedAt.Year:0000}-{month:00}-{day:00}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            return context with { SelectedDate = parsedDate };
        var date = normalized switch
        {
            var value when value.Contains("depois de amanha", StringComparison.Ordinal) => DateOnly.FromDateTime(receivedAt.DateTime).AddDays(2),
            var value when value.Contains("amanha", StringComparison.Ordinal) => DateOnly.FromDateTime(receivedAt.DateTime).AddDays(1),
            var value when value.Contains("hoje", StringComparison.Ordinal) => DateOnly.FromDateTime(receivedAt.DateTime),
            _ => context.SelectedDate
        };
        return context with { SelectedDate = date };
    }

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildInformationalResponseAsync(ConversationTransitionResult transition, ConversationContext context, Guid tenantId, Guid patientId, CancellationToken cancellationToken)
    {
        if (transition.ResponseKey is "conversation.greeting" or "conversation.menu" or "conversation.expired")
        {
            var clinic = await dbContext.Clinics.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(item => item.TenantId == tenantId, cancellationToken);
            var assistant = string.IsNullOrWhiteSpace(clinic?.AssistantDisplayName) ? "IA Recepção" : clinic.AssistantDisplayName.Trim();
            var clinicName = string.IsNullOrWhiteSpace(clinic?.TradeName) ? "a clínica" : clinic.TradeName.Trim();
            var text = transition.ResponseKey switch
            {
                "conversation.greeting" => $"Olá! 👋\n\nEu sou a {assistant}, assistente virtual da {clinicName}.\nEstou por aqui para ajudar com sua consulta.\n\nComo posso ajudar?",
                "conversation.expired" => "Vamos continuar por aqui. Como posso ajudar?",
                _ => "Claro. Como posso ajudar agora?"
            };
            return (ConversationStateMachine.MenuOptions(), text);
        }
        if (transition.Intent is (ConversationIntent.ConfirmSelectedSlot or ConversationIntent.ConfirmAppointment) && context.PendingConfirmation && context.CurrentIntent == ConversationIntent.ScheduleAppointment)
            return ([], context.SelectedSlotStartsAt.HasValue ? null : "Não consegui continuar com esse horário. Vou consultar os horários novamente.");
        if (transition.Intent is (ConversationIntent.RescheduleAppointment or ConversationIntent.CancelAppointment or ConversationIntent.ConfirmExistingAppointment or ConversationIntent.ConfirmAppointment))
            return await BuildAppointmentOperationResponseAsync(transition.Intent, context, tenantId, patientId, cancellationToken);
        if (transition.Intent == ConversationIntent.ScheduleAppointment)
        {
            if (!context.SelectedSpecialtyId.HasValue)
                return await BuildSpecialtiesAsync("Escolha uma especialidade para começarmos:", tenantId, cancellationToken);
            if (!context.SelectedProfessionalId.HasValue)
            {
                var specialtyId = context.SelectedSpecialtyId.Value;
                var professionals = await dbContext.Professionals.IgnoreQueryFilters().AsNoTracking().Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active && item.Specialties.Any(specialty => specialty.SpecialtyId == specialtyId)).OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
                var options = professionals.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"professional:{item.Id}|{item.ClinicUnitId}||{item.Name}", index + 1, $"professional:{item.Id}")).ToList();
                return (options, professionals.Count == 0 ? "Não encontrei profissionais para essa especialidade." : "Encontrei estes profissionais. Qual você prefere?");
            }
            if (!context.SelectedDate.HasValue)
            {
                if (context.AwaitingDateSelection)
                    return ([], "Qual data você prefere? Você pode escrever, por exemplo, *amanhã* ou *25/08*.");
                return await BuildAvailableDayResponseAsync(context, tenantId, cancellationToken);
            }
            if (!context.SelectedSlotStartsAt.HasValue)
            {
                var slots = await GetSlotsAsync(context.SelectedProfessionalId.Value, context.SelectedDate.Value, tenantId, cancellationToken);
                var timeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var unitId = context.SelectedUnitId ?? await GetProfessionalUnitIdAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var options = BuildSlotOptions(slots.Take(_options.MaxOptionsPerMessage).ToList(), context.SelectedProfessionalId.Value, unitId, timeZone);
                return (options, slots.Count == 0
                    ? "Não encontrei horários nessa data. Você prefere tentar outro dia?"
                    : BuildSlotText("Encontrei estes horários. Qual você prefere?", slots.Take(_options.MaxOptionsPerMessage).ToList(), options, timeZone, "Para consultar outros dias, escreva *outros dias*."));
            }
            var confirmationTimeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
            var localSlot = TimeZoneInfo.ConvertTime(context.SelectedSlotStartsAt.Value, confirmationTimeZone);
            return ([new("1", "confirm_slot", 1, "confirm_slot"), new("2", "more_slots", 2, "more_slots")], $"Você escolheu:\n{context.SelectedProfessionalName ?? "o profissional selecionado"}\n{localSlot:dd/MM} às {localSlot:HH\\:mm}.\n\nPosso confirmar esse agendamento?\n\nPara voltar ao início, escreva menu.");
        }
        if (transition.Intent == ConversationIntent.ListSpecialties)
        {
            return await BuildSpecialtiesAsync("Claro. Estas são algumas especialidades disponíveis:", tenantId, cancellationToken);
        }

        if (transition.Intent == ConversationIntent.CheckAvailability)
        {
            if (!context.SelectedProfessionalId.HasValue)
            {
                if (context.SelectedSpecialtyId.HasValue)
                    return await BuildProfessionalsForSpecialtyAsync(context.SelectedSpecialtyId.Value, tenantId, cancellationToken);
                return ([], "Vamos consultar os horários disponíveis. Qual especialidade ou profissional você procura?");
            }

            if (!context.SelectedSpecialtyId.HasValue)
            {
                var specialties = await GetProfessionalSpecialtiesAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                if (specialties.Count == 0)
                    return ([new("1", "professionals", 1), new("2", "mainmenu", 2)],
                        $"Não encontrei especialidades disponíveis para {context.SelectedProfessionalName ?? "esse profissional"} no momento.\n\nEscolha outro profissional ou escreva *menu* para voltar ao início.");
                if (specialties.Count > 1)
                {
                    var options = specialties.Select((specialty, index) => new ConversationOptionDefinition(
                        (index + 1).ToString(CultureInfo.InvariantCulture),
                        $"specialty:{specialty.Id}||{specialty.Name}",
                        index + 1,
                        $"specialty:{specialty.Id}")).ToList();
                    return (options,
                        $"{context.SelectedProfessionalName ?? "Esse profissional"} atende estas especialidades:\n\n" +
                        string.Join(Environment.NewLine, options.Select(option => $"{option.Key} - {option.Value.Split("||", 2)[1]}")) +
                        "\n\nQual delas você deseja consultar?");
                }
            }

            if (context.AwaitingDateSelection)
                return ([], "Claro. Qual data você prefere? Você pode escrever, por exemplo, *25/08* ou *sexta*.");

            if (context.AwaitingAvailableDaySelection || !context.SelectedDate.HasValue && !context.SelectedSlotStartsAt.HasValue)
                return await BuildAvailableDayResponseAsync(context, tenantId, cancellationToken);

            if (context.SelectedSlotStartsAt.HasValue)
            {
                var confirmationTimeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var localSlot = TimeZoneInfo.ConvertTime(context.SelectedSlotStartsAt.Value, confirmationTimeZone);
                return ([new("1", "confirm_slot", 1, "confirm_slot"), new("2", "more_slots", 2, "more_slots")],
                    $"Você escolheu:\n{context.SelectedProfessionalName ?? "o profissional selecionado"}\n{localSlot:dd/MM} às {localSlot:HH\\:mm}.\n\nPosso confirmar esse agendamento?\n\nPara voltar ao início, escreva menu.");
            }

            if (context.SelectedDate.HasValue)
            {
                var timeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var slots = await GetSlotsAsync(context.SelectedProfessionalId.Value, context.SelectedDate.Value, tenantId, cancellationToken);
                var from = context.AvailabilityCursor ?? DateTimeOffset.UtcNow;
                var futureSlots = slots.Where(slot => slot.StartsAt > from).Take(_options.MaxOptionsPerMessage).ToList();
                var unitId = context.SelectedUnitId ?? await GetProfessionalUnitIdAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var options = BuildSlotOptions(futureSlots, context.SelectedProfessionalId.Value, unitId, timeZone);
                if (futureSlots.Count > 0)
                {
                    var localDate = TimeZoneInfo.ConvertTime(futureSlots[0].StartsAt, timeZone).Date;
                    var label = FormatLocalDate(DateOnly.FromDateTime(localDate), timeZone);
                    return (options, BuildSlotText($"Perfeito. Para {label}, tenho estes horários:", futureSlots, options, timeZone, "Para consultar outros dias, escreva *outros dias*."));
                }

                var fallbackSlots = await GetNextAvailableSlotsAsync(context.SelectedProfessionalId.Value, tenantId, null, _options.MaxOptionsPerMessage, timeZone, cancellationToken);
                var fallbackUnitId = context.SelectedUnitId ?? await GetProfessionalUnitIdAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
                var fallbackOptions = BuildSlotOptions(fallbackSlots, context.SelectedProfessionalId.Value, fallbackUnitId, timeZone);
                if (fallbackSlots.Count > 0)
                    return (fallbackOptions, $"Não encontrei horários nessa data com {context.SelectedProfessionalName ?? "esse profissional"}.\n\n{BuildGroupedSlotText("Mas encontrei estes próximos horários:", fallbackSlots, fallbackOptions, timeZone)}");
                return (BuildNoAvailabilityOptions(), BuildNoAvailabilityFallback(context.SelectedProfessionalName));
            }

            var nextTimeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
            var nextSlots = await GetNextAvailableSlotsAsync(context.SelectedProfessionalId.Value, tenantId, context.AvailabilityCursor, _options.MaxOptionsPerMessage, nextTimeZone, cancellationToken);
            var nextUnitId = context.SelectedUnitId ?? await GetProfessionalUnitIdAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
            var nextOptions = BuildSlotOptions(nextSlots, context.SelectedProfessionalId.Value, nextUnitId, nextTimeZone);
            if (nextSlots.Count > 0)
                return (nextOptions, BuildGroupedSlotText($"Encontrei estes próximos horários com {context.SelectedProfessionalName ?? "o profissional escolhido"}:", nextSlots, nextOptions, nextTimeZone));
            return (BuildNoAvailabilityOptions(), BuildNoAvailabilityFallback(context.SelectedProfessionalName));
        }

        if (transition.Intent == ConversationIntent.ListProfessionals)
        {
            return await BuildProfessionalsForSpecialtyAsync(context.SelectedSpecialtyId, tenantId, cancellationToken);
        }

        return (transition.Options, null);
    }

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildProfessionalsForSpecialtyAsync(Guid? specialtyId, Guid tenantId, CancellationToken cancellationToken)
    {
        var professionalsQuery = dbContext.Professionals.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active);
        if (specialtyId.HasValue)
            professionalsQuery = professionalsQuery.Where(item => item.Specialties.Any(specialty => specialty.SpecialtyId == specialtyId.Value));
        var professionals = await professionalsQuery.OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
        var options = professionals.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"professional:{item.Id}|{item.ClinicUnitId}||{item.Name}", index + 1, $"professional:{item.Id}")).ToList();
        return (options, professionals.Count == 0
            ? "Não encontrei profissionais disponíveis para essa seleção."
            : "Perfeito. Escolha um profissional para eu consultar os horários disponíveis:");
    }

    private async Task<IReadOnlyList<(Guid Id, string Name)>> GetProfessionalSpecialtiesAsync(Guid professionalId, Guid tenantId, CancellationToken cancellationToken)
    {
        var rows = await (from link in dbContext.ProfessionalSpecialties.IgnoreQueryFilters()
                          join specialty in dbContext.Specialties.IgnoreQueryFilters() on link.SpecialtyId equals specialty.Id
                          where link.ProfessionalId == professionalId && specialty.TenantId == tenantId && specialty.Status == CatalogStatus.Active
                          orderby specialty.Name
                          select new { specialty.Id, specialty.Name })
            .Distinct()
            .ToListAsync(cancellationToken);
        return rows.Select(item => (item.Id, item.Name)).ToList();
    }

    private async Task<AvailabilityTargetResolution> ResolveAvailabilityTargetAsync(string input, Guid tenantId, CancellationToken cancellationToken)
    {
        var normalizedInput = CanonicalTarget(input);
        if (normalizedInput.Length == 0) return AvailabilityTargetResolution.NoMatch();

        var professionals = await dbContext.Professionals.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active)
            .OrderBy(item => item.Name)
            .Select(item => new TargetCandidate(item.Id, item.Name))
            .ToListAsync(cancellationToken);
        var specialties = await dbContext.Specialties.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active)
            .OrderBy(item => item.Name)
            .Select(item => new TargetCandidate(item.Id, item.Name))
            .ToListAsync(cancellationToken);

        var professionalMatches = professionals.Where(candidate => IsTargetMatch(normalizedInput, candidate.Name, false)).ToList();
        if (professionalMatches.Count == 1)
            return AvailabilityTargetResolution.Professional(professionalMatches[0]);
        if (professionalMatches.Count > 1)
            return AvailabilityTargetResolution.Ambiguous(AvailabilityTargetKind.Professional, professionalMatches);

        var specialtyMatches = specialties.Where(candidate => IsTargetMatch(normalizedInput, candidate.Name, true)).ToList();
        if (specialtyMatches.Count == 1)
            return AvailabilityTargetResolution.Specialty(specialtyMatches[0]);
        if (specialtyMatches.Count > 1)
            return AvailabilityTargetResolution.Ambiguous(AvailabilityTargetKind.Specialty, specialtyMatches);

        return AvailabilityTargetResolution.NoMatch(specialties);
    }

    private static bool IsTargetMatch(string input, string name, bool specialty)
    {
        var normalizedName = CanonicalTarget(name);
        if (input == normalizedName || normalizedName.StartsWith(input, StringComparison.Ordinal)) return true;
        if (specialty && SpecialtyAliases.TryGetValue(input, out var alias))
            return CanonicalTarget(alias) == normalizedName;
        return !specialty && normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(input, StringComparer.Ordinal);
    }

    private static string CanonicalTarget(string? value)
    {
        var normalized = ConversationIntentResolver.Normalize(value);
        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token is not "dr" and not "dra" and not "doutor" and not "doutora");
        return new string(string.Join(' ', tokens).Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character)).ToArray()).Trim();
    }

    private static readonly Dictionary<string, string> SpecialtyAliases = new(StringComparer.Ordinal)
    {
        ["cardiologista"] = "cardiologia",
        ["pediatra"] = "pediatria",
        ["clinico"] = "clinico geral",
        ["clinica geral"] = "clinico geral",
        ["ortopedista"] = "ortopedia",
        ["dermatologista"] = "dermatologia"
    };

    private static (IReadOnlyCollection<ConversationOptionDefinition> Options, string Text) BuildAvailabilityTargetFallback(AvailabilityTargetResolution resolution, int invalidAttempts)
    {
        if (resolution.Kind == AvailabilityTargetKind.Ambiguous)
        {
            var options = resolution.Candidates.Select((candidate, index) => new ConversationOptionDefinition(
                (index + 1).ToString(CultureInfo.InvariantCulture),
                resolution.CandidateKind == AvailabilityTargetKind.Specialty
                    ? $"specialty:{candidate.Id}||{candidate.Name}"
                    : $"professional:{candidate.Id}||{candidate.Name}",
                index + 1,
                resolution.CandidateKind == AvailabilityTargetKind.Specialty ? $"specialty:{candidate.Id}" : $"professional:{candidate.Id}"))
                .ToArray();
            var subject = resolution.CandidateKind == AvailabilityTargetKind.Specialty ? "especialidades" : "profissionais";
            return (options, $"Encontrei mais de uma opção em {subject}:\n\n" + string.Join(Environment.NewLine, options.Select(option => $"{option.Key} - {option.Value.Split("||", 2)[1]}")) + "\n\nQual delas você procura?");
        }

        if (invalidAttempts > 0)
        {
            var retryOptions = new[]
            {
                new ConversationOptionDefinition("1", "specialties", 1),
                new ConversationOptionDefinition("2", "professionals", 2),
                new ConversationOptionDefinition("3", "human", 3)
            };
            return (retryOptions, "Ainda não consegui localizar essa opção. Como você prefere consultar?\n\n1 - Por especialidade\n2 - Por profissional\n3 - Falar com atendente");
        }

        var available = resolution.Candidates.Take(6).Select((candidate, index) => new ConversationOptionDefinition(
            (index + 1).ToString(CultureInfo.InvariantCulture),
            $"specialty:{candidate.Id}||{candidate.Name}", index + 1, $"specialty:{candidate.Id}")).ToList();
        available.Add(new ConversationOptionDefinition((available.Count + 1).ToString(CultureInfo.InvariantCulture), "professionals", available.Count + 1));
        var text = available.Count == 1
            ? "Não encontrei essa especialidade ou profissional por aqui. Você pode escrever outro nome ou voltar ao menu."
            : "Não encontrei essa especialidade ou profissional por aqui.\n\nEstas são algumas opções disponíveis:\n\n" + string.Join(Environment.NewLine, available.Take(available.Count - 1).Select(option => $"{option.Key} - {option.Value.Split("||", 2)[1]}")) + $"\n{available[^1].Key} - Ver profissionais";
        return (available, text);
    }

    private enum AvailabilityTargetKind { NoMatch, Specialty, Professional, Ambiguous }
    private sealed record TargetCandidate(Guid Id, string Name);
    private sealed record AvailabilityTargetResolution(AvailabilityTargetKind Kind, Guid? Id, string? Name, IReadOnlyList<TargetCandidate> Candidates, AvailabilityTargetKind CandidateKind)
    {
        public static AvailabilityTargetResolution Professional(TargetCandidate candidate) => new(AvailabilityTargetKind.Professional, candidate.Id, candidate.Name, [], AvailabilityTargetKind.Professional);
        public static AvailabilityTargetResolution Specialty(TargetCandidate candidate) => new(AvailabilityTargetKind.Specialty, candidate.Id, candidate.Name, [], AvailabilityTargetKind.Specialty);
        public static AvailabilityTargetResolution Ambiguous(AvailabilityTargetKind kind, IReadOnlyList<TargetCandidate> candidates) => new(AvailabilityTargetKind.Ambiguous, null, null, candidates, kind);
        public static AvailabilityTargetResolution NoMatch(IReadOnlyList<TargetCandidate>? candidates = null) => new(AvailabilityTargetKind.NoMatch, null, null, candidates ?? [], AvailabilityTargetKind.NoMatch);
    }

    private static List<ConversationOptionDefinition> BuildSlotOptions(IReadOnlyList<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)> slots, Guid professionalId, Guid? unitId, TimeZoneInfo timeZone) =>
        slots.Select((slot, index) =>
        {
            var machineValue = $"slot:{professionalId}|{unitId?.ToString() ?? string.Empty}|{slot.StartsAt:O}|{slot.EndsAt:O}||{TimeZoneInfo.ConvertTime(slot.StartsAt, timeZone):HH\\:mm}";
            return new ConversationOptionDefinition(
                (index + 1).ToString(CultureInfo.InvariantCulture),
                machineValue,
                index + 1,
                $"slot:{CreateStableActionToken(professionalId, unitId, slot.StartsAt, slot.EndsAt)}");
        }).ToList();

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildAvailableDayResponseAsync(ConversationContext context, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!context.SelectedProfessionalId.HasValue)
            return ([], "Qual profissional você prefere?");

        var timeZone = await GetClinicTimeZoneAsync(context.SelectedProfessionalId.Value, tenantId, cancellationToken);
        var slots = await GetNextAvailableSlotsAsync(context.SelectedProfessionalId.Value, tenantId, null,
            Math.Max(_options.MaxAvailableDaysPerMessage * _options.MaxOptionsPerMessage, 24), timeZone, cancellationToken);
        var days = slots
            .Where(slot => slot.StartsAt > DateTimeOffset.UtcNow)
            .GroupBy(slot => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(slot.StartsAt, timeZone).Date))
            .OrderBy(group => group.Key)
            .Take(_options.MaxAvailableDaysPerMessage)
            .Select((group, index) => new ConversationOptionDefinition(
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"day:{group.Key:yyyy-MM-dd}||{FormatLocalDate(group.Key, timeZone)}",
                index + 1,
                $"day:{group.Key:yyyy-MM-dd}"))
            .ToList();

        if (days.Count == 0)
            return (BuildNoAvailabilityOptions(), BuildNoAvailabilityFallback(context.SelectedProfessionalName));

        var professional = context.SelectedProfessionalName ?? "o profissional escolhido";
        return (days, $"Encontrei horários disponíveis com {professional}.\n\nQual dia fica melhor para você?\n\n" +
            string.Join(Environment.NewLine, days.Select(option => $"{option.Key} - {option.Value.Split("||", 2)[1]}")) +
            "\n\nSe quiser informar outra data, escreva *outra data*.");
    }

    private static string CreateStableActionToken(Guid professionalId, Guid? unitId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        var source = $"{professionalId:N}|{unitId?.ToString("N", CultureInfo.InvariantCulture) ?? string.Empty}|{startsAt.ToUniversalTime():O}|{endsAt.ToUniversalTime():O}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(hash[..10]).ToLowerInvariant();
    }

    private static string BuildGroupedSlotText(string heading, IReadOnlyList<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)> slots, List<ConversationOptionDefinition> options, TimeZoneInfo timeZone)
    {
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).Date;
        var lines = new List<string> { heading };
        foreach (var group in slots.Select((slot, index) => (slot, option: options[index])).GroupBy(item => TimeZoneInfo.ConvertTime(item.slot.StartsAt, timeZone).Date))
        {
            var label = group.Key == localNow ? "Hoje" : group.Key == localNow.AddDays(1) ? "Amanhã" : group.Key.ToString("dddd, dd/MM", CultureInfo.GetCultureInfo("pt-BR"));
            lines.Add($"\n*{Capitalize(label)}*");
            lines.AddRange(group.Select(item => $"{item.option.Key} - {TimeZoneInfo.ConvertTime(item.slot.StartsAt, timeZone):HH\\:mm}"));
        }
        lines.Add("\nEscolha o número do horário que preferir. Se quiser consultar outras datas, escreva *mais horários*.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildSlotText(string heading, IReadOnlyList<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)> slots, List<ConversationOptionDefinition> options, TimeZoneInfo timeZone, string footer)
    {
        var lines = new List<string> { heading };
        lines.AddRange(options.Select(option => $"{option.Key} - {DisplaySlotTime(option.Value, timeZone)}"));
        lines.Add($"\nEscolha o horário que preferir. {footer}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string DisplaySlotTime(string value, TimeZoneInfo timeZone)
    {
        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        var startIndex = parts.Length >= 4 ? 2 : 1;
        return startIndex < parts.Length && DateTimeOffset.TryParse(parts[startIndex], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var startsAt)
            ? $"{TimeZoneInfo.ConvertTime(startsAt, timeZone):HH\\:mm}"
            : "Horário disponível";
    }

    private static string FormatLocalDate(DateOnly date, TimeZoneInfo timeZone)
    {
        var localToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).Date);
        var label = date == localToday ? $"Hoje, {date:dd/MM}" : date == localToday.AddDays(1) ? $"Amanhã, {date:dd/MM}" : $"{Capitalize(date.ToDateTime(TimeOnly.MinValue).ToString("dddd", CultureInfo.GetCultureInfo("pt-BR")))}, {date:dd/MM}";
        return label;
    }

    private static string Capitalize(string value) => string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static string BuildNoAvailabilityFallback(string? professionalName) =>
        $"Não encontrei horários próximos com {professionalName ?? "esse profissional"}.\n\n" +
        "Posso mostrar outros profissionais, escolher outra especialidade, consultar uma data específica ou encaminhar você para um atendente.\n\n" +
        "1 - Outros profissionais\n2 - Outra especialidade\n3 - Informar uma data\n4 - Falar com atendente\n5 - Voltar ao menu";

    private static List<ConversationOptionDefinition> BuildNoAvailabilityOptions() =>
        [new("1", "professionals", 1), new("2", "specialties", 2), new("3", "date_request", 3), new("4", "human", 4), new("5", "mainmenu", 5)];

    private async Task<List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>> GetNextAvailableSlotsAsync(Guid professionalId, Guid tenantId, DateTimeOffset? cursor, int maximum, TimeZoneInfo timeZone, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var from = cursor.HasValue && cursor.Value > now ? cursor.Value.AddSeconds(1) : now;
        var localFrom = TimeZoneInfo.ConvertTime(from, timeZone);
        var firstDate = DateOnly.FromDateTime(localFrom.DateTime.Date);
        var lastDate = firstDate.AddDays(Math.Max(1, _options.AvailabilitySearchDays));
        var startUtc = ToUtc(firstDate, TimeOnly.MinValue, timeZone);
        var endUtc = ToUtc(lastDate, TimeOnly.MaxValue, timeZone);
        var rules = await dbContext.AvailabilityRules.IgnoreQueryFilters()
            .Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Active)
            .ToListAsync(cancellationToken);
        var busy = await dbContext.Appointments.IgnoreQueryFilters()
            .Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Status != AppointmentStatus.Cancelled && item.StartsAt < endUtc && item.EndsAt > startUtc)
            .Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var blocks = await dbContext.ScheduleBlocks.IgnoreQueryFilters()
            .Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endUtc && item.EndsAt > startUtc)
            .Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var vacations = await dbContext.ProfessionalVacations.IgnoreQueryFilters()
            .Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endUtc && item.EndsAt > startUtc)
            .Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);

        var slots = new List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>();
        for (var date = firstDate; date <= lastDate && slots.Count < maximum * 2; date = date.AddDays(1))
        {
            foreach (var rule in rules.Where(item => item.DayOfWeek == date.DayOfWeek))
            {
                for (var start = ToUtc(date, rule.StartTime, timeZone); start.AddMinutes(rule.SlotDurationMinutes) <= ToUtc(date, rule.EndTime, timeZone); start = start.AddMinutes(rule.SlotDurationMinutes))
                {
                    var end = start.AddMinutes(rule.SlotDurationMinutes);
                    if (start < from || busy.Concat(blocks).Concat(vacations).Any(item => item.StartsAt < end && item.EndsAt > start)) continue;
                    slots.Add((start, end));
                }
            }
        }
        return slots.OrderBy(slot => slot.StartsAt).Take(maximum).ToList();
    }

    private async Task<TimeZoneInfo> GetClinicTimeZoneAsync(Guid professionalId, Guid tenantId, CancellationToken cancellationToken)
    {
        var timeZoneId = await (from professional in dbContext.Professionals.IgnoreQueryFilters()
                                join unit in dbContext.ClinicUnits.IgnoreQueryFilters() on professional.ClinicUnitId equals unit.Id
                                join clinic in dbContext.Clinics.IgnoreQueryFilters() on unit.ClinicId equals clinic.Id
                                where professional.TenantId == tenantId && professional.Id == professionalId
                                select clinic.TimeZone).SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(timeZoneId)) return TimeZoneInfo.Utc;
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }

    private Task<Guid?> GetProfessionalUnitIdAsync(Guid professionalId, Guid tenantId, CancellationToken cancellationToken) =>
        dbContext.Professionals.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Id == professionalId)
            .Select(item => (Guid?)item.ClinicUnitId)
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildAppointmentOperationResponseAsync(ConversationIntent intent, ConversationContext context, Guid tenantId, Guid patientId, CancellationToken cancellationToken)
    {
        if (context.SelectedAppointmentId.HasValue)
        {
            var selected = await (from appointment in dbContext.Appointments.IgnoreQueryFilters()
                                  join professional in dbContext.Professionals.IgnoreQueryFilters() on appointment.ProfessionalId equals professional.Id
                                  join specialty in dbContext.Specialties.IgnoreQueryFilters() on appointment.SpecialtyId equals specialty.Id
                                  where appointment.TenantId == tenantId && appointment.PatientId == patientId && appointment.Id == context.SelectedAppointmentId.Value
                                  select new { appointment.StartsAt, appointment.EndsAt, appointment.Status, appointment.Version, appointment.ProfessionalId, appointment.SpecialtyId, appointment.ClinicUnitId, ProfessionalName = professional.Name, SpecialtyName = specialty.Name })
                .SingleOrDefaultAsync(cancellationToken);
            if (selected is null) return ([], "Não encontrei essa consulta. Vamos tentar novamente?");
            if (intent == ConversationIntent.RescheduleAppointment)
            {
                var timeZone = await GetClinicTimeZoneAsync(selected.ProfessionalId, tenantId, cancellationToken);
                var currentLocalStart = TimeZoneInfo.ConvertTime(selected.StartsAt, timeZone);
                if (!context.SelectedDate.HasValue)
                {
                    var dayContext = context with
                    {
                        SelectedProfessionalId = selected.ProfessionalId,
                        SelectedProfessionalName = selected.ProfessionalName,
                        SelectedSpecialtyId = selected.SpecialtyId,
                        SelectedSpecialtyName = selected.SpecialtyName,
                        SelectedUnitId = selected.ClinicUnitId,
                        AwaitingAvailableDaySelection = true,
                        CurrentIntent = ConversationIntent.RescheduleAppointment
                    };
                    var days = await BuildAvailableDayResponseAsync(dayContext, tenantId, cancellationToken);
                    return days.Options.Count == 0
                        ? ([], $"Não encontrei novos horários para {selected.ProfessionalName}. Você pode escrever *menu* para voltar ao início.")
                        : (days.Options, $"Sua consulta atual é em {selected.ProfessionalName}, {currentLocalStart:dd/MM} às {currentLocalStart:HH\\:mm}.\n\n{days.Text}");
                }
                if (!context.SelectedSlotStartsAt.HasValue)
                {
                    var slots = (await GetSlotsAsync(selected.ProfessionalId, context.SelectedDate.Value, tenantId, cancellationToken))
                        .Where(slot => slot.StartsAt != selected.StartsAt || slot.EndsAt != selected.EndsAt)
                        .Take(_options.MaxOptionsPerMessage)
                        .ToList();
                    var unitId = context.SelectedUnitId ?? selected.ClinicUnitId;
                    var rescheduleOptions = BuildSlotOptions(slots, selected.ProfessionalId, unitId, timeZone);
                    var localDate = context.SelectedDate.Value.ToDateTime(TimeOnly.MinValue);
                    var dateLabel = FormatLocalDate(DateOnly.FromDateTime(localDate), timeZone);
                    return (rescheduleOptions, slots.Count == 0 ? "Não encontrei horários nessa data. Você pode escrever *outra data* ou *menu*." : BuildSlotText($"Para {dateLabel}, encontrei estes horários:", slots, rescheduleOptions, timeZone, "Para consultar outra data, escreva *outra data*."));
                }
                var newLocalStart = TimeZoneInfo.ConvertTime(context.SelectedSlotStartsAt.Value, timeZone);
                var confirmationOptions = new[] { new ConversationOptionDefinition("1", "confirm_reschedule", 1, "confirm_reschedule"), new ConversationOptionDefinition("2", "more_slots", 2, "more_slots") };
                return (confirmationOptions, $"Você quer alterar esta consulta?\n\nAtual: {selected.ProfessionalName} · {selected.SpecialtyName}\n{currentLocalStart:dd/MM} às {currentLocalStart:HH\\:mm}\n\nPara: {selected.ProfessionalName}\n{newLocalStart:dd/MM} às {newLocalStart:HH\\:mm}\n\nPosso confirmar o reagendamento?");
            }
            var action = intent == ConversationIntent.CancelAppointment ? "cancelar" : "confirmar sua presença em";
            var selectedTimeZone = await GetClinicTimeZoneAsync(selected.ProfessionalId, tenantId, cancellationToken);
            var selectedLocal = TimeZoneInfo.ConvertTime(selected.StartsAt, selectedTimeZone);
            return ([], $"Encontrei sua consulta em {selectedLocal:dd/MM} às {selectedLocal:HH\\:mm}. Deseja {action}?");
        }

        var now = DateTimeOffset.UtcNow;
        var appointments = await (from appointment in dbContext.Appointments.IgnoreQueryFilters()
                                  join professional in dbContext.Professionals.IgnoreQueryFilters() on appointment.ProfessionalId equals professional.Id
                                  join specialty in dbContext.Specialties.IgnoreQueryFilters() on appointment.SpecialtyId equals specialty.Id
                                  join unit in dbContext.ClinicUnits.IgnoreQueryFilters() on appointment.ClinicUnitId equals unit.Id
                                  join clinic in dbContext.Clinics.IgnoreQueryFilters() on unit.ClinicId equals clinic.Id
                                  where appointment.TenantId == tenantId && appointment.PatientId == patientId && appointment.StartsAt >= now
                                      && (intent == ConversationIntent.CancelAppointment
                                          ? appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed
                                          : intent == ConversationIntent.RescheduleAppointment
                                              ? appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed
                                              : appointment.Status == AppointmentStatus.Pending)
                                  orderby appointment.StartsAt
                                  select new { appointment.Id, appointment.Version, appointment.StartsAt, appointment.ProfessionalId, ProfessionalName = professional.Name, SpecialtyName = specialty.Name, TimeZoneId = clinic.TimeZone })
            .Take(_options.MaxOptionsPerMessage)
            .ToListAsync(cancellationToken);
        var options = appointments.Select((item, index) =>
        {
            var localStart = TimeZoneInfo.ConvertTime(item.StartsAt, ResolveTimeZone(item.TimeZoneId));
            return new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"appointment:{item.Id}|{item.Version}||{item.ProfessionalName} · {item.SpecialtyName} · {localStart:dd/MM} às {localStart:HH\\:mm}", index + 1);
        }).ToList();
        var text = intent switch { ConversationIntent.CancelAppointment => "Encontrei estas consultas. Qual você deseja cancelar?", ConversationIntent.RescheduleAppointment => "Encontrei estas consultas marcadas para você. Qual deseja reagendar?", _ => "Encontrei estas consultas pendentes. Qual você deseja confirmar?" };
        return (options, appointments.Count == 0 ? intent == ConversationIntent.RescheduleAppointment ? "Não encontrei consultas futuras para reagendar. Posso ajudar a agendar uma nova consulta ou voltar ao menu." : "Não encontrei consultas futuras para essa operação." : text);
    }

    private async Task<ConversationContext> EnrichRescheduleContextAsync(ConversationContext context, Guid tenantId, Guid patientId, CancellationToken cancellationToken)
    {
        if (!context.SelectedAppointmentId.HasValue)
            return context;

        var selected = await (from appointment in dbContext.Appointments.IgnoreQueryFilters()
                              join professional in dbContext.Professionals.IgnoreQueryFilters() on appointment.ProfessionalId equals professional.Id
                              join specialty in dbContext.Specialties.IgnoreQueryFilters() on appointment.SpecialtyId equals specialty.Id
                              where appointment.TenantId == tenantId
                                    && appointment.PatientId == patientId
                                    && appointment.Id == context.SelectedAppointmentId.Value
                              select new
                              {
                                  appointment.ProfessionalId,
                                  appointment.SpecialtyId,
                                  appointment.ClinicUnitId,
                                  appointment.Version,
                                  appointment.Status,
                                  ProfessionalName = professional.Name,
                                  SpecialtyName = specialty.Name
                              }).SingleOrDefaultAsync(cancellationToken);

        if (selected is null || (selected.Status != AppointmentStatus.Pending && selected.Status != AppointmentStatus.Confirmed))
            return context;

        return context with
        {
            SelectedProfessionalId = selected.ProfessionalId,
            SelectedProfessionalName = selected.ProfessionalName,
            SelectedSpecialtyId = selected.SpecialtyId,
            SelectedSpecialtyName = selected.SpecialtyName,
            SelectedUnitId = selected.ClinicUnitId,
            SelectedAppointmentVersion = selected.Version
        };
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildSpecialtiesAsync(string text, Guid tenantId, CancellationToken cancellationToken)
    {
        var specialties = await dbContext.Specialties.IgnoreQueryFilters().AsNoTracking().Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active).OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
        var options = specialties.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"specialty:{item.Id}||{item.Name}", index + 1, $"specialty:{item.Id}")).ToList();
        return (options, specialties.Count == 0 ? "No momento, não encontrei especialidades disponíveis." : text);
    }

    private async Task<List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>> GetSlotsAsync(Guid professionalId, DateOnly date, Guid tenantId, CancellationToken cancellationToken)
    {
        var timeZone = await GetClinicTimeZoneAsync(professionalId, tenantId, cancellationToken);
        var rules = await dbContext.AvailabilityRules.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Active && item.DayOfWeek == date.DayOfWeek).ToListAsync(cancellationToken);
        var startOfDay = ToUtc(date, TimeOnly.MinValue, timeZone);
        var endOfDay = startOfDay.AddDays(1);
        var busy = await dbContext.Appointments.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Status != ClinicAssistant.Domain.Scheduling.AppointmentStatus.Cancelled && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var blocks = await dbContext.ScheduleBlocks.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var vacations = await dbContext.ProfessionalVacations.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var slots = new List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>();
        foreach (var rule in rules)
            for (var start = ToUtc(date, rule.StartTime, timeZone); start.AddMinutes(rule.SlotDurationMinutes) <= ToUtc(date, rule.EndTime, timeZone); start = start.AddMinutes(rule.SlotDurationMinutes))
            {
                var end = start.AddMinutes(rule.SlotDurationMinutes);
                if (!busy.Concat(blocks).Concat(vacations).Any(item => item.StartsAt < end && item.EndsAt > start)) slots.Add((start, end));
            }
        return slots;
    }

    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timeZone), TimeSpan.Zero);
    }

    private async Task<(bool Success, string Message)> TryCreateScheduledAppointmentAsync(ProcessConversationMessageCommand command, Guid patientId, ConversationContext context, CancellationToken cancellationToken)
    {
        if (!context.SelectedSpecialtyId.HasValue || !context.SelectedProfessionalId.HasValue || !context.SelectedSlotStartsAt.HasValue || !context.SelectedSlotEndsAt.HasValue)
            return (false, string.Empty);

        var idempotencyKey = $"conversation:{command.ConversationId:N}:schedule:{command.ConversationMessageId:N}";
        var scope = $"conversation.schedule:{command.ConversationId:N}";
        var prior = await dbContext.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(item => item.Scope == scope && item.Key == idempotencyKey, cancellationToken);
        if (prior is not null) return (true, "Consulta agendada ✅\n\nSua solicitação já havia sido confirmada anteriormente.");

        var professional = await dbContext.Professionals.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == command.TenantId && item.Id == context.SelectedProfessionalId.Value && item.Status == CatalogStatus.Active, cancellationToken);
        var validSpecialty = professional is not null && await dbContext.ProfessionalSpecialties.IgnoreQueryFilters().AnyAsync(item => item.ProfessionalId == professional.Id && item.SpecialtyId == context.SelectedSpecialtyId.Value, cancellationToken);
        if (professional is null || !validSpecialty) return (false, string.Empty);
        if (context.SelectedUnitId.HasValue && context.SelectedUnitId.Value != professional.ClinicUnitId) return (false, "Não consegui validar a unidade deste horário. Vou consultar outras opções para você.");

        var startsAt = context.SelectedSlotStartsAt.Value.ToUniversalTime();
        var endsAt = context.SelectedSlotEndsAt.Value.ToUniversalTime();
        var conflict = await dbContext.Appointments.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.Status != AppointmentStatus.Cancelled && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken)
            || await dbContext.ScheduleBlocks.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken)
            || await dbContext.ProfessionalVacations.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken);
        if (conflict) return (false, "Esse horário acabou de ficar indisponível. Vou consultar outras opções para você.");

        var appointment = new Appointment(command.TenantId, professional.ClinicUnitId, professional.Id, context.SelectedSpecialtyId.Value, patientId, startsAt, endsAt, AppointmentSource.WhatsApp, "Agendamento iniciado pela conversa WhatsApp.");
        var response = new AppointmentResponse(appointment.Id, patientId, professional.Id, startsAt, endsAt, appointment.Status.ToString());
        dbContext.AddRange(appointment, new AuditRecord(command.TenantId, null, "appointment.created", "Appointment", appointment.Id, "Succeeded", "Appointment created through conversational WhatsApp flow."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response)));
        var timeZone = await GetClinicTimeZoneAsync(professional.Id, command.TenantId, cancellationToken);
        var localStart = TimeZoneInfo.ConvertTime(startsAt, timeZone);
        return (true, $"Consulta agendada ✅\n\n{professional.Name}\n{localStart:dd/MM} às {localStart:HH\\:mm}\n\nSe precisar, você pode escrever *reagendar*, *cancelar* ou *menu*.");
    }

    private async Task<(bool Success, string Message)> TryCancelAppointmentAsync(ProcessConversationMessageCommand command, Guid patientId, Guid appointmentId, CancellationToken cancellationToken)
    {
        var key = $"conversation:{command.ConversationId:N}:cancel:{appointmentId:N}:{command.ConversationMessageId:N}";
        var scope = $"conversation.cancel:{command.ConversationId:N}";
        if (await dbContext.IdempotencyRecords.AsNoTracking().AnyAsync(item => item.Scope == scope && item.Key == key, cancellationToken)) return (true, "Essa operação já havia sido concluída.");
        var appointment = await dbContext.Appointments.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == command.TenantId && item.PatientId == patientId && item.Id == appointmentId, cancellationToken);
        if (appointment is null || appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed) return (false, "Essa consulta não pode mais ser cancelada.");
        appointment.Cancel("Cancelamento confirmado pelo paciente via WhatsApp.");
        var response = new AppointmentResponse(appointment.Id, patientId, appointment.ProfessionalId, appointment.StartsAt, appointment.EndsAt, appointment.Status.ToString());
        dbContext.AddRange(new AuditRecord(command.TenantId, null, "appointment.cancelled", "Appointment", appointment.Id, "Succeeded", "Appointment cancelled through conversational WhatsApp flow."), new IdempotencyRecord(scope, key, JsonSerializer.Serialize(response)));
        return (true, $"Consulta cancelada ✅\n\n{appointment.StartsAt:dd/MM} às {appointment.StartsAt:HH\\:mm}. Se precisar, é só escrever *menu*.");
    }

    private async Task<(bool Success, string Message)> TryConfirmAppointmentAsync(ProcessConversationMessageCommand command, Guid patientId, Guid appointmentId, CancellationToken cancellationToken)
    {
        var key = $"conversation:{command.ConversationId:N}:confirm:{appointmentId:N}:{command.ConversationMessageId:N}";
        var scope = $"conversation.confirm:{command.ConversationId:N}";
        if (await dbContext.IdempotencyRecords.AsNoTracking().AnyAsync(item => item.Scope == scope && item.Key == key, cancellationToken)) return (true, "Essa confirmação já havia sido concluída.");
        var appointment = await dbContext.Appointments.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == command.TenantId && item.PatientId == patientId && item.Id == appointmentId, cancellationToken);
        if (appointment is null) return (false, "Não encontrei essa consulta.");
        if (appointment.Status == AppointmentStatus.Confirmed) return (true, "Essa consulta já está confirmada.");
        if (appointment.Status != AppointmentStatus.Pending) return (false, "Essa consulta não está disponível para confirmação.");
        appointment.Confirm();
        var response = new AppointmentResponse(appointment.Id, patientId, appointment.ProfessionalId, appointment.StartsAt, appointment.EndsAt, appointment.Status.ToString());
        dbContext.AddRange(new AuditRecord(command.TenantId, null, "appointment.confirmed", "Appointment", appointment.Id, "Succeeded", "Appointment confirmed through conversational WhatsApp flow."), new IdempotencyRecord(scope, key, JsonSerializer.Serialize(response)));
        return (true, $"Presença confirmada ✅\n\nEsperamos você no dia {appointment.StartsAt:dd/MM} às {appointment.StartsAt:HH\\:mm}.");
    }

    private async Task<(bool Success, string Message)> TryRescheduleAppointmentAsync(ProcessConversationMessageCommand command, Guid patientId, ConversationContext context, CancellationToken cancellationToken)
    {
        if (!context.SelectedAppointmentId.HasValue || !context.SelectedSlotStartsAt.HasValue || !context.SelectedSlotEndsAt.HasValue) return (false, "");
        var key = $"conversation:{command.ConversationId:N}:reschedule:{context.SelectedAppointmentId.Value:N}:{command.ConversationMessageId:N}";
        var scope = $"conversation.reschedule:{command.ConversationId:N}";
        if (await dbContext.IdempotencyRecords.AsNoTracking().AnyAsync(item => item.Scope == scope && item.Key == key, cancellationToken)) return (true, "Esse reagendamento já havia sido concluído.");
        var appointment = await dbContext.Appointments.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == command.TenantId && item.PatientId == patientId && item.Id == context.SelectedAppointmentId.Value, cancellationToken);
        if (appointment is null || appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.Rescheduled) return (false, "Essa consulta não pode mais ser reagendada.");
        if (context.SelectedAppointmentVersion.HasValue && appointment.Version != context.SelectedAppointmentVersion.Value) return (false, "Essa consulta foi alterada por outro atendimento. Vou atualizar as opções para você.");
        var start = context.SelectedSlotStartsAt.Value.ToUniversalTime();
        var end = context.SelectedSlotEndsAt.Value.ToUniversalTime();
        var conflict = await dbContext.Appointments.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == appointment.ProfessionalId && item.Id != appointment.Id && item.Status != AppointmentStatus.Cancelled && item.Status != AppointmentStatus.Rescheduled && item.StartsAt < end && item.EndsAt > start, cancellationToken)
            || await dbContext.ScheduleBlocks.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == appointment.ProfessionalId && item.StartsAt < end && item.EndsAt > start, cancellationToken)
            || await dbContext.ProfessionalVacations.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == appointment.ProfessionalId && item.StartsAt < end && item.EndsAt > start, cancellationToken);
        if (conflict) return (false, "Esse horário acabou de ficar indisponível. Vou manter sua consulta atual.");
        appointment.MarkRescheduled();
        var replacement = new Appointment(command.TenantId, appointment.ClinicUnitId, appointment.ProfessionalId, appointment.SpecialtyId, appointment.PatientId, start, end, appointment.Source, "Reagendamento confirmado pela conversa WhatsApp.");
        var response = new RescheduleAppointmentResponse(new AppointmentResponse(appointment.Id, patientId, appointment.ProfessionalId, appointment.StartsAt, appointment.EndsAt, appointment.Status.ToString()), new AppointmentResponse(replacement.Id, patientId, replacement.ProfessionalId, replacement.StartsAt, replacement.EndsAt, replacement.Status.ToString()), false);
        dbContext.AddRange(replacement, new AuditRecord(command.TenantId, null, "appointment.rescheduled", "Appointment", appointment.Id, "Succeeded", "Appointment rescheduled through conversational WhatsApp flow."), new IdempotencyRecord(scope, key, JsonSerializer.Serialize(response)));
        return (true, $"Consulta reagendada ✅\n\nNovo horário: {start:dd/MM} às {start:HH\\:mm}.");
    }
}
