using ClinicAssistant.Domain.Operations;

namespace ClinicAssistant.Application.Operations;

public sealed record OperationalNotificationItem(Guid Id, Guid ConversationId, string PatientName, OperationalNotificationType Type, OperationalNotificationSeverity Severity, OperationalNotificationStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? WaitingSince);
public sealed record NotificationPage(IReadOnlyList<OperationalNotificationItem> Items, int Page, int PageSize, int Total);
public sealed record NotificationSummary(int UnreadCount, int WaitingCount, int SlaExceededCount, DateTimeOffset? OldestWaitingSince);
public interface IOperationalNotificationService
{
    Task<NotificationPage> ListAsync(int page, int pageSize, CancellationToken ct);
    Task<NotificationSummary> SummaryAsync(CancellationToken ct);
    Task MarkReadAsync(Guid id, CancellationToken ct);
    Task MarkAllReadAsync(CancellationToken ct);
    Task CreateInitialAsync(Guid tenantId, Guid conversationId, string correlationId, CancellationToken ct);
    Task ResolveForConversationAsync(Guid tenantId, Guid conversationId, CancellationToken ct);
    Task ProcessEscalationsAsync(CancellationToken ct);
}
