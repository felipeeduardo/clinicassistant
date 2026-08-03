using ClinicAssistant.Domain.Identity;

namespace ClinicAssistant.Application.Authorization;

public static class ClinicPolicies
{
    public const string ClinicsView = "Clinics.View";
    public const string ClinicsManage = "Clinics.Manage";
    public const string UnitsView = "Units.View";
    public const string UnitsManage = "Units.Manage";
    public const string PatientsView = "Patients.View";
    public const string PatientsManage = "Patients.Manage";
    public const string ProfessionalsView = "Professionals.View";
    public const string ProfessionalsManage = "Professionals.Manage";
    public const string SpecialtiesView = "Specialties.View";
    public const string SpecialtiesManage = "Specialties.Manage";

    public static readonly string[] ViewRoles = [UserRole.ClinicAdmin.ToString(), UserRole.Receptionist.ToString(), UserRole.Professional.ToString(), UserRole.Viewer.ToString()];
    public static readonly string[] ManageRoles = [UserRole.ClinicAdmin.ToString()];
}
