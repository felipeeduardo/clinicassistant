using System.Text.Json;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Contracts.Platform;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Domain.Platform;
using ClinicAssistant.Infrastructure.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicAssistant.Infrastructure.Platform;

public sealed class PlatformAdministrationService(ClinicAssistantDbContext db, IOperationalEventPublisher events, HealthCheckService healthChecks) : IPlatformAdministrationService
{
    public async Task<IReadOnlyList<PlatformTenantResponse>> GetTenantsAsync(CancellationToken ct) => await db.Tenants.IgnoreQueryFilters().OrderBy(x => x.Name).Select(x => new PlatformTenantResponse(x.Id, x.Name, x.Slug, x.Status.ToString(), db.Clinics.IgnoreQueryFilters().Where(c => c.TenantId == x.Id).Select(c => (Guid?)c.Id).FirstOrDefault(), db.Users.IgnoreQueryFilters().Count(u => u.TenantId == x.Id), db.Users.IgnoreQueryFilters().Where(u => u.TenantId == x.Id && u.Role == UserRole.ClinicAdmin).OrderBy(u => u.CreatedAt).Select(u => u.Name).FirstOrDefault(), db.Users.IgnoreQueryFilters().Where(u => u.TenantId == x.Id && u.Role == UserRole.ClinicAdmin).OrderBy(u => u.CreatedAt).Select(u => u.Email).FirstOrDefault(), db.ClinicUnits.IgnoreQueryFilters().Where(u => u.TenantId == x.Id).OrderBy(u => u.CreatedAt).Select(u => u.Name).FirstOrDefault(), x.CreatedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<PlatformUserResponse>> GetUsersAsync(CancellationToken ct) => await db.Users.IgnoreQueryFilters().OrderBy(x => x.Email).Select(x => new PlatformUserResponse(x.Id, x.TenantId, x.Name, x.Email, x.Role.ToString(), x.Status.ToString())).ToListAsync(ct);
    public async Task<IReadOnlyList<PlatformClinicResponse>> GetClinicsAsync(CancellationToken ct) => await db.Clinics.IgnoreQueryFilters().OrderBy(x => x.TradeName).Select(x => new PlatformClinicResponse(x.Id, x.TenantId, x.TradeName, x.LegalName, x.Status.ToString())).ToListAsync(ct);
    public async Task SetTenantStatusAsync(Guid tenantId, string action, CancellationToken ct)
    {
        var tenant = await db.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, ct) ?? throw new KeyNotFoundException("Tenant not found.");
        switch (action.ToLowerInvariant())
        {
            case "activate":
                var readiness = await GetOnboardingStatusAsync(tenantId, ct);
                if (!readiness.CanActivate)
                {
                    var missing = new List<string>();
                    if (!readiness.ClinicConfigured) missing.Add("Clinic");
                    if (!readiness.UnitConfigured) missing.Add("Unit");
                    if (!readiness.ClinicAdminConfigured) missing.Add("ClinicAdmin");
                    throw new ClinicNotReadyForActivationException(tenantId, missing);
                }
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
            var slug = r.TenantSlug.Trim().ToLowerInvariant(); var email = r.AdminEmail?.Trim().ToLowerInvariant(); if (await db.Tenants.AnyAsync(x => x.Slug == slug, ct)) throw new InvalidOperationException("Tenant slug is already in use."); if (email is not null && await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == email, ct)) throw new InvalidOperationException("Administrator email is already in use.");
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var tenant = new Tenant(r.TenantName.Trim(), slug);
            tenant.BeginOnboarding();
            var clinic = new Clinic(tenant.Id, r.ClinicLegalName, r.ClinicTradeName, r.ClinicDocument, r.ClinicEmail, r.ClinicPhone, r.TimeZone);
            var unit = new ClinicUnit(tenant.Id, clinic.Id, r.UnitName, r.UnitAddress, r.UnitPhone);
            if (string.IsNullOrWhiteSpace(r.AdminName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(r.TemporaryPassword)) throw new InvalidOperationException("Clinic administrator data is required for provisioning.");
            if (r.TemporaryPassword.Length < 12) throw new InvalidOperationException("Clinic administrator password must contain at least 12 characters.");
            User admin = new User(tenant.Id, r.AdminName!.Trim(), email!, PasswordHasher.Hash(r.TemporaryPassword!), UserRole.ClinicAdmin);
            var response = new OnboardTenantResponse(tenant.Id, clinic.Id, unit.Id, admin.Id, null, false);

            // The domain entities intentionally carry foreign-key IDs rather than navigation
            // properties. Persist the principal first so PostgreSQL cannot insert Clinic before
            // its Tenant when batching unrelated Added entries.
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(ct);
            db.AddRange(clinic, unit);
            db.Users.Add(admin!);
            db.AddRange(
                new AuditRecord(tenant.Id, null, "tenant.onboard", "Tenant", tenant.Id, "Succeeded", "Transactional onboarding"),
                new IdempotencyRecord(scope, key, JsonSerializer.Serialize(response)));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            OperationalTelemetry.PlatformOnboarding.Add(1);
            await PublishAuditAsync(tenant.Id, "tenant.onboard", tenant.Id, ct);
            return response;
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
        return new PlatformOnboardingStatusResponse(tenantId, clinicConfigured, unitConfigured, specialtiesConfigured, professionalsConfigured, availabilityConfigured, clinicAdminConfigured, whatsAppConfigured, clinicConfigured && unitConfigured && clinicAdminConfigured);
    }
    public async Task<PlatformWhatsAppStatusResponse> GetWhatsAppStatusAsync(Guid tenantId, CancellationToken ct)
    {
        await EnsureTenantAsync(tenantId, ct);
        var integration = await db.WhatsAppIntegrations.IgnoreQueryFilters().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(ct);
        if (integration is null) return new(tenantId, false, null, null, null, null, null, null, null);
        return new(tenantId, integration.Status != WhatsAppIntegrationStatus.Disabled, integration.Provider.ToString(), integration.Status.ToString(), MaskPhone(integration.DisplayPhoneNumber ?? integration.WhatsAppFrom), integration.LastWebhookAt, integration.LastSuccessfulSendAt, integration.LastFailureAt, integration.FailureReason);
    }
    private static string MaskPhone(string value)
    {
        var normalized = value.Trim();
        return normalized.Length <= 4 ? "••••" : $"••••{normalized[^4..]}";
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
    public async Task DeleteTenantAsync(Guid tenantId, DeleteTenantRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClinicAdminEmail) || string.IsNullOrWhiteSpace(request.Confirmation))
            throw new InvalidOperationException("Clinic administrator email and confirmation are required.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var tenant = await db.Tenants.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == tenantId, ct) ?? throw new KeyNotFoundException("Tenant not found.");
        if (string.Equals(tenant.Slug, "platform-system", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The platform tenant cannot be deleted.");
        if (!string.Equals(request.Confirmation.Trim(), tenant.Slug, StringComparison.Ordinal))
            throw new InvalidOperationException("Confirmation must match the tenant slug.");

        var adminEmail = request.ClinicAdminEmail.Trim().ToLowerInvariant();
        var adminExists = await db.Users.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Role == UserRole.ClinicAdmin && x.Status == UserStatus.Active && x.Email == adminEmail, ct);
        if (!adminExists) throw new UnauthorizedAccessException("The informed email is not an active ClinicAdmin of this tenant.");

        // Delete dependents explicitly because tenant foreign keys are intentionally
        // restrictive in the operational model. This keeps the operation atomic and
        // avoids leaving orphaned conversations, schedules or provider records.
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.whatsapp_media WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.conversation_options WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.conversation_processed_messages WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.conversation_states WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.human_queue_items WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.conversation_messages WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.conversations WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.whatsapp_templates WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.whatsapp_integrations WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.inbox_messages WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.outbox_messages WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.appointments WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.professional_specialties WHERE \"ProfessionalId\" IN (SELECT \"Id\" FROM clinic_assistant.professionals WHERE \"TenantId\" = {tenantId}) OR \"SpecialtyId\" IN (SELECT \"Id\" FROM clinic_assistant.specialties WHERE \"TenantId\" = {tenantId})", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.availability_rules WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.schedule_blocks WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.professional_vacations WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.professionals WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.specialties WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.unit_business_hours WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.clinic_units WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.patients WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.audit_records WHERE \"TenantId\" = {tenantId}", ct);
        var tenantMarker = $"%{tenantId}%";
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.idempotency_records WHERE \"Scope\" LIKE {tenantMarker} OR \"Key\" LIKE {tenantMarker} OR \"ResponseJson\" LIKE {tenantMarker}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.users WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.clinics WHERE \"TenantId\" = {tenantId}", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clinic_assistant.tenants WHERE \"Id\" = {tenantId}", ct);
        await transaction.CommitAsync(ct);
    }
    public async Task<PlatformDashboardResponse> GetDashboardAsync(PlatformDashboardQuery query, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var period = query.Period?.Trim().ToLowerInvariant() ?? "30d";
        var days = period switch { "7d" => 7, "30d" => 30, "90d" => 90, _ => 30 };
        var to = query.To ?? now;
        var from = query.From ?? to.AddDays(-days);
        if (to <= from || to - from > TimeSpan.FromDays(90)) throw new ArgumentException("Dashboard period must be between 1 and 90 days.");

        var tenants = await db.Tenants.IgnoreQueryFilters().Select(t => new { t.Id, t.Name, t.Status, t.CreatedAt }).ToListAsync(ct);
        var leads = await db.DemoLeads.IgnoreQueryFilters().Where(l => l.CreatedAt >= from && l.CreatedAt < to).Select(l => new { l.Status, l.CreatedAt, l.LastContactAt, l.CompanyOrClinicName }).ToListAsync(ct);
        var admins = await db.Users.IgnoreQueryFilters().Where(u => u.Role == UserRole.ClinicAdmin && u.Status == UserStatus.Active).Select(u => u.TenantId).ToListAsync(ct);
        var provisioning = tenants.Where(t => t.Status == TenantStatus.Provisioning).ToList();
        var clinics = new List<PlatformDashboardClinic>();
        foreach (var tenant in provisioning.Take(50))
        {
            var status = await GetOnboardingStatusAsync(tenant.Id, ct);
            var completed = new[] { status.ClinicConfigured, status.UnitConfigured, status.SpecialtiesConfigured, status.ProfessionalsConfigured, status.AvailabilityConfigured, status.ClinicAdminConfigured, status.WhatsAppConfigured }.Count(x => x);
            var adminName = await db.Users.IgnoreQueryFilters().Where(u => u.TenantId == tenant.Id && u.Role == UserRole.ClinicAdmin).Select(u => u.Name).FirstOrDefaultAsync(ct);
            clinics.Add(new PlatformDashboardClinic(tenant.Id, tenant.Name, tenant.Status.ToString(), completed, 7, adminName, tenant.CreatedAt));
        }

        var statusCounts = tenants.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.Count());
        var commercial = new PlatformDashboardCommercial(
            leads.Count(l => l.Status == DemoLeadStatus.New), leads.Count(l => l.Status == DemoLeadStatus.Contacted),
            leads.Count(l => l.Status == DemoLeadStatus.Qualified), leads.Count(l => l.Status == DemoLeadStatus.DemoScheduled),
            leads.Count(l => l.Status == DemoLeadStatus.Won), leads.Count(l => l.Status is DemoLeadStatus.New or DemoLeadStatus.Contacted), leads.Count);
        var growth = Enumerable.Range(0, (to.Date - from.Date).Days + 1).Select(offset =>
        {
            var date = DateOnly.FromDateTime(from.UtcDateTime.Date.AddDays(offset));
            return new PlatformDashboardGrowthPoint(date, tenants.Count(t => DateOnly.FromDateTime(t.CreatedAt.UtcDateTime) == date), leads.Count(l => DateOnly.FromDateTime(l.CreatedAt.UtcDateTime) == date));
        }).ToList();

        var healthReport = await healthChecks.CheckHealthAsync(c => c.Tags.Contains("ready"), ct);
        var health = new List<PlatformDashboardHealth> { new("API", "Operacional", "Endpoint do dashboard respondeu.") };
        foreach (var entry in healthReport.Entries)
        {
            var name = entry.Key switch { "postgresql" => "Banco de dados", "rabbitmq" => "RabbitMQ", "redis" => "Cache/Redis", _ => entry.Key };
            health.Add(new PlatformDashboardHealth(name, entry.Value.Status switch { HealthStatus.Healthy => "Operacional", HealthStatus.Degraded => "Atenção", _ => "Indisponível" }, entry.Value.Description));
        }
        var attention = new List<PlatformDashboardAttention>();
        if (provisioning.Count > 0) attention.Add(new("info", "Clínicas em configuração", $"{provisioning.Count} clínica(s) ainda precisam concluir o setup.", "/platform"));
        if (commercial.AwaitingContact > 0) attention.Add(new("info", "Leads aguardando contato", $"{commercial.AwaitingContact} lead(s) aguardam o primeiro contato.", "/platform/leads"));
        attention.AddRange(health.Where(h => h.Status != "Operacional").Select(h => new PlatformDashboardAttention("warning", $"{h.Service} requer atenção", h.Message ?? "Verifique o serviço.", "/platform")));
        if (attention.Count == 0) attention.Add(new("info", "Nenhum ponto crítico identificado", "A plataforma não possui alertas registrados no período.", null));

        var audit = await db.AuditRecords.IgnoreQueryFilters().Where(a => a.CreatedAt >= from && a.CreatedAt < to).OrderByDescending(a => a.CreatedAt).Take(10).Select(a => new { a.CreatedAt, a.Action, a.Details }).ToListAsync(ct);
        var activity = audit.Select(a => new PlatformDashboardActivity(a.CreatedAt, a.Action switch { "tenant.onboard" => "Nova clínica provisionada", "demo_lead.created" => "Novo lead comercial", "tenant.activate" => "Clínica ativada", "tenant.suspend" => "Clínica suspensa", _ => "Atividade da plataforma" }, a.Details, a.Action.StartsWith("demo_lead", StringComparison.Ordinal) ? "/platform/leads" : "/platform")).ToList();
        var summary = new PlatformDashboardSummary(tenants.Count, statusCounts.GetValueOrDefault(TenantStatus.Active), provisioning.Count, statusCounts.GetValueOrDefault(TenantStatus.Suspended), statusCounts.GetValueOrDefault(TenantStatus.Blocked), tenants.Count(t => t.CreatedAt >= from && t.CreatedAt < to), admins.Distinct().Count());
        return new PlatformDashboardResponse(from, to, summary, commercial, growth, clinics, health, attention, activity);
    }
    private async Task EnsureTenantAsync(Guid tenantId, CancellationToken ct)
    {
        if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId, ct)) throw new KeyNotFoundException("Tenant not found.");
    }
    private Task PublishAuditAsync(Guid tenantId, string action, Guid resourceId, CancellationToken ct) => events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = "Tenant", ResourceId = resourceId, Result = "Succeeded" }, ct);
}
