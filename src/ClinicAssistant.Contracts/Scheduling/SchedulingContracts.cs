namespace ClinicAssistant.Contracts.Scheduling;

public sealed record PatientRequest(string Name, string Phone, string? Email, DateOnly? BirthDate, string ConsentStatus);
public sealed record AvailabilityRuleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, int SlotDurationMinutes, bool Active);
public sealed record ScheduleBlockRequest(DateTimeOffset StartsAt, DateTimeOffset EndsAt, string? Reason);
public sealed record AppointmentRequest(Guid ClinicUnitId, Guid ProfessionalId, Guid SpecialtyId, Guid PatientId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Source, string? Notes);
public sealed record CancelAppointmentRequest(string Reason);
public sealed record PatientResponse(Guid Id, string Name, string Phone, string? Email, DateOnly? BirthDate, string ConsentStatus);
public sealed record PatientSearchRequest(int Page = 1, int PageSize = 25, string? Search = null, string? ConsentStatus = null);
public sealed record PatientListItem(Guid Id, string Name, string Phone, string? Email, DateOnly? BirthDate, string ConsentStatus, string Source, DateTimeOffset? LastContactAt);
public sealed record PatientPage(IReadOnlyList<PatientListItem> Items, int Page, int PageSize, int TotalCount);
public sealed record PatientAppointmentSummary(Guid Id, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status, string Source);
public sealed record PatientConversationSummary(Guid Id, string Status, string AutomationMode, DateTimeOffset? LastMessageAt);
public sealed record PatientAuditSummary(DateTimeOffset OccurredAt, string Action, string Result);
public sealed record PatientDetailResponse(PatientResponse Patient, string Source, DateTimeOffset? FirstContactAt, DateTimeOffset? LastContactAt, IReadOnlyList<PatientAppointmentSummary> UpcomingAppointments, IReadOnlyList<PatientConversationSummary> Conversations, IReadOnlyList<PatientAuditSummary> RecentAudit);
public sealed record AppointmentResponse(Guid Id, Guid PatientId, Guid ProfessionalId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status);
public sealed record AppointmentListItem(Guid Id, Guid ClinicUnitId, Guid ProfessionalId, Guid SpecialtyId, Guid PatientId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status, string Source, string? Notes);
public sealed record AvailableSlot(DateTimeOffset StartsAt, DateTimeOffset EndsAt);
