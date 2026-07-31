using ClinicAssistant.Application.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace ClinicAssistant.Api.Realtime;

public sealed class SignalROperationalEventPublisher(IHubContext<OperationsHub> hub) : IOperationalEventPublisher
{
    public Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken) =>
        hub.Clients.Group(OperationsHub.TenantGroup(tenantId)).SendAsync(eventName, new { EventId = Guid.NewGuid(), Payload = payload }, cancellationToken);
}
