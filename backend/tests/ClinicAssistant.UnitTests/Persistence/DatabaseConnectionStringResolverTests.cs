using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClinicAssistant.UnitTests.Persistence;

public sealed class DatabaseConnectionStringResolverTests
{
    [Fact]
    public void ResolveWhenTargetIsTestUsesTestConnectionString()
    {
        var configuration = CreateConfiguration("test");

        var connectionString = DatabaseConnectionStringResolver.Resolve(configuration);

        Assert.Equal("Host=test-postgres;Database=clinicassistant_test", connectionString);
    }

    [Fact]
    public void ResolveWhenTargetIsPrimaryUsesPrimaryConnectionString()
    {
        var configuration = CreateConfiguration("primary");

        var connectionString = DatabaseConnectionStringResolver.Resolve(configuration);

        Assert.Equal("Host=primary-postgres;Database=clinicassistant", connectionString);
    }

    private static IConfiguration CreateConfiguration(string target) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Target"] = target,
            ["ConnectionStrings:Primary"] = "Host=primary-postgres;Database=clinicassistant",
            ["ConnectionStrings:Test"] = "Host=test-postgres;Database=clinicassistant_test"
        })
        .Build();
}
