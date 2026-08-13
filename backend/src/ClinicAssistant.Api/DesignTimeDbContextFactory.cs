using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicAssistant.Api;

/// <summary>
/// Creates the DbContext for EF Core commands without starting the API host.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClinicAssistantDbContext>
{
    public ClinicAssistantDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__Default must be set when running EF Core commands.");
        }

        var options = new DbContextOptionsBuilder<ClinicAssistantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ClinicAssistantDbContext(options);
    }
}
