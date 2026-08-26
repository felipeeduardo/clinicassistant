namespace ClinicAssistant.Infrastructure.Scheduling;

public sealed class AppointmentReminderOptions
{
    public const string SectionName = "AppointmentReminders";
    public bool Enabled { get; init; }
    public bool DayBeforeEnabled { get; init; } = true;
    public bool HourBeforeEnabled { get; init; } = true;
    public int PollingSeconds { get; init; } = 15;
}
