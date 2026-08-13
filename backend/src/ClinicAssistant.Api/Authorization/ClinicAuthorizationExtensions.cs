using ClinicAssistant.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ClinicAssistant.Api.Authorization;

public static class ClinicAuthorizationExtensions
{
    public static void AddClinicPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(ClinicPolicies.ClinicsView, policy => policy.RequireRole(ClinicPolicies.ViewRoles));
        options.AddPolicy(ClinicPolicies.ClinicsManage, policy => policy.RequireRole(ClinicPolicies.ManageRoles));
        options.AddPolicy(ClinicPolicies.UnitsView, policy => policy.RequireRole(ClinicPolicies.ViewRoles));
        options.AddPolicy(ClinicPolicies.UnitsManage, policy => policy.RequireRole(ClinicPolicies.ManageRoles));
        options.AddPolicy(ClinicPolicies.PatientsView, policy => policy.RequireRole(ClinicPolicies.ViewRoles));
        options.AddPolicy(ClinicPolicies.PatientsManage, policy => policy.RequireRole(ClinicPolicies.ManageRoles));
        options.AddPolicy(ClinicPolicies.ProfessionalsView, policy => policy.RequireRole(ClinicPolicies.ViewRoles));
        options.AddPolicy(ClinicPolicies.ProfessionalsManage, policy => policy.RequireRole(ClinicPolicies.ManageRoles));
        options.AddPolicy(ClinicPolicies.SpecialtiesView, policy => policy.RequireRole(ClinicPolicies.ViewRoles));
        options.AddPolicy(ClinicPolicies.SpecialtiesManage, policy => policy.RequireRole(ClinicPolicies.ManageRoles));
    }
}
