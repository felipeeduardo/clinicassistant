using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Conversations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Operations;

public sealed class OperationalNotificationService(ClinicAssistantDbContext db, TenantAccessGuard guard, IOperationalEventPublisher events, IOptions<HumanQueueOptions> options) : IOperationalNotificationService
{
    public async Task<NotificationPage> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        var tenant = guard.RequireTenantId(); page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from n in db.OperationalNotifications join c in db.Conversations on n.ConversationId equals c.Id join p in db.Patients on c.PatientId equals p.Id where n.TenantId == tenant && n.Status != OperationalNotificationStatus.Resolved orderby n.CreatedAt descending select new { n, p.Name, c.WaitingSince };
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new OperationalNotificationItem(x.n.Id, x.n.ConversationId, x.Name, x.n.Type, x.n.Severity, x.n.Status, x.n.CreatedAt, x.WaitingSince)).ToListAsync(ct);
        return new(items, page, pageSize, total);
    }
    public async Task<NotificationSummary> SummaryAsync(CancellationToken ct)
    {
        var tenant = guard.RequireTenantId();
        var unread = await db.OperationalNotifications.CountAsync(x => x.TenantId == tenant && x.Status == OperationalNotificationStatus.Unread, ct);
        var waiting = await db.Conversations.CountAsync(x => x.TenantId == tenant && x.Status == ConversationStatus.WaitingHuman, ct);
        var sla = await db.OperationalNotifications.CountAsync(x => x.TenantId == tenant && x.Type == OperationalNotificationType.HumanQueueSlaExceeded && x.Status != OperationalNotificationStatus.Resolved, ct);
        var oldest = await db.Conversations.Where(x => x.TenantId == tenant && x.Status == ConversationStatus.WaitingHuman && x.WaitingSince != null).MinAsync(x => (DateTimeOffset?)x.WaitingSince, ct);
        return new(unread, waiting, sla, oldest);
    }
    public async Task MarkReadAsync(Guid id, CancellationToken ct) { var tenant = guard.RequireTenantId(); var n = await db.OperationalNotifications.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenant, ct) ?? throw new KeyNotFoundException(); n.MarkRead(); await db.SaveChangesAsync(ct); OperationalTelemetry.HumanQueueNotificationsRead.Add(1); }
    public async Task MarkAllReadAsync(CancellationToken ct) { var tenant = guard.RequireTenantId(); var list = await db.OperationalNotifications.Where(x => x.TenantId == tenant && x.Status == OperationalNotificationStatus.Unread).ToListAsync(ct); foreach (var n in list) n.MarkRead(); await db.SaveChangesAsync(ct); }
    public async Task CreateInitialAsync(Guid tenantId, Guid conversationId, string correlationId, CancellationToken ct)
    {
        var exists = await db.OperationalNotifications.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.ConversationId == conversationId && x.Type == OperationalNotificationType.HumanHandoffRequested, ct);
        if (exists) return;
        var notification = new OperationalNotification(tenantId, conversationId, OperationalNotificationType.HumanHandoffRequested, OperationalNotificationSeverity.New, correlationId);
        db.OperationalNotifications.Add(notification); await db.SaveChangesAsync(ct); OperationalTelemetry.HumanQueueNotificationsCreated.Add(1);
        await events.PublishAsync(tenantId, "human.handoff.requested", new { NotificationId = notification.Id, ConversationId = conversationId, Type = notification.Type.ToString(), Severity = notification.Severity.ToString(), CreatedAt = notification.CreatedAt, CorrelationId = correlationId }, ct);
    }
    public async Task ResolveForConversationAsync(Guid tenantId, Guid conversationId, CancellationToken ct)
    { var list = await db.OperationalNotifications.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.ConversationId == conversationId && x.Status != OperationalNotificationStatus.Resolved).ToListAsync(ct); foreach (var n in list) n.Resolve(); if (list.Count > 0) { await db.SaveChangesAsync(ct); await events.PublishAsync(tenantId, "human.conversation.resolved", new { ConversationId = conversationId }, ct); } }
    public async Task ProcessEscalationsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow; var reminderAt = now.AddMinutes(-options.Value.ReminderMinutes); var slaAt = now.AddMinutes(-options.Value.SlaMinutes);
        var candidates = await db.Conversations.IgnoreQueryFilters().Where(x => x.Status == ConversationStatus.WaitingHuman && x.WaitingSince != null && x.AssignedUserId == null && (x.HumanQueueReminderSentAt == null || x.HumanQueueSlaExceededAt == null)).Take(200).ToListAsync(ct);
        foreach (var conversation in candidates)
        {
            if (conversation.WaitingSince <= slaAt && conversation.HumanQueueSlaExceededAt == null)
            { conversation.MarkSlaExceeded(now); var n = await EnsureEscalationAsync(conversation, OperationalNotificationType.HumanQueueSlaExceeded, OperationalNotificationSeverity.High, ct); OperationalTelemetry.HumanQueueSlaExceeded.Add(1); await events.PublishAsync(conversation.TenantId, "human.queue.sla.exceeded", new { NotificationId = n.Id, ConversationId = conversation.Id, Severity = "High" }, ct); }
            else if (conversation.WaitingSince <= reminderAt && conversation.HumanQueueReminderSentAt == null)
            { conversation.MarkReminderSent(now); var n = await EnsureEscalationAsync(conversation, OperationalNotificationType.HumanQueueReminder, OperationalNotificationSeverity.Attention, ct); OperationalTelemetry.HumanQueueReminders.Add(1); await events.PublishAsync(conversation.TenantId, "human.queue.reminder", new { NotificationId = n.Id, ConversationId = conversation.Id, Severity = "Attention" }, ct); }
        }
        if (candidates.Count > 0) await db.SaveChangesAsync(ct);
    }
    private async Task<OperationalNotification> EnsureEscalationAsync(Conversation c, OperationalNotificationType type, OperationalNotificationSeverity severity, CancellationToken ct)
    { var n = await db.OperationalNotifications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == c.TenantId && x.ConversationId == c.Id && x.Type == type, ct); if (n is null) { n = new OperationalNotification(c.TenantId, c.Id, type, severity, Guid.NewGuid().ToString("N")); db.OperationalNotifications.Add(n); } else n.Escalate(severity); return n; }
}
