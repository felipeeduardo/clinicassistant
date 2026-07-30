using ClinicAssistant.Application.Clinics;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Contracts.Clinics;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Clinics;

public sealed class ClinicCatalogService(ClinicAssistantDbContext db, TenantAccessGuard tenantGuard) : IClinicCatalogService
{
    public async Task<ClinicResponse?> GetClinicAsync(CancellationToken ct) => (await db.Clinics.SingleOrDefaultAsync(ct)) is { } clinic ? Map(clinic) : null;
    public async Task<ClinicResponse> UpdateClinicAsync(ClinicRequest request, CancellationToken ct)
    {
        await new ClinicRequestValidator().ValidateAndThrowAsync(request, ct); var tenantId = tenantGuard.RequireTenantId();
        var clinic = await db.Clinics.SingleOrDefaultAsync(ct);
        if (clinic is null)
        {
            clinic = new Clinic(tenantId, request.LegalName, request.TradeName, request.Document, request.Email, request.Phone, request.TimeZone);
            db.Clinics.Add(clinic);
        }
        else clinic.Update(request.LegalName, request.TradeName, request.Document, request.Email, request.Phone, request.TimeZone);
        await db.SaveChangesAsync(ct); return Map(clinic);
    }
    public async Task<IReadOnlyList<UnitResponse>> GetUnitsAsync(CancellationToken ct) => await db.ClinicUnits.OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);
    public async Task<UnitResponse?> GetUnitAsync(Guid id, CancellationToken ct) => (await db.ClinicUnits.FindAsync([id], ct)) is { } unit ? Map(unit) : null;
    public async Task<UnitResponse> CreateUnitAsync(UnitRequest request, CancellationToken ct)
    {
        await new UnitRequestValidator().ValidateAndThrowAsync(request, ct); var tenantId = tenantGuard.RequireTenantId(); var clinic = await db.Clinics.SingleOrDefaultAsync(ct) ?? throw new InvalidOperationException("Configure the clinic before adding units.");
        var unit = new ClinicUnit(tenantId, clinic.Id, request.Name, request.Address, request.Phone); db.ClinicUnits.Add(unit); await db.SaveChangesAsync(ct); return Map(unit);
    }
    public async Task<UnitResponse> UpdateUnitAsync(Guid id, UnitRequest request, CancellationToken ct) { await new UnitRequestValidator().ValidateAndThrowAsync(request, ct); var unit = await Require(db.ClinicUnits, id, ct); unit.Update(request.Name, request.Address, request.Phone); await db.SaveChangesAsync(ct); return Map(unit); }
    public async Task DeleteUnitAsync(Guid id, CancellationToken ct) { db.ClinicUnits.Remove(await Require(db.ClinicUnits, id, ct)); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<SpecialtyResponse>> GetSpecialtiesAsync(CancellationToken ct) => await db.Specialties.OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);
    public async Task<SpecialtyResponse> CreateSpecialtyAsync(SpecialtyRequest request, CancellationToken ct) { await new SpecialtyRequestValidator().ValidateAndThrowAsync(request, ct); var entity = new Specialty(tenantGuard.RequireTenantId(), request.Name, request.Description); db.Specialties.Add(entity); await db.SaveChangesAsync(ct); return Map(entity); }
    public async Task<SpecialtyResponse> UpdateSpecialtyAsync(Guid id, SpecialtyRequest request, CancellationToken ct) { await new SpecialtyRequestValidator().ValidateAndThrowAsync(request, ct); var entity = await Require(db.Specialties, id, ct); entity.Update(request.Name, request.Description); await db.SaveChangesAsync(ct); return Map(entity); }
    public async Task DeleteSpecialtyAsync(Guid id, CancellationToken ct) { db.Specialties.Remove(await Require(db.Specialties, id, ct)); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<ProfessionalResponse>> GetProfessionalsAsync(CancellationToken ct) => (await db.Professionals.Include(x => x.Specialties).OrderBy(x => x.Name).ToListAsync(ct)).Select(Map).ToList();
    public async Task<ProfessionalResponse?> GetProfessionalAsync(Guid id, CancellationToken ct) => (await db.Professionals.Include(x => x.Specialties).SingleOrDefaultAsync(x => x.Id == id, ct)) is { } p ? Map(p) : null;
    public async Task<ProfessionalResponse> CreateProfessionalAsync(ProfessionalRequest request, CancellationToken ct) { await new ProfessionalRequestValidator().ValidateAndThrowAsync(request, ct); var p = await BuildProfessional(request, null, ct); db.Professionals.Add(p); await db.SaveChangesAsync(ct); return Map(p); }
    public async Task<ProfessionalResponse> UpdateProfessionalAsync(Guid id, ProfessionalRequest request, CancellationToken ct) { await new ProfessionalRequestValidator().ValidateAndThrowAsync(request, ct); if (!await db.ClinicUnits.AnyAsync(x => x.Id == request.ClinicUnitId, ct)) throw new InvalidOperationException("Clinic unit not found."); var p = await Require(db.Professionals.Include(x => x.Specialties), id, ct); p.Update(request.ClinicUnitId, request.Name, request.Email, request.Phone, request.RegistrationNumber); p.Specialties.Clear(); await SetSpecialties(p, request.SpecialtyIds, ct); await db.SaveChangesAsync(ct); return Map(p); }
    public async Task DeleteProfessionalAsync(Guid id, CancellationToken ct) { db.Professionals.Remove(await Require(db.Professionals, id, ct)); await db.SaveChangesAsync(ct); }
    private async Task<Professional> BuildProfessional(ProfessionalRequest r, Professional? ignored, CancellationToken ct) { var tenant = tenantGuard.RequireTenantId(); if (!await db.ClinicUnits.AnyAsync(x => x.Id == r.ClinicUnitId, ct)) throw new InvalidOperationException("Clinic unit not found."); var p = new Professional(tenant, r.ClinicUnitId, r.Name, r.Email, r.Phone, r.RegistrationNumber); await SetSpecialties(p, r.SpecialtyIds, ct); return p; }
    private async Task SetSpecialties(Professional p, IReadOnlyCollection<Guid> ids, CancellationToken ct) { var unique = ids.Distinct().ToArray(); if (unique.Length != ids.Count || await db.Specialties.CountAsync(x => unique.Contains(x.Id), ct) != unique.Length) throw new InvalidOperationException("One or more specialties are invalid for this tenant."); foreach (var id in unique) p.Specialties.Add(new ProfessionalSpecialty(p.Id, id)); }
    private static async Task<T> Require<T>(IQueryable<T> query, Guid id, CancellationToken ct) where T : class => await query.SingleOrDefaultAsync(x => EF.Property<Guid>(x, "Id") == id, ct) ?? throw new KeyNotFoundException("Resource not found.");
    private static ClinicResponse Map(Clinic x) => new(x.Id, x.LegalName, x.TradeName, x.Document, x.Email, x.Phone, x.TimeZone, x.Status.ToString());
    private static UnitResponse Map(ClinicUnit x) => new(x.Id, x.ClinicId, x.Name, x.Address, x.Phone, x.Status.ToString());
    private static SpecialtyResponse Map(Specialty x) => new(x.Id, x.Name, x.Description, x.Status.ToString());
    private static ProfessionalResponse Map(Professional x) => new(x.Id, x.ClinicUnitId, x.Name, x.Email, x.Phone, x.RegistrationNumber, x.Status.ToString(), x.Specialties.Select(s => s.SpecialtyId).ToList());
}

file sealed class ClinicRequestValidator : AbstractValidator<ClinicRequest> { public ClinicRequestValidator() { RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200); RuleFor(x => x.TradeName).NotEmpty().MaximumLength(200); RuleFor(x => x.Document).NotEmpty().MaximumLength(32); RuleFor(x => x.Email).EmailAddress().MaximumLength(320); RuleFor(x => x.Phone).NotEmpty().MaximumLength(32); RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100); } }
file sealed class UnitRequestValidator : AbstractValidator<UnitRequest> { public UnitRequestValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Address).NotEmpty().MaximumLength(500); RuleFor(x => x.Phone).NotEmpty().MaximumLength(32); } }
file sealed class SpecialtyRequestValidator : AbstractValidator<SpecialtyRequest> { public SpecialtyRequestValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(160); RuleFor(x => x.Description).MaximumLength(1000); } }
file sealed class ProfessionalRequestValidator : AbstractValidator<ProfessionalRequest> { public ProfessionalRequestValidator() { RuleFor(x => x.ClinicUnitId).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Email).EmailAddress().MaximumLength(320); RuleFor(x => x.Phone).NotEmpty().MaximumLength(32); RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(80); RuleFor(x => x.SpecialtyIds).NotEmpty(); } }
