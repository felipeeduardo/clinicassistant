using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicAssistant.Api.Health;

public sealed class TcpHealthCheck(IConfiguration configuration, string sectionName, int defaultPort) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var configuredHost = configuration[$"{sectionName}:Host"];
        var host = string.IsNullOrWhiteSpace(configuredHost) ? sectionName.ToLowerInvariant() : configuredHost;
        var port = configuration.GetValue<int?>($"{sectionName}:Port") ?? defaultPort;

        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(host, port, cancellationToken);
            return HealthCheckResult.Healthy($"{sectionName} is reachable.");
        }
        catch (SocketException exception)
        {
            return HealthCheckResult.Unhealthy($"{sectionName} is unavailable.", exception);
        }
    }
}
