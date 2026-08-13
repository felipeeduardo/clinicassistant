using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Operations;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ClinicAssistant.Api.Realtime;

public sealed class SignalROperationalEventPublisher(IHubContext<OperationsHub> hub) : IOperationalEventPublisher
{
    public async Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var version = payload.GetType().GetProperty("Version")?.GetValue(payload); long? resourceVersion = version is int value ? value : null;
        var envelope = new RealtimeEvent<object>(Guid.NewGuid(), eventName, tenantId, DateTimeOffset.UtcNow, correlationId, resourceVersion, payload);
        try
        {
            await hub.Clients.Group(OperationsHub.TenantGroup(tenantId)).SendAsync(eventName, envelope, cancellationToken);
            OperationalTelemetry.SignalREventsPublished.Add(1);
            if (eventName == "audit.created") OperationalTelemetry.AuditEntries.Add(1);
        }
        catch
        {
            OperationalTelemetry.SignalRPublishFailures.Add(1);
            throw;
        }
    }
}
