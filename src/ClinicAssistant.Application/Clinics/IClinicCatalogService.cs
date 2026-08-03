using ClinicAssistant.Contracts.Clinics;

namespace ClinicAssistant.Application.Clinics;

public interface IClinicCatalogService
{
    Task<ClinicResponse?> GetClinicAsync(CancellationToken ct);
    Task<ClinicResponse> UpdateClinicAsync(ClinicRequest request, CancellationToken ct);
    Task<IReadOnlyList<UnitResponse>> GetUnitsAsync(CancellationToken ct);
    Task<UnitResponse?> GetUnitAsync(Guid id, CancellationToken ct);
    Task<UnitDetailResponse?> GetUnitDetailAsync(Guid id, CancellationToken ct);
    Task<UnitResponse> CreateUnitAsync(UnitRequest request, CancellationToken ct);
    Task<UnitResponse> UpdateUnitAsync(Guid id, UnitRequest request, CancellationToken ct);
    Task DeleteUnitAsync(Guid id, CancellationToken ct);
    Task SetUnitStatusAsync(Guid id, string status, CancellationToken ct);
    Task<IReadOnlyList<UnitBusinessHourResponse>> ReplaceUnitBusinessHoursAsync(Guid id, IReadOnlyList<UnitBusinessHourRequest> request, CancellationToken ct);
    Task<IReadOnlyList<SpecialtyResponse>> GetSpecialtiesAsync(CancellationToken ct);
    Task<SpecialtyResponse> CreateSpecialtyAsync(SpecialtyRequest request, CancellationToken ct);
    Task<SpecialtyResponse> UpdateSpecialtyAsync(Guid id, SpecialtyRequest request, CancellationToken ct);
    Task DeleteSpecialtyAsync(Guid id, CancellationToken ct);
    Task<SpecialtyDependenciesResponse> GetSpecialtyDependenciesAsync(Guid id, CancellationToken ct);
    Task SetSpecialtyStatusAsync(Guid id, string status, CancellationToken ct);
    Task<IReadOnlyList<ProfessionalResponse>> GetProfessionalsAsync(CancellationToken ct);
    Task<ProfessionalResponse?> GetProfessionalAsync(Guid id, CancellationToken ct);
    Task<ProfessionalResponse> CreateProfessionalAsync(ProfessionalRequest request, CancellationToken ct);
    Task<ProfessionalResponse> UpdateProfessionalAsync(Guid id, ProfessionalRequest request, CancellationToken ct);
    Task DeleteProfessionalAsync(Guid id, CancellationToken ct);
}
