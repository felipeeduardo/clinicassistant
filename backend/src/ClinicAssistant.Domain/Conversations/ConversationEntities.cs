using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Conversations;

public enum ConversationAutomationMode { Automated, Paused, Human }
public enum ConversationPriority { Normal, High, Urgent }
public enum ConversationStateStatus { Active, Expired, Completed, HandedOff }
public enum ConversationFlowState { Initial, Menu, AwaitingSelection, AwaitingSlotSelection, AwaitingScheduleConfirmation, Cancelled, HandedOff, Closed }
public enum ConversationIntent
{
    Unknown, Greeting, InstitutionalQuestion, ListSpecialties, ListProfessionals,
    CheckAvailability, ScheduleAppointment, RescheduleAppointment, CancelAppointment,
    ConfirmAppointment, TalkToHuman, Farewell, Unsupported,
    MainMenu, GoBack, CancelCurrentFlow, Repeat, Help,
    ConfirmSelectedSlot, ConfirmExistingAppointment, ConfirmReschedule,
    ViewSpecialties = ListSpecialties,
    ViewProfessionals = ListProfessionals,
    HumanHandoff = TalkToHuman
}
public enum ConversationAction { None, ShowMenu, GoBack, CancelFlow, Handoff, CloseConversation, ReopenConversation }

public sealed class ConversationState : Entity, ITenantEntity
{
    private ConversationState() { }

    public ConversationState(Guid tenantId, Guid conversationId, DateTimeOffset expiresAt)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        ExpiresAt = expiresAt;
        Status = ConversationStateStatus.Active;
        FlowState = ConversationFlowState.Initial;
        Intent = ConversationIntent.Unknown;
        ContextJson = "{}";
        Version = 1;
    }

    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public ConversationStateStatus Status { get; private set; }
    public ConversationFlowState FlowState { get; private set; }
    public ConversationIntent Intent { get; private set; }
    public string ContextJson { get; private set; } = null!;
    public int InvalidAttempts { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? LastInteractionAt { get; private set; }
    public int Version { get; private set; }

    public void Apply(ConversationFlowState flowState, ConversationStateStatus status, ConversationIntent intent, int invalidAttempts, DateTimeOffset expiresAt)
    {
        FlowState = flowState;
        Status = status;
        Intent = intent;
        InvalidAttempts = invalidAttempts;
        ExpiresAt = expiresAt;
        LastInteractionAt = DateTimeOffset.UtcNow;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContext(string contextJson)
    {
        ContextJson = contextJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class ConversationProcessedMessage : Entity, ITenantEntity
{
    private ConversationProcessedMessage() { }

    public ConversationProcessedMessage(Guid tenantId, Guid conversationId, Guid conversationMessageId)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        ConversationMessageId = conversationMessageId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid ConversationMessageId { get; private set; }
    public Guid? ResponseMessageId { get; private set; }
    public Guid? OutboxMessageId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    public void SetResponse(Guid responseMessageId, Guid outboxMessageId)
    {
        ResponseMessageId = responseMessageId;
        OutboxMessageId = outboxMessageId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class ConversationOption : Entity, ITenantEntity
{
    private ConversationOption() { }

    public ConversationOption(Guid tenantId, Guid conversationStateId, string key, string value, int displayOrder, DateTimeOffset expiresAt, string? actionId = null)
    {
        TenantId = tenantId;
        ConversationStateId = conversationStateId;
        Key = key;
        Value = value;
        DisplayOrder = displayOrder;
        ExpiresAt = expiresAt;
        ActionId = actionId;
    }

    public Guid TenantId { get; private set; }
    public Guid ConversationStateId { get; private set; }
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? ActionId { get; private set; }
}
