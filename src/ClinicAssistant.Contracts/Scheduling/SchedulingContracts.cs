namespace ClinicAssistant.Contracts.Scheduling;

public sealed record PatientRequest(string Name, string Phone, string? Email, DateOnly? BirthDate, string ConsentStatus);
public sealed record AvailabilityRuleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, int SlotDurationMinutes, bool Active);
public sealed record ScheduleBlockRequest(DateTimeOffset StartsAt, DateTimeOffset EndsAt, string? Reason);
public sealed record AppointmentRequest(Guid ClinicUnitId, Guid ProfessionalId, Guid SpecialtyId, Guid PatientId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Source, string? Notes);
public sealed record CancelAppointmentRequest(string Reason);
public sealed record PatientResponse(Guid Id, string Name, string Phone, string? Email, DateOnly? BirthDate, string ConsentStatus);
public sealed record AppointmentResponse(Guid Id, Guid PatientId, Guid ProfessionalId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status);
public sealed record AppointmentListItem(Guid Id, Guid ClinicUnitId, Guid ProfessionalId, Guid SpecialtyId, Guid PatientId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status, string Source, string? Notes);
public sealed record AvailableSlot(DateTimeOffset StartsAt, DateTimeOffset EndsAt);
