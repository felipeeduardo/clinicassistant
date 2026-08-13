using System.Net.Sockets;
using System.Security.Authentication;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ClinicAssistant.Infrastructure.Messaging;

public sealed partial class RabbitMqHealthCheck(RabbitMqConnectionFactory connectionFactory, ILogger<RabbitMqHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await connectionFactory.Create("ClinicAssistant.Api").CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("RabbitMQ is reachable.");
        }
        catch (Exception exception) when (exception is RabbitMQ.Client.Exceptions.RabbitMQClientException or IOException or TimeoutException or SocketException or AuthenticationException)
        {
            LogHealthCheckFailed(logger, exception);
            return HealthCheckResult.Unhealthy("RabbitMQ is unavailable.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ health check failed for the configured host and virtual host.")]
    private static partial void LogHealthCheckFailed(ILogger logger, Exception exception);
}
