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
    public async Task SetTenantStatusAsync(Guid tenantId, string action, CancellationToken ct) { var tenant = await db.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, ct) ?? throw new KeyNotFoundException("Tenant not found."); switch (action.ToLowerInvariant()) { case "activate": tenant.Activate(); break; case "suspend": tenant.Suspend(); break; case "disable": tenant.Disable(); break; default: throw new InvalidOperationException("Invalid tenant action."); } var auditAction = $"tenant.{action}"; db.AuditRecords.Add(new AuditRecord(tenantId, null, auditAction, "Tenant", tenantId, "Succeeded", "Platform status change")); await db.SaveChangesAsync(ct); await PublishAuditAsync(tenantId, auditAction, tenantId, ct); }
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
    private Task PublishAuditAsync(Guid tenantId, string action, Guid resourceId, CancellationToken ct) => events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = "Tenant", ResourceId = resourceId, Result = "Succeeded" }, ct);
}
