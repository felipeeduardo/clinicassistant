using ClinicAssistant.Contracts.Scheduling;

namespace ClinicAssistant.Application.Scheduling;

public interface ISchedulingService
{
    Task<IReadOnlyList<PatientResponse>> GetPatientsAsync(CancellationToken ct);
    Task<PatientPage> SearchPatientsAsync(PatientSearchRequest r, CancellationToken ct);
    Task<PatientDetailResponse> GetPatientDetailAsync(Guid id, CancellationToken ct);
    Task<PatientResponse> CreatePatientAsync(PatientRequest r, CancellationToken ct);
    Task<PatientResponse> UpdatePatientAsync(Guid id, PatientRequest r, CancellationToken ct);
    Task<IReadOnlyList<AvailableSlot>> GetAvailabilityAsync(Guid professionalId, DateOnly appointmentDate, CancellationToken ct);
    Task AddAvailabilityRuleAsync(Guid professionalId, AvailabilityRuleRequest r, CancellationToken ct);
    Task<IReadOnlyList<AvailabilityRuleResponse>> GetAvailabilityRulesAsync(Guid professionalId, CancellationToken ct);
    Task<IReadOnlyList<AvailabilityRuleResponse>> ReplaceAvailabilityRulesAsync(Guid professionalId, IReadOnlyList<AvailabilityRuleRequest> r, CancellationToken ct);
    Task AddScheduleBlockAsync(Guid professionalId, ScheduleBlockRequest r, CancellationToken ct);
    Task<IReadOnlyList<ScheduleBlockResponse>> GetScheduleBlocksAsync(Guid professionalId, CancellationToken ct);
    Task DeleteScheduleBlockAsync(Guid professionalId, Guid blockId, CancellationToken ct);
    Task<IReadOnlyList<VacationResponse>> GetVacationsAsync(Guid professionalId, CancellationToken ct);
    Task AddVacationAsync(Guid professionalId, VacationRequest r, CancellationToken ct);
    Task DeleteVacationAsync(Guid professionalId, Guid vacationId, CancellationToken ct);
    Task<ProfessionalScheduleResponse> GetProfessionalScheduleAsync(Guid professionalId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct);
    Task<IReadOnlyList<AppointmentListItem>> GetAppointmentsAsync(DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct);
    Task<AppointmentPage> SearchAppointmentsAsync(AppointmentSearchRequest r, CancellationToken ct);
    Task<AppointmentDetailResponse> GetAppointmentDetailAsync(Guid id, CancellationToken ct);
    Task<AppointmentResponse> CreateAppointmentAsync(AppointmentRequest r, string idempotencyKey, CancellationToken ct);
    Task<AppointmentResponse> ConfirmAsync(Guid id, AppointmentOperationRequest r, string idempotencyKey, CancellationToken ct);
    Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, string idempotencyKey, CancellationToken ct);
    Task<RescheduleAppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest r, string idempotencyKey, CancellationToken ct);
}
