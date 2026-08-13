using System.Text.Json;
using System.Globalization;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.Scheduling;
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

            var stateContext = ReadContext(state.ContextJson, state.Intent, state.FlowState, state.InvalidAttempts);
            var previousStep = state.FlowState;
            var existingOptions = await dbContext.ConversationOptions.IgnoreQueryFilters()
                .Where(item => item.TenantId == command.TenantId && item.ConversationStateId == state.Id && item.ExpiresAt > (incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow))
                .OrderBy(item => item.DisplayOrder)
                .Select(item => new ConversationOptionDefinition(item.Key, item.Value, item.DisplayOrder))
                .ToListAsync(cancellationToken);
            stateContext = ApplyContextualDate(ApplyContextualSelection(stateContext, incomingMessage.ContentSanitized, existingOptions), incomingMessage.ContentSanitized, incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow);
            var transition = stateMachine.Transition(new(incomingMessage.ContentSanitized, state.FlowState, state.Status, state.Intent, state.InvalidAttempts, state.ExpiresAt, incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow, existingOptions, stateContext));
            ConversationTelemetry.RecordIntent(transition.Intent, transition.FlowState);
            if (transition.Intent is ConversationIntent.Unknown or ConversationIntent.Unsupported) ConversationTelemetry.InvalidInput.Add(1);
            if (transition.Action == ConversationAction.Handoff) ConversationTelemetry.Handoff.Add(1);
            if (transition.ResponseKey == "conversation.expired") ConversationTelemetry.FlowTimeout.Add(1);
            if (transition.Action == ConversationAction.CancelFlow) ConversationTelemetry.FlowAbandoned.Add(1);
            if (transition.Action == ConversationAction.CloseConversation) ConversationTelemetry.FlowCompleted.Add(1);
            if (state.FlowState == ConversationFlowState.Initial && transition.FlowState != ConversationFlowState.Initial) ConversationTelemetry.FlowStarted.Add(1);
            var (responseOptions, responseText) = await BuildInformationalResponseAsync(transition, stateContext, command.TenantId, patient.Id, cancellationToken);
            if (transition.Intent == ConversationIntent.ConfirmAppointment && stateContext.PendingConfirmation && stateContext.CurrentIntent == ConversationIntent.ScheduleAppointment)
            {
                var schedulingResult = await TryCreateScheduledAppointmentAsync(command, patient.Id, stateContext, cancellationToken);
                responseText = schedulingResult.Success ? schedulingResult.Message : "Não consegui concluir o agendamento desse horário. Podemos tentar outra opção?";
                if (schedulingResult.Success)
                    stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedSpecialtyId = null, SelectedProfessionalId = null, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
            }
            else if (transition.Intent is ConversationIntent.CancelAppointment or ConversationIntent.ConfirmAppointment && stateContext.PendingConfirmation && stateContext.SelectedAppointmentId.HasValue)
            {
                var operation = transition.Intent == ConversationIntent.CancelAppointment
                    ? await TryCancelAppointmentAsync(command, patient.Id, stateContext.SelectedAppointmentId.Value, cancellationToken)
                    : await TryConfirmAppointmentAsync(command, patient.Id, stateContext.SelectedAppointmentId.Value, cancellationToken);
                responseText = operation.Message;
                if (operation.Success) stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedAppointmentId = null };
            }
            else if (transition.Intent == ConversationIntent.RescheduleAppointment && stateContext.PendingConfirmation && stateContext.SelectedAppointmentId.HasValue)
            {
                var operation = await TryRescheduleAppointmentAsync(command, patient.Id, stateContext, cancellationToken);
                responseText = operation.Message;
                if (operation.Success) stateContext = stateContext with { PendingConfirmation = false, CurrentIntent = ConversationIntent.Unknown, SelectedAppointmentId = null, SelectedDate = null, SelectedSlotStartsAt = null, SelectedSlotEndsAt = null };
            }
            var response = responseComposer.Compose(new(transition.ResponseKey, responseOptions, _options.DefaultLanguage, responseText));
            if (response.Text.Length > _options.MaxMessageLength) return ConversationOrchestrationResult.Rejected;

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.StateExpirationMinutes);
            state.Apply(transition.FlowState, transition.Status, transition.Intent, transition.InvalidAttempts, expiresAt);
            state.UpdateContext(JsonSerializer.Serialize(stateContext with
            {
                CurrentIntent = transition.Intent,
                CurrentStep = transition.FlowState,
                PreviousStep = previousStep,
                InvalidAttemptCount = transition.InvalidAttempts,
                LastUserMessage = incomingMessage.ContentSanitized,
                LastBotMessage = response.Text,
                LastInteractionAt = incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow,
                FlowStartedAt = stateContext.FlowStartedAt ?? incomingMessage.ReceivedAt ?? DateTimeOffset.UtcNow
            }));
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

            var optionsToReplace = await dbContext.ConversationOptions.IgnoreQueryFilters().Where(item => item.TenantId == command.TenantId && item.ConversationStateId == state.Id).ToListAsync(cancellationToken);
            dbContext.ConversationOptions.RemoveRange(optionsToReplace);
            foreach (var option in responseOptions.Take(_options.MaxOptionsPerMessage))
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

    private static ConversationContext ReadContext(string json, ConversationIntent intent, ConversationFlowState step, int invalidAttempts)
    {
        try { return JsonSerializer.Deserialize<ConversationContext>(json) ?? new(intent, step, null, invalidAttempts); }
        catch (JsonException) { return new(intent, step, null, invalidAttempts); }
    }

    private static ConversationContext ApplyContextualSelection(ConversationContext context, string? message, IReadOnlyCollection<ConversationOptionDefinition> options)
    {
        if (!int.TryParse(message?.Trim(), out var number)) return context;
        var value = options.SingleOrDefault(item => item.Key == number.ToString(CultureInfo.InvariantCulture))?.Value;
        if (value is null) return context;
        var machineValue = value.Split("||", 2, StringSplitOptions.None)[0];
        if (machineValue.StartsWith("specialty:", StringComparison.Ordinal) && Guid.TryParse(machineValue[10..], out var specialtyId)) return context with { SelectedSpecialtyId = specialtyId };
        if (machineValue.StartsWith("professional:", StringComparison.Ordinal) && Guid.TryParse(machineValue[13..], out var professionalId)) return context with { SelectedProfessionalId = professionalId };
        if (machineValue.StartsWith("appointment:", StringComparison.Ordinal))
        {
            var appointmentParts = machineValue[12..].Split('|', StringSplitOptions.TrimEntries);
            if (appointmentParts.Length > 0 && Guid.TryParse(appointmentParts[0], out var appointmentId))
                return context with { SelectedAppointmentId = appointmentId, SelectedAppointmentVersion = appointmentParts.Length > 1 && int.TryParse(appointmentParts[1], CultureInfo.InvariantCulture, out var version) ? version : null, PendingConfirmation = context.CurrentIntent is ConversationIntent.CancelAppointment or ConversationIntent.ConfirmAppointment };
        }
        if (machineValue.StartsWith("slot:", StringComparison.Ordinal))
        {
            var parts = machineValue.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length >= 3 && DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var startsAt) && DateTimeOffset.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var endsAt))
                return context with { SelectedSlotStartsAt = startsAt, SelectedSlotEndsAt = endsAt, PendingConfirmation = true };
        }
        return context;
    }

    private static ConversationContext ApplyContextualDate(ConversationContext context, string? message, DateTimeOffset receivedAt)
    {
        var normalized = ConversationIntentResolver.Normalize(message);
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
        if (transition.Intent is ConversationIntent.CancelAppointment or ConversationIntent.ConfirmAppointment)
            return await BuildAppointmentOperationResponseAsync(transition.Intent, context, tenantId, patientId, cancellationToken);
        if (transition.Intent == ConversationIntent.ScheduleAppointment)
        {
            if (!context.SelectedSpecialtyId.HasValue)
                return await BuildSpecialtiesAsync("Escolha uma especialidade para começarmos:", tenantId, cancellationToken);
            if (!context.SelectedProfessionalId.HasValue)
            {
                var specialtyId = context.SelectedSpecialtyId.Value;
                var professionals = await dbContext.Professionals.IgnoreQueryFilters().AsNoTracking().Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active && item.Specialties.Any(specialty => specialty.SpecialtyId == specialtyId)).OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
                var options = professionals.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"professional:{item.Id}||{item.Name}", index + 1)).ToList();
                return (options, professionals.Count == 0 ? "Não encontrei profissionais para essa especialidade." : "Encontrei estes profissionais. Qual você prefere?");
            }
            if (!context.SelectedDate.HasValue)
                return ([], "Qual data você prefere? Você pode escrever, por exemplo, *amanhã*.");
            if (!context.SelectedSlotStartsAt.HasValue)
            {
                var slots = await GetSlotsAsync(context.SelectedProfessionalId.Value, context.SelectedDate.Value, tenantId, cancellationToken);
                var options = slots.Take(_options.MaxOptionsPerMessage).Select((slot, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"slot:{context.SelectedProfessionalId.Value}|{slot.StartsAt:O}|{slot.EndsAt:O}||{slot.StartsAt:HH\\:mm}", index + 1)).ToList();
                return (options, slots.Count == 0 ? "Não encontrei horários nessa data. Você prefere tentar outro dia?" : "Encontrei estes horários. Qual você prefere?");
            }
            return ([], $"Certo. Sua consulta será em {context.SelectedDate:dd/MM} às {context.SelectedSlotStartsAt:HH\\:mm}. Posso confirmar o agendamento?");
        }
        if (transition.Intent == ConversationIntent.ListSpecialties)
        {
            return await BuildSpecialtiesAsync("Claro. Estas são algumas especialidades disponíveis:", tenantId, cancellationToken);
        }

        if (transition.Intent == ConversationIntent.ListProfessionals)
        {
            var professionalsQuery = dbContext.Professionals.IgnoreQueryFilters().AsNoTracking().Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active);
            if (context.SelectedSpecialtyId.HasValue)
            {
                var specialtyId = context.SelectedSpecialtyId.Value;
                professionalsQuery = professionalsQuery.Where(item => item.Specialties.Any(specialty => specialty.SpecialtyId == specialtyId));
            }
            var professionals = await professionalsQuery.OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
            var options = professionals.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"professional:{item.Id}||{item.Name}", index + 1)).ToList();
            var text = professionals.Count == 0 ? "No momento, não encontrei profissionais disponíveis." : "Claro. Estes são alguns profissionais disponíveis:";
            return (options, text);
        }

        return (transition.Options, null);
    }

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildAppointmentOperationResponseAsync(ConversationIntent intent, ConversationContext context, Guid tenantId, Guid patientId, CancellationToken cancellationToken)
    {
        if (context.SelectedAppointmentId.HasValue)
        {
            var selected = await dbContext.Appointments.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.PatientId == patientId && item.Id == context.SelectedAppointmentId.Value).Select(item => new { item.StartsAt, item.Status }).SingleOrDefaultAsync(cancellationToken);
            if (selected is null) return ([], "Não encontrei essa consulta. Vamos tentar novamente?");
            if (intent == ConversationIntent.RescheduleAppointment)
            {
                if (!context.SelectedDate.HasValue) return ([], $"Encontrei sua consulta em {selected.StartsAt:dd/MM} às {selected.StartsAt:HH\\:mm}. Qual nova data você prefere?");
                if (!context.SelectedSlotStartsAt.HasValue)
                {
                    var appointmentProfessional = await dbContext.Appointments.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.Id == context.SelectedAppointmentId.Value).Select(item => item.ProfessionalId).SingleAsync(cancellationToken);
                    var slots = await GetSlotsAsync(appointmentProfessional, context.SelectedDate.Value, tenantId, cancellationToken);
                    var rescheduleOptions = slots.Take(_options.MaxOptionsPerMessage).Select((slot, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"slot:{appointmentProfessional}|{slot.StartsAt:O}|{slot.EndsAt:O}||{slot.StartsAt:HH\\:mm}", index + 1)).ToList();
                    return (rescheduleOptions, slots.Count == 0 ? "Não encontrei horários nessa data. Você prefere tentar outro dia?" : "Para essa data, encontrei estes horários. Qual você prefere?");
                }
                return ([], $"Deseja mudar sua consulta para {context.SelectedDate:dd/MM} às {context.SelectedSlotStartsAt:HH\\:mm}?");
            }
            var action = intent == ConversationIntent.CancelAppointment ? "cancelar" : "confirmar sua presença em";
            return ([], $"Encontrei sua consulta em {selected.StartsAt:dd/MM} às {selected.StartsAt:HH\\:mm}. Deseja {action}?");
        }

        var now = DateTimeOffset.UtcNow;
        var appointments = await dbContext.Appointments.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.PatientId == patientId && item.StartsAt >= now && (intent == ConversationIntent.CancelAppointment ? item.Status != AppointmentStatus.Cancelled && item.Status != AppointmentStatus.Completed : item.Status == AppointmentStatus.Pending)).OrderBy(item => item.StartsAt).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
        var options = appointments.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"appointment:{item.Id}|{item.Version}||{item.StartsAt:dd/MM} às {item.StartsAt:HH\\:mm}", index + 1)).ToList();
        var text = intent switch { ConversationIntent.CancelAppointment => "Encontrei estas consultas. Qual você deseja cancelar?", ConversationIntent.RescheduleAppointment => "Encontrei estas consultas. Qual você deseja reagendar?", _ => "Encontrei estas consultas pendentes. Qual você deseja confirmar?" };
        return (options, appointments.Count == 0 ? "Não encontrei consultas futuras para essa operação." : text);
    }

    private async Task<(IReadOnlyCollection<ConversationOptionDefinition> Options, string? Text)> BuildSpecialtiesAsync(string text, Guid tenantId, CancellationToken cancellationToken)
    {
        var specialties = await dbContext.Specialties.IgnoreQueryFilters().AsNoTracking().Where(item => item.TenantId == tenantId && item.Status == CatalogStatus.Active).OrderBy(item => item.Name).Take(_options.MaxOptionsPerMessage).ToListAsync(cancellationToken);
        var options = specialties.Select((item, index) => new ConversationOptionDefinition((index + 1).ToString(CultureInfo.InvariantCulture), $"specialty:{item.Id}||{item.Name}", index + 1)).ToList();
        return (options, specialties.Count == 0 ? "No momento, não encontrei especialidades disponíveis." : text);
    }

    private async Task<List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>> GetSlotsAsync(Guid professionalId, DateOnly date, Guid tenantId, CancellationToken cancellationToken)
    {
        var rules = await dbContext.AvailabilityRules.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Active && item.DayOfWeek == date.DayOfWeek).ToListAsync(cancellationToken);
        var startOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = startOfDay.AddDays(1);
        var busy = await dbContext.Appointments.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.Status != ClinicAssistant.Domain.Scheduling.AppointmentStatus.Cancelled && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var blocks = await dbContext.ScheduleBlocks.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var vacations = await dbContext.ProfessionalVacations.IgnoreQueryFilters().Where(item => item.TenantId == tenantId && item.ProfessionalId == professionalId && item.StartsAt < endOfDay && item.EndsAt > startOfDay).Select(item => new { item.StartsAt, item.EndsAt }).ToListAsync(cancellationToken);
        var slots = new List<(DateTimeOffset StartsAt, DateTimeOffset EndsAt)>();
        foreach (var rule in rules)
            for (var start = new DateTimeOffset(date.ToDateTime(rule.StartTime), TimeSpan.Zero); start.AddMinutes(rule.SlotDurationMinutes) <= new DateTimeOffset(date.ToDateTime(rule.EndTime), TimeSpan.Zero); start = start.AddMinutes(rule.SlotDurationMinutes))
            {
                var end = start.AddMinutes(rule.SlotDurationMinutes);
                if (!busy.Concat(blocks).Concat(vacations).Any(item => item.StartsAt < end && item.EndsAt > start)) slots.Add((start, end));
            }
        return slots;
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

        var startsAt = context.SelectedSlotStartsAt.Value.ToUniversalTime();
        var endsAt = context.SelectedSlotEndsAt.Value.ToUniversalTime();
        var conflict = await dbContext.Appointments.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.Status != AppointmentStatus.Cancelled && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken)
            || await dbContext.ScheduleBlocks.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken)
            || await dbContext.ProfessionalVacations.IgnoreQueryFilters().AnyAsync(item => item.TenantId == command.TenantId && item.ProfessionalId == professional.Id && item.StartsAt < endsAt && item.EndsAt > startsAt, cancellationToken);
        if (conflict) return (false, string.Empty);

        var appointment = new Appointment(command.TenantId, professional.ClinicUnitId, professional.Id, context.SelectedSpecialtyId.Value, patientId, startsAt, endsAt, AppointmentSource.WhatsApp, "Agendamento iniciado pela conversa WhatsApp.");
        var response = new AppointmentResponse(appointment.Id, patientId, professional.Id, startsAt, endsAt, appointment.Status.ToString());
        dbContext.AddRange(appointment, new AuditRecord(command.TenantId, null, "appointment.created", "Appointment", appointment.Id, "Succeeded", "Appointment created through conversational WhatsApp flow."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response)));
        return (true, $"Consulta agendada ✅\n\n{professional.Name}\n{startsAt:dd/MM} às {startsAt:HH\\:mm}\n\nSe precisar, você pode escrever *reagendar*, *cancelar* ou *menu*.");
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
