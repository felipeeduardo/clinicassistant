using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicAssistant.Api.Health;

public sealed class PostgreSqlHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection string is not configured.");
        }

        var options = new DbContextOptionsBuilder<ClinicAssistantDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using var dbContext = new ClinicAssistantDbContext(options);

        return await dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("PostgreSQL is reachable.")
            : HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
    }
}
