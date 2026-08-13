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
    public async Task<DashboardSummary> GetAsync(DateTimeOffset? from, DateTimeOffset? endsAt, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var tenantId = guard.RequireTenantId(); var now = DateTimeOffset.UtcNow; var dayStart = from ?? new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero); var dayEnd = endsAt ?? dayStart.AddDays(1);
        if (dayEnd <= dayStart) throw new ArgumentException("Dashboard period must have a positive duration.", nameof(endsAt));
        var appointments = db.Appointments.Where(item => item.TenantId == tenantId && item.StartsAt >= dayStart && item.StartsAt < dayEnd); var integration = await db.WhatsAppIntegrations.Where(item => item.TenantId == tenantId).OrderByDescending(item => item.UpdatedAt).Select(item => item.Status.ToString()).FirstOrDefaultAsync(ct);
        var status = await appointments.GroupBy(item => item.Status).Select(group => new { Status = group.Key.ToString(), Count = group.Count() }).ToDictionaryAsync(item => item.Status, item => item.Count, ct);
        var upcomingEnd = now.AddDays(7);
        var upcoming = await (from appointment in db.Appointments
                              join patient in db.Patients on appointment.PatientId equals patient.Id
                              join professional in db.Professionals on appointment.ProfessionalId equals professional.Id
                              join unit in db.ClinicUnits on appointment.ClinicUnitId equals unit.Id
                              where appointment.TenantId == tenantId && appointment.StartsAt >= now && appointment.StartsAt < upcomingEnd && appointment.Status != AppointmentStatus.Cancelled
                              orderby appointment.StartsAt
                              select new DashboardUpcomingAppointment(appointment.Id, appointment.StartsAt, appointment.EndsAt, appointment.Status.ToString(), patient.Name, professional.Name, unit.Name)).Take(5).ToListAsync(ct);
        var messages = db.ConversationMessages.Where(item => item.TenantId == tenantId && item.CreatedAt >= dayStart && item.CreatedAt < dayEnd);
        var openConversations = await db.Conversations.CountAsync(item => item.TenantId == tenantId && item.Status != ConversationStatus.Closed, ct);
        var failedMessages = await messages.CountAsync(item => item.Direction == ConversationMessageDirection.Outbound && item.Status == ConversationMessageStatus.Failed, ct);
        var whatsappMetrics = new DashboardWhatsAppMetrics(await messages.CountAsync(item => item.Direction == ConversationMessageDirection.Inbound, ct), await messages.CountAsync(item => item.Direction == ConversationMessageDirection.Outbound, ct), failedMessages, openConversations);
        var result = new DashboardSummary(await appointments.CountAsync(ct), await appointments.CountAsync(item => item.Status == AppointmentStatus.Pending, ct), await db.HumanQueueItems.CountAsync(item => item.TenantId == tenantId && item.Status == HumanQueueItemStatus.Waiting, ct), openConversations, await db.OutboxMessages.CountAsync(item => item.TenantId == tenantId && item.Status == MessageStatus.DeadLettered, ct), integration, status, upcoming, whatsappMetrics);
        OperationalTelemetry.DashboardRequests.Add(1);
        OperationalTelemetry.DashboardRequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
        return result;
    }
}
