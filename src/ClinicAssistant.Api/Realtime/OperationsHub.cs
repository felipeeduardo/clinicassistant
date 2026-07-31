using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClinicAssistant.Api.Realtime;

[Authorize(Policy = "ClinicStaff")]
public sealed class OperationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantId, out _))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantId));
        await base.OnConnectedAsync();
    }

    public static string TenantGroup(Guid tenantId) => TenantGroup(tenantId.ToString("D"));
    private static string TenantGroup(string tenantId) => $"tenant:{tenantId}";
}
