using System.Security.Claims;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Domain.Identity;
using Microsoft.AspNetCore.Http;

namespace ClinicAssistant.Infrastructure.Identity;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
    public Guid? TenantId => Guid.TryParse(Principal?.FindFirstValue("tenant_id"), out var tenantId) ? tenantId : null;
    public Guid? UserId => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    public bool IsPlatformAdmin => string.Equals(Principal?.FindFirstValue(ClaimTypes.Role), UserRole.PlatformAdmin.ToString(), StringComparison.Ordinal);
}
