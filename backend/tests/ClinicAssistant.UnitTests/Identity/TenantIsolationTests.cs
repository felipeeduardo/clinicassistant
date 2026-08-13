using ClinicAssistant.Application.Identity;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicAssistant.UnitTests.Identity;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task TenantQueryFilterOnlyReturnsItsOwnUsers()
    {
        var options = new DbContextOptionsBuilder<ClinicAssistantDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var tenantA = new Tenant("Clinic A", "clinic-a");
        var tenantB = new Tenant("Clinic B", "clinic-b");

        await using (var seedContext = new ClinicAssistantDbContext(options, new TestTenantContext(isPlatformAdmin: true)))
        {
            seedContext.AddRange(tenantA, tenantB);
            seedContext.Users.Add(new User(tenantA.Id, "Admin A", "admin-a@example.test", "hash", UserRole.ClinicAdmin));
            seedContext.Users.Add(new User(tenantB.Id, "Admin B", "admin-b@example.test", "hash", UserRole.ClinicAdmin));
            await seedContext.SaveChangesAsync();
        }

        await using var tenantAContext = new ClinicAssistantDbContext(options, new TestTenantContext(tenantA.Id));
        var users = await tenantAContext.Users.ToListAsync();

        var user = Assert.Single(users);
        Assert.Equal(tenantA.Id, user.TenantId);
    }

    [Fact]
    public void TenantAccessGuardRejectsAnotherTenant()
    {
        var currentTenant = Guid.NewGuid();
        var guard = new TenantAccessGuard(new TestTenantContext(currentTenant));

        Assert.Throws<UnauthorizedAccessException>(() => guard.EnsureAccess(Guid.NewGuid()));
    }

    private sealed class TestTenantContext(Guid? tenantId = null, bool isPlatformAdmin = false) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
        public Guid? UserId => null;
        public bool IsPlatformAdmin { get; } = isPlatformAdmin;
    }
}
