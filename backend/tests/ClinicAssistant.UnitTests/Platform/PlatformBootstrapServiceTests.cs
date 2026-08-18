using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Infrastructure.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.Platform;

public sealed class PlatformBootstrapServiceTests
{
    [Fact]
    public async Task DisabledBootstrapDoesNothing()
    {
        await using var db = CreateDb();
        var service = CreateService(db, new PlatformBootstrapOptions { Enabled = false });

        var result = await service.RunAsync(CancellationToken.None);

        Assert.False(result.Enabled);
        Assert.Empty(await db.Users.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task EnabledBootstrapCreatesEachConfiguredAdminAndIsIdempotent()
    {
        await using var db = CreateDb();
        var options = new PlatformBootstrapOptions
        {
            Enabled = true,
            Admins = [
                new() { Email = "admin-a@example.test", Password = "Valid-Password-123!" },
                new() { Email = "admin-b@example.test", Password = "Valid-Password-456!" }]
        };
        var service = CreateService(db, options);

        var first = await service.RunAsync(CancellationToken.None);
        var second = await service.RunAsync(CancellationToken.None);

        Assert.Equal(2, first.Created);
        Assert.Equal(0, first.AlreadyExisting);
        Assert.Equal(0, second.Created);
        Assert.Equal(2, second.AlreadyExisting);
        Assert.Equal(2, await db.Users.IgnoreQueryFilters().CountAsync(user => user.Role == UserRole.PlatformAdmin));
        Assert.Equal(2, await db.AuditRecords.CountAsync(record => record.Action == "PlatformAdminCreated"));
    }

    [Fact]
    public async Task ExistingNonPlatformUserIsNotPromoted()
    {
        await using var db = CreateDb();
        var tenant = new ClinicAssistant.Domain.Identity.Tenant("Clinic", "clinic");
        db.Tenants.Add(tenant);
        db.Users.Add(new ClinicAssistant.Domain.Identity.User(tenant.Id, "Existing", "admin@example.test", "hash", UserRole.ClinicAdmin));
        await db.SaveChangesAsync();
        var service = CreateService(db, new PlatformBootstrapOptions
        {
            Enabled = true,
            Admins = [new() { Email = "admin@example.test", Password = "Valid-Password-123!" }]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAsync(CancellationToken.None));
        Assert.Equal(UserRole.ClinicAdmin, (await db.Users.IgnoreQueryFilters().SingleAsync()).Role);
    }

    [Fact]
    public async Task InvalidPasswordFailsWithoutPersistingUsers()
    {
        await using var db = CreateDb();
        var service = CreateService(db, new PlatformBootstrapOptions
        {
            Enabled = true,
            Admins = [new() { Email = "admin@example.test", Password = "weak" }]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAsync(CancellationToken.None));
        Assert.Empty(await db.Users.IgnoreQueryFilters().ToListAsync());
    }

    private static PlatformBootstrapService CreateService(ClinicAssistantDbContext db, PlatformBootstrapOptions options) =>
        new(db, Options.Create(options), NullLogger<PlatformBootstrapService>.Instance);

    private static ClinicAssistantDbContext CreateDb() => new(new DbContextOptionsBuilder<ClinicAssistantDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options, new TestTenantContext());

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public Guid? UserId => null;
        public bool IsPlatformAdmin => true;
    }
}
