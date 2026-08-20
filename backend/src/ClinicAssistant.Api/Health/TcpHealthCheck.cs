using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicAssistant.Api.Health;

public sealed class TcpHealthCheck(IConfiguration configuration, string sectionName, int defaultPort) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = sectionName.Equals("Redis", StringComparison.OrdinalIgnoreCase)
            ? configuration["Redis:ConnectionString"]
                ?? configuration["REDIS_URL"]
                ?? configuration["REDIS_PRIVATE_URL"]
            : null;
        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var connectionUri)
            && connectionUri.Scheme is "redis" or "rediss")
        {
            var hostFromUri = connectionUri.Host;
            var portFromUri = connectionUri.IsDefaultPort ? defaultPort : connectionUri.Port;
            return await CheckEndpointAsync(hostFromUri, portFromUri, cancellationToken);
        }

        var configuredHost = configuration[$"{sectionName}:Host"];
        var host = string.IsNullOrWhiteSpace(configuredHost) ? sectionName.ToLowerInvariant() : configuredHost;
        var port = configuration.GetValue<int?>($"{sectionName}:Port") ?? defaultPort;

        return await CheckEndpointAsync(host, port, cancellationToken);
    }

    private static async Task<HealthCheckResult> CheckEndpointAsync(string host, int port, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(host, port, cancellationToken);
            return HealthCheckResult.Healthy("Redis is reachable.");
        }
        catch (SocketException exception)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable.", exception);
        }
    }
}
