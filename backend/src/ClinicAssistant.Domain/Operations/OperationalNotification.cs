using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Operations;

public enum OperationalNotificationType { HumanHandoffRequested, HumanQueueReminder, HumanQueueSlaExceeded }
public enum OperationalNotificationSeverity { New, Attention, High }
public enum OperationalNotificationStatus { Unread, Read, Resolved }

public sealed class OperationalNotification : Entity, ITenantEntity
{
    private OperationalNotification() { }
    public OperationalNotification(Guid tenantId, Guid conversationId, OperationalNotificationType type, OperationalNotificationSeverity severity, string correlationId)
    { TenantId = tenantId; ConversationId = conversationId; Type = type; Severity = severity; CorrelationId = correlationId; Status = OperationalNotificationStatus.Unread; }
    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public OperationalNotificationType Type { get; private set; }
    public OperationalNotificationSeverity Severity { get; private set; }
    public OperationalNotificationStatus Status { get; private set; }
    public string CorrelationId { get; private set; } = null!;
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public void MarkRead() { if (Status == OperationalNotificationStatus.Unread) { Status = OperationalNotificationStatus.Read; ReadAt = DateTimeOffset.UtcNow; UpdatedAt = ReadAt.Value; } }
    public void Resolve() { Status = OperationalNotificationStatus.Resolved; ResolvedAt = DateTimeOffset.UtcNow; UpdatedAt = ResolvedAt.Value; }
    public void Escalate(OperationalNotificationSeverity severity) { if (Status != OperationalNotificationStatus.Resolved) Severity = severity; UpdatedAt = DateTimeOffset.UtcNow; }
}
