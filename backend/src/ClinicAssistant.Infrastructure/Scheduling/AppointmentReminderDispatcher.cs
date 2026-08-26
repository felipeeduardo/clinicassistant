using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ClinicAssistant.Infrastructure.Scheduling;

/// Claims due reminders and materializes them as the existing WhatsApp outbox command.
/// The claim is persisted before publishing, so restarts cannot create duplicates.
public sealed partial class AppointmentReminderDispatcher(ClinicAssistantDbContext db, IOptions<TwilioOptions> twilio, IOptions<AppointmentReminderOptions> options, ILogger<AppointmentReminderDispatcher> logger)
{
    public async Task<int> DispatchDueAsync(CancellationToken ct)
    {
        if (!options.Value.Enabled) return 0;
        var now = DateTimeOffset.UtcNow;
        var due = await db.AppointmentReminders.Where(x => x.Status == AppointmentReminderStatus.Scheduled && x.ScheduledAtUtc <= now).OrderBy(x => x.ScheduledAtUtc).Take(50).ToListAsync(ct);
        var count = 0;
        foreach (var reminder in due)
        {
            var data = await (from a in db.Appointments
                              join p in db.Patients on a.PatientId equals p.Id
                              join unit in db.ClinicUnits on a.ClinicUnitId equals unit.Id
                              join clinic in db.Clinics on unit.ClinicId equals clinic.Id
                              join integration in db.WhatsAppIntegrations on a.TenantId equals integration.TenantId
                              where a.Id == reminder.AppointmentId && a.TenantId == reminder.TenantId && integration.Status == WhatsAppIntegrationStatus.Connected
                              select new { Appointment = a, Patient = p, Unit = unit, Clinic = clinic, Integration = integration }).SingleOrDefaultAsync(ct);
            if (data is null || data.Appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rescheduled || data.Appointment.StartsAt != reminder.AppointmentStartUtc)
            { reminder.Cancel(); continue; }
            var sid = reminder.Type == AppointmentReminderType.DayBefore ? twilio.Value.AppointmentReminder24hContentSid : twilio.Value.AppointmentReminder1hContentSid;
            if (string.IsNullOrWhiteSpace(sid)) { reminder.MarkFailed("template_missing", "Reminder template is not configured."); continue; }
            var conversation = await db.Conversations.SingleOrDefaultAsync(x => x.TenantId == reminder.TenantId && x.PatientId == data.Patient.Id && x.IntegrationId == data.Integration.Id, ct);
            if (conversation is null) { conversation = new Conversation(reminder.TenantId, data.Patient.Id, data.Integration.Id, data.Patient.Phone); db.Conversations.Add(conversation); }
            if (reminder.WhatsAppChannelId.HasValue) conversation.SetWhatsAppChannel(reminder.WhatsAppChannelId.Value);
            var message = new ConversationMessage(reminder.TenantId, conversation.Id, ConversationMessageType.Template, reminder.Type == AppointmentReminderType.DayBefore ? "Lembrete de consulta (24h)" : "Lembrete de consulta (1h)", data.Integration.Provider);
            var timeZone = ResolveTimeZone(data.Clinic.TimeZone);
            var local = TimeZoneInfo.ConvertTime(data.Appointment.StartsAt, timeZone);
            var command = new SendWhatsAppMessageCommand(reminder.TenantId, data.Integration.Id, conversation.Id, message.Id, WhatsAppOutgoingMessageType.Template, data.Patient.Phone, message.Content, sid, new Dictionary<string,string> { ["1"] = data.Patient.Name, ["2"] = local.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), ["3"] = local.ToString("HH:mm", CultureInfo.InvariantCulture), ["4"] = data.Unit.Name }, null, $"appointment-reminder:{reminder.Id:N}", reminder.CorrelationId, null, reminder.WhatsAppChannelId);
            reminder.Queue(); db.Add(message); db.Add(new OutboxMessage(reminder.TenantId, nameof(SendWhatsAppMessageCommand), JsonSerializer.Serialize(command), reminder.WhatsAppChannelId)); count++;
        }
        if (count > 0 || due.Count > 0) await db.SaveChangesAsync(ct);
        if (count > 0) LogQueued(logger, count);
        return count;
    }
    [LoggerMessage(Level = LogLevel.Information, Message = "Appointment reminders queued: {Count}")]
    private static partial void LogQueued(ILogger logger, int count);
    private static TimeZoneInfo ResolveTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }
}
