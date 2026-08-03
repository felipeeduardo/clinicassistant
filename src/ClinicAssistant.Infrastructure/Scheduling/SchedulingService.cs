using System.Data;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Scheduling;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Scheduling;

public sealed class SchedulingService(ClinicAssistantDbContext db, TenantAccessGuard guard, IOperationalEventPublisher events) : ISchedulingService
{
    public async Task<IReadOnlyList<PatientResponse>> GetPatientsAsync(CancellationToken ct) => await db.Patients.OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);
    public async Task<PatientPage> SearchPatientsAsync(PatientSearchRequest r, CancellationToken ct)
    {
        var page = Math.Max(1, r.Page); var pageSize = Math.Clamp(r.PageSize, 1, 100);
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(r.Search)) { var term = r.Search.Trim(); query = query.Where(x => EF.Functions.ILike(x.Name, $"%{term}%") || EF.Functions.ILike(x.Phone, $"%{term}%") || (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%"))); }
        if (!string.IsNullOrWhiteSpace(r.ConsentStatus) && Enum.TryParse<ConsentStatus>(r.ConsentStatus, true, out var consent)) query = query.Where(x => x.ConsentStatus == consent);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new PatientListItem(x.Id, x.Name, x.Phone, x.Email, x.BirthDate, x.ConsentStatus.ToString(), x.Source.ToString(), x.LastContactAt)).ToListAsync(ct);
        return new(items, page, pageSize, total);
    }
    public async Task<PatientDetailResponse> GetPatientDetailAsync(Guid id, CancellationToken ct)
    {
        var patient = await PatientById(id, ct); var tenantId = guard.RequireTenantId(); var now = DateTimeOffset.UtcNow;
        var appointments = await db.Appointments.Where(x => x.PatientId == id && x.StartsAt >= now && x.Status != AppointmentStatus.Cancelled).OrderBy(x => x.StartsAt).Take(20).Select(x => new PatientAppointmentSummary(x.Id, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString())).ToListAsync(ct);
        var conversations = await db.Conversations.Where(x => x.PatientId == id).OrderByDescending(x => x.LastMessageAt).Take(20).Select(x => new PatientConversationSummary(x.Id, x.Status.ToString(), x.AutomationMode.ToString(), x.LastMessageAt)).ToListAsync(ct);
        var audit = await db.AuditRecords.AsNoTracking().Where(x => x.TenantId == tenantId && x.ResourceType == "Patient" && x.ResourceId == id).OrderByDescending(x => x.CreatedAt).Take(20).Select(x => new PatientAuditSummary(x.CreatedAt, x.Action, x.Result)).ToListAsync(ct);
        return new(Map(patient), patient.Source.ToString(), patient.FirstContactAt, patient.LastContactAt, appointments, conversations, audit);
    }
    public async Task<PatientResponse> CreatePatientAsync(PatientRequest r, CancellationToken ct) { var tenantId = guard.RequireTenantId(); var p = new Patient(tenantId, r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); db.Patients.Add(p); db.AuditRecords.Add(new AuditRecord(tenantId, null, "patient.created", "Patient", p.Id, "Succeeded", "Patient created by clinic administration.")); await db.SaveChangesAsync(ct); return Map(p); }
    public async Task<PatientResponse> UpdatePatientAsync(Guid id, PatientRequest r, CancellationToken ct) { var p = await PatientById(id, ct); p.Update(r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); db.AuditRecords.Add(new AuditRecord(guard.RequireTenantId(), null, "patient.updated", "Patient", p.Id, "Succeeded", "Patient updated by clinic administration.")); await db.SaveChangesAsync(ct); return Map(p); }
    public async Task AddAvailabilityRuleAsync(Guid professionalId, AvailabilityRuleRequest r, CancellationToken ct) { await RequireProfessional(professionalId, ct); if (r.EndTime <= r.StartTime || r.SlotDurationMinutes is < 5 or > 240) throw new InvalidOperationException("Invalid availability rule."); db.AvailabilityRules.Add(new AvailabilityRule(guard.RequireTenantId(), professionalId, r.DayOfWeek, r.StartTime, r.EndTime, r.SlotDurationMinutes)); await db.SaveChangesAsync(ct); }
    public async Task AddScheduleBlockAsync(Guid professionalId, ScheduleBlockRequest r, CancellationToken ct) { await RequireProfessional(professionalId, ct); if (r.EndsAt <= r.StartsAt) throw new InvalidOperationException("Block end must be after start."); db.ScheduleBlocks.Add(new ScheduleBlock(guard.RequireTenantId(), professionalId, r.StartsAt.ToUniversalTime(), r.EndsAt.ToUniversalTime(), r.Reason)); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<AvailableSlot>> GetAvailabilityAsync(Guid professionalId, DateOnly appointmentDate, CancellationToken ct)
    {
        await RequireProfessional(professionalId, ct); var rules = await db.AvailabilityRules.Where(x => x.ProfessionalId == professionalId && x.Active && x.DayOfWeek == appointmentDate.DayOfWeek).ToListAsync(ct);
        var dayStart = new DateTimeOffset(appointmentDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); var dayEnd = dayStart.AddDays(1);
        var busy = await db.Appointments.Where(x => x.ProfessionalId == professionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < dayEnd && x.EndsAt > dayStart).Select(x => new { x.StartsAt, x.EndsAt }).ToListAsync(ct);
        var blocks = await db.ScheduleBlocks.Where(x => x.ProfessionalId == professionalId && x.StartsAt < dayEnd && x.EndsAt > dayStart).Select(x => new { x.StartsAt, x.EndsAt }).ToListAsync(ct);
        var slots = new List<AvailableSlot>(); foreach (var rule in rules) for (var start = new DateTimeOffset(appointmentDate.ToDateTime(rule.StartTime), TimeSpan.Zero); start.AddMinutes(rule.SlotDurationMinutes) <= new DateTimeOffset(appointmentDate.ToDateTime(rule.EndTime), TimeSpan.Zero); start = start.AddMinutes(rule.SlotDurationMinutes)) { var end = start.AddMinutes(rule.SlotDurationMinutes); if (!busy.Concat(blocks).Any(x => x.StartsAt < end && x.EndsAt > start)) slots.Add(new(start, end)); } return slots;
    }
    public async Task<IReadOnlyList<AppointmentListItem>> GetAppointmentsAsync(DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct)
    {
        if (endsAt <= startsAt) throw new InvalidOperationException("Appointment period is invalid.");
        return await db.Appointments.Where(x => x.StartsAt < endsAt.ToUniversalTime() && x.EndsAt > startsAt.ToUniversalTime()).OrderBy(x => x.StartsAt).Select(x => new AppointmentListItem(x.Id, x.ClinicUnitId, x.ProfessionalId, x.SpecialtyId, x.PatientId, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString(), x.Notes)).ToListAsync(ct);
    }
    public async Task<AppointmentResponse> CreateAppointmentAsync(AppointmentRequest r, CancellationToken ct)
    {
        if (r.EndsAt <= r.StartsAt) throw new InvalidOperationException("Appointment end must be after start."); var tenant = guard.RequireTenantId(); await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await db.Professionals.AnyAsync(x => x.Id == r.ProfessionalId && x.ClinicUnitId == r.ClinicUnitId, ct) || !await db.Patients.AnyAsync(x => x.Id == r.PatientId, ct) || !await db.Specialties.AnyAsync(x => x.Id == r.SpecialtyId, ct)) throw new KeyNotFoundException("Appointment references are invalid.");
        var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime(); var conflict = await db.Appointments.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct) || await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct); if (conflict) throw new SchedulingConflictException("The selected slot is no longer available.");
        var a = new Appointment(tenant, r.ClinicUnitId, r.ProfessionalId, r.SpecialtyId, r.PatientId, start, end, Enum.Parse<AppointmentSource>(r.Source, true), r.Notes); db.Appointments.Add(a); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); await events.PublishAsync(tenant, "appointment.created", new { a.Id }, ct); return Map(a);
    }
    public async Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken ct) { var a = await AppointmentById(id, ct); a.Confirm(); await db.SaveChangesAsync(ct); await events.PublishAsync(guard.RequireTenantId(), "appointment.updated", new { a.Id }, ct); return Map(a); }
    public async Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, CancellationToken ct) { var a = await AppointmentById(id, ct); a.Cancel(r.Reason); await db.SaveChangesAsync(ct); await events.PublishAsync(guard.RequireTenantId(), "appointment.cancelled", new { a.Id }, ct); return Map(a); }
    private async Task<Patient> PatientById(Guid id, CancellationToken ct) => await db.Patients.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Patient not found.");
    private async Task<Appointment> AppointmentById(Guid id, CancellationToken ct) => await db.Appointments.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Appointment not found.");
    private async Task RequireProfessional(Guid id, CancellationToken ct) { if (!await db.Professionals.AnyAsync(x => x.Id == id, ct)) throw new KeyNotFoundException("Professional not found."); }
    private static ConsentStatus ParseConsent(string value) => Enum.TryParse<ConsentStatus>(value, true, out var status) ? status : throw new InvalidOperationException("Invalid consent status.");
    private static PatientResponse Map(Patient x) => new(x.Id, x.Name, x.Phone, x.Email, x.BirthDate, x.ConsentStatus.ToString());
    private static AppointmentResponse Map(Appointment x) => new(x.Id, x.PatientId, x.ProfessionalId, x.StartsAt, x.EndsAt, x.Status.ToString());
}
