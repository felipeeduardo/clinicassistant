using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Scheduling;

public enum AppointmentReminderType { DayBefore = 1, HourBefore = 2 }
public enum AppointmentReminderStatus { Scheduled, Queued, Sending, Sent, Failed, Cancelled, Skipped }

public sealed class AppointmentReminder : Entity, ITenantEntity
{
    private AppointmentReminder() { }
    public AppointmentReminder(Guid tenantId, Guid appointmentId, Guid? channelId, AppointmentReminderType type, DateTimeOffset appointmentStartUtc, DateTimeOffset scheduledAtUtc, string correlationId)
    { TenantId = tenantId; AppointmentId = appointmentId; WhatsAppChannelId = channelId; Type = type; AppointmentStartUtc = appointmentStartUtc; ScheduledAtUtc = scheduledAtUtc; CorrelationId = correlationId; }
    public Guid TenantId { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Guid? WhatsAppChannelId { get; private set; }
    public AppointmentReminderType Type { get; private set; }
    public DateTimeOffset AppointmentStartUtc { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public AppointmentReminderStatus Status { get; private set; } = AppointmentReminderStatus.Scheduled;
    public int RetryCount { get; private set; }
    public DateTimeOffset? QueuedAtUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public DateTimeOffset? FailedAtUtc { get; private set; }
    public string? ProviderCode { get; private set; }
    public string? FailureReason { get; private set; }
    public string CorrelationId { get; private set; } = null!;
    public void Queue() { if (Status is AppointmentReminderStatus.Scheduled or AppointmentReminderStatus.Failed) { Status = AppointmentReminderStatus.Queued; QueuedAtUtc = DateTimeOffset.UtcNow; UpdatedAt = QueuedAtUtc.Value; } }
    public void MarkSent() { Status = AppointmentReminderStatus.Sent; SentAtUtc = DateTimeOffset.UtcNow; UpdatedAt = SentAtUtc.Value; }
    public void MarkFailed(string? providerCode, string reason) { Status = AppointmentReminderStatus.Failed; RetryCount++; ProviderCode = providerCode; FailureReason = reason; FailedAtUtc = DateTimeOffset.UtcNow; UpdatedAt = FailedAtUtc.Value; }
    public void Cancel() { if (Status is not (AppointmentReminderStatus.Sent or AppointmentReminderStatus.Cancelled)) { Status = AppointmentReminderStatus.Cancelled; UpdatedAt = DateTimeOffset.UtcNow; } }
    public void Skip() { if (Status == AppointmentReminderStatus.Scheduled) { Status = AppointmentReminderStatus.Skipped; UpdatedAt = DateTimeOffset.UtcNow; } }
}
