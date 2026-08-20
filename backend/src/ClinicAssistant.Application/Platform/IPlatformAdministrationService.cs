using ClinicAssistant.Contracts.Platform;
using ClinicAssistant.Contracts.Clinics;
using ClinicAssistant.Contracts.Scheduling;

namespace ClinicAssistant.Application.Platform;

public interface IPlatformAdministrationService
{
    Task<IReadOnlyList<PlatformTenantResponse>> GetTenantsAsync(CancellationToken ct);
    Task<IReadOnlyList<PlatformUserResponse>> GetUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<PlatformClinicResponse>> GetClinicsAsync(CancellationToken ct);
    Task SetTenantStatusAsync(Guid tenantId, string action, CancellationToken ct);
    Task<OnboardTenantResponse> OnboardAsync(OnboardTenantRequest r, string key, CancellationToken ct);
    Task<PlatformOnboardingStatusResponse> GetOnboardingStatusAsync(Guid tenantId, CancellationToken ct);
    Task<PlatformWhatsAppStatusResponse> GetWhatsAppStatusAsync(Guid tenantId, CancellationToken ct);
    Task<PlatformUserResponse> CreateClinicAdminAsync(Guid tenantId, CreateClinicAdminRequest request, string idempotencyKey, CancellationToken ct);
    Task DeleteTenantAsync(Guid tenantId, DeleteTenantRequest request, CancellationToken ct);
    Task<PlatformDashboardResponse> GetDashboardAsync(PlatformDashboardQuery query, CancellationToken ct);
}
