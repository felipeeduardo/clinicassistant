using Microsoft.Extensions.Configuration;

namespace ClinicAssistant.Infrastructure.Persistence;

public static class DatabaseConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var target = configuration["Database:Target"]?.Trim().ToLowerInvariant() ?? "primary";
        var connectionName = target switch
        {
            "primary" => "Primary",
            "test" => "Test",
            _ => throw new InvalidOperationException("Database:Target must be 'primary' or 'test'.")
        };

        return configuration.GetConnectionString(connectionName)
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException($"ConnectionStrings:{connectionName} must be configured.");
    }
}
