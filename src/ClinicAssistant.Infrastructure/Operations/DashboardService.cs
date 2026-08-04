using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClinicAssistant.Infrastructure.Operations;

public sealed class DashboardService(ClinicAssistantDbContext db, TenantAccessGuard guard) : IDashboardService
{
    public async Task<DashboardSummary> GetAsync(CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var tenantId = guard.RequireTenantId(); var now = DateTimeOffset.UtcNow; var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero); var dayEnd = dayStart.AddDays(1);
        var appointments = db.Appointments.Where(item => item.TenantId == tenantId && item.StartsAt >= dayStart && item.StartsAt < dayEnd); var integration = await db.WhatsAppIntegrations.Where(item => item.TenantId == tenantId).OrderByDescending(item => item.UpdatedAt).Select(item => item.Status.ToString()).FirstOrDefaultAsync(ct);
        var result = new DashboardSummary(await appointments.CountAsync(ct), await appointments.CountAsync(item => item.Status == AppointmentStatus.Pending, ct), await db.HumanQueueItems.CountAsync(item => item.TenantId == tenantId && item.Status == HumanQueueItemStatus.Waiting, ct), await db.Conversations.CountAsync(item => item.TenantId == tenantId && item.Status != ConversationStatus.Closed, ct), await db.OutboxMessages.CountAsync(item => item.TenantId == tenantId && item.Status == MessageStatus.DeadLettered, ct), integration);
        OperationalTelemetry.DashboardRequests.Add(1);
        OperationalTelemetry.DashboardRequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
        return result;
    }
}
