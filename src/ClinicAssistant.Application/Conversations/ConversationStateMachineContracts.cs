using ClinicAssistant.Domain.Conversations;

namespace ClinicAssistant.Application.Conversations;

public interface IConversationStateMachine
{
    ConversationTransitionResult Transition(ConversationInput input);
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
    DateTimeOffset ReceivedAt);

public sealed record ConversationTransitionResult(
    ConversationFlowState FlowState,
    ConversationStateStatus Status,
    ConversationIntent Intent,
    ConversationAction Action,
    int InvalidAttempts,
    string ResponseKey,
    IReadOnlyCollection<ConversationOptionDefinition> Options);

public sealed record ConversationOptionDefinition(string Key, string Value, int DisplayOrder);

public sealed record ConversationResponseRequest(
    string ResponseKey,
    IReadOnlyCollection<ConversationOptionDefinition> Options,
    string Language);

public sealed record ConversationResponse(string Text, IReadOnlyCollection<ConversationOptionDefinition> Options);
