using System.Data;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Scheduling;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Scheduling;

public sealed class SchedulingService(ClinicAssistantDbContext db, TenantAccessGuard guard) : ISchedulingService
{
    public async Task<IReadOnlyList<PatientResponse>> GetPatientsAsync(CancellationToken ct) => await db.Patients.OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);
    public async Task<PatientResponse> CreatePatientAsync(PatientRequest r, CancellationToken ct) { var p = new Patient(guard.RequireTenantId(), r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); db.Patients.Add(p); await db.SaveChangesAsync(ct); return Map(p); }
    public async Task<PatientResponse> UpdatePatientAsync(Guid id, PatientRequest r, CancellationToken ct) { var p = await PatientById(id, ct); p.Update(r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); await db.SaveChangesAsync(ct); return Map(p); }
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
    public async Task<AppointmentResponse> CreateAppointmentAsync(AppointmentRequest r, CancellationToken ct)
    {
        if (r.EndsAt <= r.StartsAt) throw new InvalidOperationException("Appointment end must be after start."); var tenant = guard.RequireTenantId(); await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await db.Professionals.AnyAsync(x => x.Id == r.ProfessionalId && x.ClinicUnitId == r.ClinicUnitId, ct) || !await db.Patients.AnyAsync(x => x.Id == r.PatientId, ct) || !await db.Specialties.AnyAsync(x => x.Id == r.SpecialtyId, ct)) throw new KeyNotFoundException("Appointment references are invalid.");
        var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime(); var conflict = await db.Appointments.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct) || await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct); if (conflict) throw new InvalidOperationException("The selected slot is no longer available.");
        var a = new Appointment(tenant, r.ClinicUnitId, r.ProfessionalId, r.SpecialtyId, r.PatientId, start, end, Enum.Parse<AppointmentSource>(r.Source, true), r.Notes); db.Appointments.Add(a); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return Map(a);
    }
    public async Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken ct) { var a = await AppointmentById(id, ct); a.Confirm(); await db.SaveChangesAsync(ct); return Map(a); }
    public async Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, CancellationToken ct) { var a = await AppointmentById(id, ct); a.Cancel(r.Reason); await db.SaveChangesAsync(ct); return Map(a); }
    private async Task<Patient> PatientById(Guid id, CancellationToken ct) => await db.Patients.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Patient not found.");
    private async Task<Appointment> AppointmentById(Guid id, CancellationToken ct) => await db.Appointments.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Appointment not found.");
    private async Task RequireProfessional(Guid id, CancellationToken ct) { if (!await db.Professionals.AnyAsync(x => x.Id == id, ct)) throw new KeyNotFoundException("Professional not found."); }
    private static ConsentStatus ParseConsent(string value) => Enum.TryParse<ConsentStatus>(value, true, out var status) ? status : throw new InvalidOperationException("Invalid consent status.");
    private static PatientResponse Map(Patient x) => new(x.Id, x.Name, x.Phone, x.Email, x.BirthDate, x.ConsentStatus.ToString());
    private static AppointmentResponse Map(Appointment x) => new(x.Id, x.PatientId, x.ProfessionalId, x.StartsAt, x.EndsAt, x.Status.ToString());
}
