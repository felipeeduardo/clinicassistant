using ClinicAssistant.Contracts.Scheduling;

namespace ClinicAssistant.Application.Scheduling;

public interface ISchedulingService
{
    Task<IReadOnlyList<PatientResponse>> GetPatientsAsync(CancellationToken ct);
    Task<PatientResponse> CreatePatientAsync(PatientRequest r, CancellationToken ct);
    Task<PatientResponse> UpdatePatientAsync(Guid id, PatientRequest r, CancellationToken ct);
    Task<IReadOnlyList<AvailableSlot>> GetAvailabilityAsync(Guid professionalId, DateOnly appointmentDate, CancellationToken ct);
    Task AddAvailabilityRuleAsync(Guid professionalId, AvailabilityRuleRequest r, CancellationToken ct);
    Task AddScheduleBlockAsync(Guid professionalId, ScheduleBlockRequest r, CancellationToken ct);
    Task<IReadOnlyList<AppointmentListItem>> GetAppointmentsAsync(DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct);
    Task<AppointmentResponse> CreateAppointmentAsync(AppointmentRequest r, CancellationToken ct);
    Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken ct);
    Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, CancellationToken ct);
}
