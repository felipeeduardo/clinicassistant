using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicAssistant.Api.Health;

public sealed class OutboxHealthCheck(ClinicAssistantDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var pending = await db.OutboxMessages.IgnoreQueryFilters().CountAsync(x => x.Status == MessageStatus.Pending, cancellationToken);
        var deadLettered = await db.OutboxMessages.IgnoreQueryFilters().CountAsync(x => x.Status == MessageStatus.DeadLettered, cancellationToken);
        if (deadLettered > 0) return HealthCheckResult.Degraded($"Worker/outbox has {deadLettered} dead-lettered message(s) and {pending} pending.");
        return HealthCheckResult.Healthy($"Worker/outbox is observable: {pending} pending message(s).");
    }
}
