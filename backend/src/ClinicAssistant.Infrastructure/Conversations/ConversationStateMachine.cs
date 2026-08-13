using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Domain.Conversations;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationStateMachine : IConversationStateMachine
{
    private readonly ConversationOptions _options;
    private readonly IConversationIntentResolver _resolver;

    public ConversationStateMachine(IOptions<ConversationOptions> options, IConversationIntentResolver? resolver = null)
    {
        _options = options.Value;
        _resolver = resolver ?? new ConversationIntentResolver();
    }

    public ConversationTransitionResult Transition(ConversationInput input)
    {
        if (IsExpired(input)) return Menu(ConversationIntent.MainMenu, 0, "conversation.expired");

        var context = input.Context ?? new ConversationContext(input.CurrentIntent, input.CurrentFlowState, null, input.InvalidAttempts);
        var resolution = _resolver.Resolve(input.Text, context, input.Options ?? []);
        var intent = resolution.Intent;

        if (intent is ConversationIntent.MainMenu or ConversationIntent.Greeting)
            return Menu(intent, 0, intent == ConversationIntent.Greeting ? "conversation.greeting" : "conversation.menu");
        if (intent == ConversationIntent.GoBack)
            return Menu(context.CurrentIntent, 0, "conversation.back", ConversationAction.GoBack);
        if (intent == ConversationIntent.CancelCurrentFlow)
            return Menu(ConversationIntent.MainMenu, 0, "conversation.cancelled", ConversationAction.CancelFlow);
        if (intent == ConversationIntent.HumanHandoff)
            return new(ConversationFlowState.HandedOff, ConversationStateStatus.HandedOff, ConversationIntent.HumanHandoff, ConversationAction.Handoff, 0, "conversation.handoff", []);
        if (intent == ConversationIntent.Farewell)
            return new(ConversationFlowState.Closed, ConversationStateStatus.Completed, intent, ConversationAction.CloseConversation, 0, "conversation.closed", []);
        if (intent == ConversationIntent.Help)
            return new(input.CurrentFlowState == ConversationFlowState.Menu ? ConversationFlowState.Menu : input.CurrentFlowState, ConversationStateStatus.Active, context.CurrentIntent, ConversationAction.None, 0, input.CurrentFlowState == ConversationFlowState.Menu ? "conversation.help" : "conversation.help_contextual", []);
        if (intent == ConversationIntent.Repeat)
            return new(input.CurrentFlowState, ConversationStateStatus.Active, context.CurrentIntent, ConversationAction.None, 0, "conversation.repeat", input.Options ?? []);
        if (intent == ConversationIntent.Unknown || intent == ConversationIntent.Unsupported) return Invalid(input);

        return intent switch
        {
            ConversationIntent.InstitutionalQuestion => new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.institutional", MenuOptions()),
            ConversationIntent.ViewSpecialties => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ListSpecialties, ConversationAction.None, 0, "conversation.specialties", []),
            ConversationIntent.ViewProfessionals => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ListProfessionals, ConversationAction.None, 0, "conversation.professionals", []),
            ConversationIntent.CheckAvailability => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.availability", []),
            ConversationIntent.ScheduleAppointment => ScheduleTransition(input, context),
            ConversationIntent.RescheduleAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.reschedule", []),
            ConversationIntent.CancelAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.cancel_appointment", []),
            ConversationIntent.ConfirmAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.confirm", []),
            _ => Invalid(input)
        };
    }

    private ConversationTransitionResult Invalid(ConversationInput input)
    {
        var attempts = input.InvalidAttempts + 1;
        if (attempts >= _options.MaximumInvalidAttempts)
            return new(ConversationFlowState.HandedOff, ConversationStateStatus.HandedOff, ConversationIntent.HumanHandoff, ConversationAction.Handoff, attempts, "conversation.handoff", []);
        if (attempts == 1)
            return new(input.CurrentFlowState, ConversationStateStatus.Active, ConversationIntent.Unknown, ConversationAction.None, attempts, "conversation.invalid_answer", []);
        return new(ConversationFlowState.Menu, ConversationStateStatus.Active, ConversationIntent.Unknown, ConversationAction.ShowMenu, attempts, "conversation.invalid_again", MenuOptions());
    }

    private static ConversationTransitionResult Menu(ConversationIntent intent, int invalidAttempts, string responseKey, ConversationAction action = ConversationAction.ShowMenu) =>
        new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, action, invalidAttempts, responseKey, MenuOptions());

    private static ConversationTransitionResult ScheduleTransition(ConversationInput input, ConversationContext context)
    {
        var responseKey = context.PendingConfirmation ? "conversation.schedule_confirmation" : context.SelectedSlotStartsAt.HasValue ? "conversation.schedule_confirmation" : context.SelectedDate.HasValue && context.SelectedProfessionalId.HasValue ? "conversation.schedule_slot" : context.SelectedSpecialtyId.HasValue ? "conversation.schedule_professional" : "conversation.schedule_specialty";
        return new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ScheduleAppointment, ConversationAction.None, 0, responseKey, []);
    }

    public static IReadOnlyCollection<ConversationOptionDefinition> MenuOptions() =>
    [
        new("1", "specialties", 1), new("2", "professionals", 2), new("3", "availability", 3),
        new("4", "schedule", 4), new("5", "reschedule", 5), new("6", "cancel_appointment", 6),
        new("7", "confirm", 7), new("8", "human", 8)
    ];

    private static bool IsExpired(ConversationInput input) => input.ExpiresAt.HasValue && input.ExpiresAt.Value <= input.ReceivedAt;
}
