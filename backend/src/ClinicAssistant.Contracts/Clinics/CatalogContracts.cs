namespace ClinicAssistant.Contracts.Clinics;

public sealed record ClinicRequest(string LegalName, string TradeName, string Document, string Email, string Phone, string TimeZone, string? AssistantDisplayName = null);
public sealed record UnitRequest(string Name, string Address, string Phone);
public sealed record SpecialtyRequest(string Name, string? Description);
public sealed record ProfessionalRequest(Guid ClinicUnitId, string Name, string Email, string Phone, string RegistrationNumber, IReadOnlyCollection<Guid> SpecialtyIds);
public sealed record ClinicResponse(Guid Id, string LegalName, string TradeName, string Document, string Email, string Phone, string TimeZone, string Status, string AssistantDisplayName = "IA Recepção");
public sealed record UnitResponse(Guid Id, Guid ClinicId, string Name, string Address, string Phone, string Status);
public sealed record UnitBusinessHourRequest(DayOfWeek DayOfWeek, TimeOnly OpensAt, TimeOnly ClosesAt);
public sealed record UnitBusinessHourResponse(DayOfWeek DayOfWeek, TimeOnly OpensAt, TimeOnly ClosesAt);
public sealed record UnitProfessionalSummary(Guid Id, string Name, string RegistrationNumber, string Status);
public sealed record UnitAuditSummary(DateTimeOffset OccurredAt, string Action, string Result);
public sealed record UnitDetailResponse(UnitResponse Unit, string TimeZone, IReadOnlyList<UnitBusinessHourResponse> BusinessHours, IReadOnlyList<UnitProfessionalSummary> Professionals, IReadOnlyList<UnitAuditSummary> RecentAudit);
public sealed record SpecialtyResponse(Guid Id, string Name, string? Description, string Status);
public sealed record SpecialtyDependenciesResponse(bool CanDeactivate, int Professionals, int FutureAppointments);
public sealed record ProfessionalResponse(Guid Id, Guid ClinicUnitId, string Name, string Email, string Phone, string RegistrationNumber, string Status, IReadOnlyCollection<Guid> SpecialtyIds);
