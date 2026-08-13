using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Scheduling;

public enum ConsentStatus { Unknown, Granted, Revoked }
public enum PatientSource { Manual, WhatsApp }
public enum AppointmentStatus { Pending, Confirmed, Cancelled, Completed, NoShow, Rescheduled }
public enum AppointmentSource { Dashboard, WhatsApp, Reception }

public sealed class Patient : Entity, ITenantEntity
{
    private Patient() { }
    public Patient(Guid tenantId, string name, string phone, string? email, DateOnly? birthDate, ConsentStatus consentStatus)
        : this(tenantId, name, phone, email, birthDate, consentStatus, PatientSource.Manual) { }
    public Patient(Guid tenantId, string name, string phone, string? email, DateOnly? birthDate, ConsentStatus consentStatus, PatientSource source)
    { TenantId = tenantId; Source = source; Update(name, phone, email, birthDate, consentStatus); }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string? Email { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public ConsentStatus ConsentStatus { get; private set; }
    public PatientSource Source { get; private set; }
    public DateTimeOffset? FirstContactAt { get; private set; }
    public DateTimeOffset? LastContactAt { get; private set; }
    public void Update(string name, string phone, string? email, DateOnly? birthDate, ConsentStatus consentStatus) { Name = name; Phone = phone; Email = email; BirthDate = birthDate; ConsentStatus = consentStatus; UpdatedAt = DateTimeOffset.UtcNow; }
    public void RegisterContact(DateTimeOffset occurredAt) { FirstContactAt ??= occurredAt; LastContactAt = occurredAt; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class AvailabilityRule : Entity, ITenantEntity
{
    private AvailabilityRule() { }
    public AvailabilityRule(Guid tenantId, Guid professionalId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes)
    { TenantId = tenantId; ProfessionalId = professionalId; DayOfWeek = dayOfWeek; StartTime = startTime; EndTime = endTime; SlotDurationMinutes = slotDurationMinutes; }
    public Guid TenantId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public bool Active { get; private set; } = true;
    public void Update(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes, bool active) { DayOfWeek = dayOfWeek; StartTime = startTime; EndTime = endTime; SlotDurationMinutes = slotDurationMinutes; Active = active; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class ScheduleBlock : Entity, ITenantEntity
{
    private ScheduleBlock() { }
    public ScheduleBlock(Guid tenantId, Guid professionalId, DateTimeOffset startsAt, DateTimeOffset endsAt, string? reason) { TenantId = tenantId; ProfessionalId = professionalId; StartsAt = startsAt; EndsAt = endsAt; Reason = reason; }
    public Guid TenantId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string? Reason { get; private set; }
}

public sealed class ProfessionalVacation : Entity, ITenantEntity
{
    private ProfessionalVacation() { }
    public ProfessionalVacation(Guid tenantId, Guid professionalId, DateTimeOffset startsAt, DateTimeOffset endsAt, string? reason)
    { TenantId = tenantId; ProfessionalId = professionalId; StartsAt = startsAt; EndsAt = endsAt; Reason = reason; }
    public Guid TenantId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string? Reason { get; private set; }
}

public sealed class Appointment : Entity, ITenantEntity
{
    private Appointment() { }
    public Appointment(Guid tenantId, Guid clinicUnitId, Guid professionalId, Guid specialtyId, Guid patientId, DateTimeOffset startsAt, DateTimeOffset endsAt, AppointmentSource source, string? notes)
    { TenantId = tenantId; ClinicUnitId = clinicUnitId; ProfessionalId = professionalId; SpecialtyId = specialtyId; PatientId = patientId; StartsAt = startsAt; EndsAt = endsAt; Source = source; Notes = notes; }
    public Guid TenantId { get; private set; }
    public Guid ClinicUnitId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid SpecialtyId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;
    public AppointmentSource Source { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public int Version { get; private set; } = 1;
    public void Confirm() { if (Status != AppointmentStatus.Pending) throw new InvalidOperationException("Only pending appointments can be confirmed."); Status = AppointmentStatus.Confirmed; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Cancel(string reason) { if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed) throw new InvalidOperationException("This appointment cannot be cancelled."); Status = AppointmentStatus.Cancelled; CancelledAt = DateTimeOffset.UtcNow; CancellationReason = reason; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkRescheduled() { if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.Rescheduled) throw new InvalidOperationException("This appointment cannot be rescheduled."); Status = AppointmentStatus.Rescheduled; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
}
