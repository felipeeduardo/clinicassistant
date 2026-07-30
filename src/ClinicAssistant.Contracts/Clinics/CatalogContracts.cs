namespace ClinicAssistant.Contracts.Clinics;

public sealed record ClinicRequest(string LegalName, string TradeName, string Document, string Email, string Phone, string TimeZone);
public sealed record UnitRequest(string Name, string Address, string Phone);
public sealed record SpecialtyRequest(string Name, string? Description);
public sealed record ProfessionalRequest(Guid ClinicUnitId, string Name, string Email, string Phone, string RegistrationNumber, IReadOnlyCollection<Guid> SpecialtyIds);
public sealed record ClinicResponse(Guid Id, string LegalName, string TradeName, string Document, string Email, string Phone, string TimeZone, string Status);
public sealed record UnitResponse(Guid Id, Guid ClinicId, string Name, string Address, string Phone, string Status);
public sealed record SpecialtyResponse(Guid Id, string Name, string? Description, string Status);
public sealed record ProfessionalResponse(Guid Id, Guid ClinicUnitId, string Name, string Email, string Phone, string RegistrationNumber, string Status, IReadOnlyCollection<Guid> SpecialtyIds);
