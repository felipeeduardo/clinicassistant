using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Clinics;

public enum CatalogStatus { Active, Inactive }

public sealed class Clinic : Entity, ITenantEntity
{
    private Clinic() { }
    public Clinic(Guid tenantId, string legalName, string tradeName, string document, string email, string phone, string timeZone)
    { TenantId = tenantId; Update(legalName, tradeName, document, email, phone, timeZone); }
    public Guid TenantId { get; private set; }
    public string LegalName { get; private set; } = null!;
    public string TradeName { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string TimeZone { get; private set; } = null!;
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public void Update(string legalName, string tradeName, string document, string email, string phone, string timeZone)
    { LegalName = legalName; TradeName = tradeName; Document = document; Email = email; Phone = phone; TimeZone = timeZone; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class ClinicUnit : Entity, ITenantEntity
{
    private ClinicUnit() { }
    public ClinicUnit(Guid tenantId, Guid clinicId, string name, string address, string phone)
    { TenantId = tenantId; ClinicId = clinicId; Update(name, address, phone); }
    public Guid TenantId { get; private set; }
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public void Update(string name, string address, string phone) { Name = name; Address = address; Phone = phone; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetStatus(CatalogStatus status) { Status = status; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class UnitBusinessHour : Entity, ITenantEntity
{
    private UnitBusinessHour() { }
    public UnitBusinessHour(Guid tenantId, Guid clinicUnitId, DayOfWeek dayOfWeek, TimeOnly opensAt, TimeOnly closesAt)
    { TenantId = tenantId; ClinicUnitId = clinicUnitId; DayOfWeek = dayOfWeek; OpensAt = opensAt; ClosesAt = closesAt; }
    public Guid TenantId { get; private set; }
    public Guid ClinicUnitId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpensAt { get; private set; }
    public TimeOnly ClosesAt { get; private set; }
}

public sealed class Specialty : Entity, ITenantEntity
{
    private Specialty() { }
    public Specialty(Guid tenantId, string name, string? description) { TenantId = tenantId; Update(name, description); }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public void Update(string name, string? description) { Name = name; Description = description; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class Professional : Entity, ITenantEntity
{
    private Professional() { }
    public Professional(Guid tenantId, Guid clinicUnitId, string name, string email, string phone, string registrationNumber)
    { TenantId = tenantId; ClinicUnitId = clinicUnitId; Update(clinicUnitId, name, email, phone, registrationNumber); }
    public Guid TenantId { get; private set; }
    public Guid ClinicUnitId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string RegistrationNumber { get; private set; } = null!;
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public ICollection<ProfessionalSpecialty> Specialties { get; } = new List<ProfessionalSpecialty>();
    public void Update(Guid clinicUnitId, string name, string email, string phone, string registrationNumber)
    { ClinicUnitId = clinicUnitId; Name = name; Email = email; Phone = phone; RegistrationNumber = registrationNumber; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class ProfessionalSpecialty
{
    public ProfessionalSpecialty(Guid professionalId, Guid specialtyId) { ProfessionalId = professionalId; SpecialtyId = specialtyId; }
    public Guid ProfessionalId { get; private set; }
    public Guid SpecialtyId { get; private set; }
    public Professional Professional { get; private set; } = null!;
    public Specialty Specialty { get; private set; } = null!;
}
