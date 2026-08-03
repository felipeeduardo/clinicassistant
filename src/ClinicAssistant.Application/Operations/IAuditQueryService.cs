namespace ClinicAssistant.Application.Operations;

public sealed record AuditQuery(int Page = 1, int PageSize = 25, Guid? UserId = null, string? Action = null, string? ResourceType = null, Guid? ResourceId = null, string? Result = null, DateTimeOffset? From = null, DateTimeOffset? To = null);
public sealed record AuditItem(DateTimeOffset OccurredAt, Guid? ActorUserId, string? ActorName, string Action, string ResourceType, Guid? ResourceId, string Result);
public sealed record AuditPage(IReadOnlyList<AuditItem> Items, int Page, int PageSize, int TotalCount);

public interface IAuditQueryService
{
    Task<AuditPage> SearchAsync(AuditQuery query, CancellationToken ct);
}
