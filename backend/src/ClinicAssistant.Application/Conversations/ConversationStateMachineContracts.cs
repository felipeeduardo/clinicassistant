using ClinicAssistant.Domain.Conversations;

namespace ClinicAssistant.Application.Conversations;

public interface IConversationStateMachine
{
    ConversationTransitionResult Transition(ConversationInput input);
}

public interface IConversationIntentResolver
{
    ConversationIntentResolution Resolve(string? message, ConversationContext context, IReadOnlyCollection<ConversationOptionDefinition> options);
}

public interface IConversationResponseComposer
{
    ConversationResponse Compose(ConversationResponseRequest request);
}

public sealed record ConversationInput(
    string? Text,
    ConversationFlowState CurrentFlowState,
    ConversationStateStatus CurrentStatus,
    ConversationIntent CurrentIntent,
    int InvalidAttempts,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset ReceivedAt,
    IReadOnlyCollection<ConversationOptionDefinition>? Options = null,
    ConversationContext? Context = null);

public sealed record ConversationContext(
    ConversationIntent CurrentIntent = ConversationIntent.Unknown,
    ConversationFlowState CurrentStep = ConversationFlowState.Initial,
    ConversationFlowState? PreviousStep = null,
    int InvalidAttemptCount = 0,
    Guid? SelectedSpecialtyId = null,
    Guid? SelectedProfessionalId = null,
    Guid? SelectedUnitId = null,
    DateOnly? SelectedDate = null,
    Guid? SelectedSlotId = null,
    Guid? SelectedAppointmentId = null,
    bool PendingConfirmation = false,
    string? LastUserMessage = null,
    string? LastBotMessage = null,
    DateTimeOffset? FlowStartedAt = null,
    DateTimeOffset? LastInteractionAt = null,
    DateTimeOffset? SelectedSlotStartsAt = null,
    DateTimeOffset? SelectedSlotEndsAt = null,
    int? SelectedAppointmentVersion = null,
    string? SelectedSpecialtyName = null,
    string? SelectedProfessionalName = null,
    DateTimeOffset? AvailabilityCursor = null,
    bool AwaitingDateSelection = false,
    bool AwaitingAvailableDaySelection = false);

public sealed record ConversationIntentResolution(ConversationIntent Intent, string NormalizedText, bool IsContextualSelection = false);

public sealed record ConversationTransitionResult(
    ConversationFlowState FlowState,
    ConversationStateStatus Status,
    ConversationIntent Intent,
    ConversationAction Action,
    int InvalidAttempts,
    string ResponseKey,
    IReadOnlyCollection<ConversationOptionDefinition> Options);

public sealed record ConversationOptionDefinition(string Key, string Value, int DisplayOrder, string? ActionId = null);

public sealed record ConversationResponseRequest(
    string ResponseKey,
    IReadOnlyCollection<ConversationOptionDefinition> Options,
    string Language,
    string? CustomText = null,
    bool OptionsAlreadyRendered = false);

public sealed record ConversationChoice(string ActionId, string Label, string? Description = null);
public enum ConversationInteractionType { None = 0, List = 1, ReplyButtons = 2 }
public sealed record ConversationInteraction(ConversationInteractionType Type, IReadOnlyCollection<ConversationChoice> Choices);
public sealed record ConversationResponse(string Text, IReadOnlyCollection<ConversationOptionDefinition> Options, ConversationInteraction? Interaction = null);
