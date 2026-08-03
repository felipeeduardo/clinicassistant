using ClinicAssistant.Application.Authorization;
using ClinicAssistant.Api.Authorization;
using ClinicAssistant.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace ClinicAssistant.UnitTests.Authorization;

public sealed class ClinicPoliciesTests
{
    [Fact]
    public async Task ManagePoliciesAllowOnlyClinicAdmin()
    {
        var authorization = CreateAuthorizationService();
        Assert.True((await authorization.AuthorizeAsync(Principal(UserRole.ClinicAdmin), ClinicPolicies.UnitsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Principal(UserRole.Receptionist), ClinicPolicies.UnitsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Principal(UserRole.Professional), ClinicPolicies.PatientsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Principal(UserRole.Viewer), ClinicPolicies.SpecialtiesManage)).Succeeded);
    }

    [Fact]
    public async Task ViewPoliciesAllowViewerWithoutGrantingManageAccess()
    {
        var authorization = CreateAuthorizationService();
        Assert.True((await authorization.AuthorizeAsync(Principal(UserRole.Viewer), ClinicPolicies.ClinicsView)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(Principal(UserRole.Receptionist), ClinicPolicies.PatientsView)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(Principal(UserRole.Professional), ClinicPolicies.ProfessionalsView)).Succeeded);
    }

    private static IAuthorizationService CreateAuthorizationService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddClinicPolicies());
        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal Principal(UserRole role) => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role.ToString())], "test"));
}
