namespace ClinicAssistant.Application.Realtime;

public sealed record RealtimeEvent<T>(Guid EventId, string EventType, Guid TenantId, DateTimeOffset OccurredAt, string CorrelationId, long? ResourceVersion, T Data);

public interface IOperationalEventPublisher
{
    Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken);
}

public sealed class NoOpOperationalEventPublisher : IOperationalEventPublisher
{
    public Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken) => Task.CompletedTask;
}
