using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Operations;

public sealed class AuditQueryService(ClinicAssistantDbContext db, TenantAccessGuard guard) : IAuditQueryService
{
    public async Task<AuditPage> SearchAsync(AuditQuery query, CancellationToken ct)
    {
        var tenantId = guard.RequireTenantId(); var page = Math.Max(1, query.Page); var pageSize = Math.Clamp(query.PageSize, 1, 100); var source = db.AuditRecords.AsNoTracking().Where(item => item.TenantId == tenantId);
        if (query.UserId.HasValue) source = source.Where(item => item.ActorUserId == query.UserId); if (!string.IsNullOrWhiteSpace(query.Action)) source = source.Where(item => item.Action == query.Action); if (!string.IsNullOrWhiteSpace(query.ResourceType)) source = source.Where(item => item.ResourceType == query.ResourceType); if (query.ResourceId.HasValue) source = source.Where(item => item.ResourceId == query.ResourceId); if (!string.IsNullOrWhiteSpace(query.Result)) source = source.Where(item => item.Result == query.Result); if (query.From.HasValue) source = source.Where(item => item.CreatedAt >= query.From.Value.ToUniversalTime()); if (query.To.HasValue) source = source.Where(item => item.CreatedAt < query.To.Value.ToUniversalTime());
        var total = await source.CountAsync(ct); var items = await (from audit in source join user in db.Users.IgnoreQueryFilters() on audit.ActorUserId equals user.Id into users from actor in users.DefaultIfEmpty() orderby audit.CreatedAt descending select new AuditItem(audit.CreatedAt, audit.ActorUserId, actor == null ? null : actor.Name, audit.Action, audit.ResourceType, audit.ResourceId, audit.Result)).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct); return new(items, page, pageSize, total);
    }
}
