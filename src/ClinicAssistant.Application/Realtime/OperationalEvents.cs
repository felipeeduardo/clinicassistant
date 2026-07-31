namespace ClinicAssistant.Application.Realtime;

public interface IOperationalEventPublisher
{
    Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken);
}

public sealed class NoOpOperationalEventPublisher : IOperationalEventPublisher
{
    public Task PublishAsync(Guid tenantId, string eventName, object payload, CancellationToken cancellationToken) => Task.CompletedTask;
}
