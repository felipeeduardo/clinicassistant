using System.Text.Json;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Contracts.Platform;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Platform;

public sealed class PlatformAdministrationService(ClinicAssistantDbContext db, IOperationalEventPublisher events) : IPlatformAdministrationService
{
    public async Task<IReadOnlyList<PlatformTenantResponse>> GetTenantsAsync(CancellationToken ct) => await db.Tenants.IgnoreQueryFilters().OrderBy(x => x.Name).Select(x => new PlatformTenantResponse(x.Id, x.Name, x.Slug, x.Status.ToString(), db.Clinics.IgnoreQueryFilters().Where(c => c.TenantId == x.Id).Select(c => (Guid?)c.Id).FirstOrDefault(), db.Users.IgnoreQueryFilters().Count(u => u.TenantId == x.Id))).ToListAsync(ct);
    public async Task<IReadOnlyList<PlatformUserResponse>> GetUsersAsync(CancellationToken ct) => await db.Users.IgnoreQueryFilters().OrderBy(x => x.Email).Select(x => new PlatformUserResponse(x.Id, x.TenantId, x.Name, x.Email, x.Role.ToString(), x.Status.ToString())).ToListAsync(ct);
    public async Task<IReadOnlyList<PlatformClinicResponse>> GetClinicsAsync(CancellationToken ct) => await db.Clinics.IgnoreQueryFilters().OrderBy(x => x.TradeName).Select(x => new PlatformClinicResponse(x.Id, x.TenantId, x.TradeName, x.LegalName, x.Status.ToString())).ToListAsync(ct);
    public async Task SetTenantStatusAsync(Guid tenantId, string action, CancellationToken ct)
    {
        var tenant = await db.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, ct) ?? throw new KeyNotFoundException("Tenant not found.");
        switch (action.ToLowerInvariant())
        {
            case "activate":
                var readiness = await GetOnboardingStatusAsync(tenantId, ct);
                if (!readiness.CanActivate) throw new InvalidOperationException("The clinic is not ready for activation. Complete the required onboarding steps first.");
                tenant.Activate();
                break;
            case "suspend": tenant.Suspend(); break;
            case "disable": tenant.Disable(); break;
            default: throw new InvalidOperationException("Invalid tenant action.");
        }
        var auditAction = $"tenant.{action}";
        db.AuditRecords.Add(new AuditRecord(tenantId, null, auditAction, "Tenant", tenantId, "Succeeded", "Platform status change"));
        await db.SaveChangesAsync(ct);
        await PublishAuditAsync(tenantId, auditAction, tenantId, ct);
    }
    public async Task<OnboardTenantResponse> OnboardAsync(OnboardTenantRequest r, string key, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Idempotency-Key is required.");
            var scope = "platform.onboard"; var replay = await db.IdempotencyRecords.SingleOrDefaultAsync(x => x.Scope == scope && x.Key == key, ct); if (replay is not null) return JsonSerializer.Deserialize<OnboardTenantResponse>(replay.ResponseJson)! with { Replayed = true };
            var slug = r.TenantSlug.Trim().ToLowerInvariant(); var email = r.AdminEmail.Trim().ToLowerInvariant(); if (await db.Tenants.AnyAsync(x => x.Slug == slug, ct)) throw new InvalidOperationException("Tenant slug is already in use."); if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == email, ct)) throw new InvalidOperationException("Administrator email is already in use.");
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var tenant = new Tenant(r.TenantName.Trim(), slug); var clinic = new Clinic(tenant.Id, r.ClinicLegalName, r.ClinicTradeName, r.ClinicDocument, r.ClinicEmail, r.ClinicPhone, r.TimeZone); var unit = new ClinicUnit(tenant.Id, clinic.Id, r.UnitName, r.UnitAddress, r.UnitPhone); var admin = new User(tenant.Id, r.AdminName, email, PasswordHasher.Hash(r.TemporaryPassword), UserRole.ClinicAdmin); var integration = new WhatsAppIntegration(tenant.Id, WhatsAppProvider.Fake, $"onboarding-{tenant.Id:N}", "disabled"); integration.Disable(); var response = new OnboardTenantResponse(tenant.Id, clinic.Id, unit.Id, admin.Id, integration.Id, false);
            db.AddRange(tenant, clinic, unit, admin, integration, new AuditRecord(tenant.Id, null, "tenant.onboard", "Tenant", tenant.Id, "Succeeded", "Transactional onboarding"), new IdempotencyRecord(scope, key, JsonSerializer.Serialize(response))); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); OperationalTelemetry.PlatformOnboarding.Add(1); await PublishAuditAsync(tenant.Id, "tenant.onboard", tenant.Id, ct); return response;
        }
        catch
        {
            OperationalTelemetry.PlatformOnboardingFailures.Add(1);
            throw;
        }
    }
    public async Task<PlatformOnboardingStatusResponse> GetOnboardingStatusAsync(Guid tenantId, CancellationToken ct)
    {
        if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId, ct)) throw new KeyNotFoundException("Tenant not found.");
        var clinicConfigured = await db.Clinics.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId, ct);
        var clinicId = await db.Clinics.IgnoreQueryFilters().Where(x => x.TenantId == tenantId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        var unitConfigured = clinicId.HasValue && await db.ClinicUnits.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.ClinicId == clinicId.Value, ct);
        var specialtiesConfigured = await db.Specialties.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId, ct);
        var professionalsConfigured = await db.Professionals.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId, ct);
        var availabilityConfigured = await db.AvailabilityRules.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId, ct);
        var clinicAdminConfigured = await db.Users.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Role == UserRole.ClinicAdmin && x.Status == UserStatus.Active, ct);
        var whatsAppConfigured = await db.WhatsAppIntegrations.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Status != WhatsAppIntegrationStatus.Disabled, ct);
        return new PlatformOnboardingStatusResponse(tenantId, clinicConfigured, unitConfigured, specialtiesConfigured, professionalsConfigured, availabilityConfigured, clinicAdminConfigured, whatsAppConfigured, clinicConfigured && unitConfigured && specialtiesConfigured && professionalsConfigured && availabilityConfigured && clinicAdminConfigured);
    }
    public async Task<PlatformUserResponse> CreateClinicAdminAsync(Guid tenantId, CreateClinicAdminRequest request, string idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required.");
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.TemporaryPassword)) throw new InvalidOperationException("Clinic administrator name, email and password are required.");
        if (request.TemporaryPassword.Length < 12) throw new InvalidOperationException("Clinic administrator password must contain at least 12 characters.");
        if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId, ct)) throw new KeyNotFoundException("Tenant not found.");
        var scope = $"platform.clinic-admin:{tenantId}";
        var replay = await db.IdempotencyRecords.SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct);
        if (replay is not null) return JsonSerializer.Deserialize<PlatformUserResponse>(replay.ResponseJson) ?? throw new InvalidOperationException("Stored idempotency response is invalid.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == email, ct)) throw new InvalidOperationException("This email is already in use.");
        var user = new User(tenantId, request.Name.Trim(), email, PasswordHasher.Hash(request.TemporaryPassword), UserRole.ClinicAdmin);
        var response = new PlatformUserResponse(user.Id, tenantId, user.Name, user.Email, user.Role.ToString(), user.Status.ToString());
        db.AddRange(user, new AuditRecord(tenantId, null, "clinic_admin.created", "User", user.Id, "Succeeded", "Clinic administrator created by platform administration."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response)));
        await db.SaveChangesAsync(ct);
        await PublishAuditAsync(tenantId, "clinic_admin.created", user.Id, ct);
        return response;
    }
    private Task PublishAuditAsync(Guid tenantId, string action, Guid resourceId, CancellationToken ct) => events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = "Tenant", ResourceId = resourceId, Result = "Succeeded" }, ct);
}
