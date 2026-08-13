namespace ClinicAssistant.Application.Identity;

public sealed class TenantAccessGuard(ITenantContext tenantContext)
{
    public Guid RequireTenantId() => tenantContext.TenantId ?? throw new UnauthorizedAccessException("A tenant context is required for this operation.");
    public void EnsureAccess(Guid tenantId)
    {
        if (!tenantContext.IsPlatformAdmin && RequireTenantId() != tenantId) throw new UnauthorizedAccessException("Cross-tenant access is not allowed.");
    }
}
