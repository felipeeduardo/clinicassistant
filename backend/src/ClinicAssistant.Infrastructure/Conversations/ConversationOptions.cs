namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationOptions
{
    public const string SectionName = "Conversation";
    public int StateExpirationMinutes { get; init; } = 30;
    public int IdleCloseHours { get; init; } = 24;
    public int MaximumInvalidAttempts { get; init; } = 3;
    public int LockTimeoutSeconds { get; init; } = 10;
    public int LockTtlSeconds { get; init; } = 60;
    public int MaxOptionsPerMessage { get; init; } = 6;
    public int MaxAvailableDaysPerMessage { get; init; } = 4;
    public int AvailabilitySearchDays { get; init; } = 14;
    public int MaxMessageLength { get; init; } = 2_000;
    public string DefaultLanguage { get; init; } = "pt-BR";
    public bool ReopenClosedConversations { get; init; } = true;
}
