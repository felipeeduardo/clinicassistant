namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class HumanQueueOptions
{
    public const string SectionName = "HumanQueue";
    public int ReminderMinutes { get; init; } = 3;
    public int SlaMinutes { get; init; } = 10;
    public int PollingSeconds { get; init; } = 30;
}
