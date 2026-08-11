namespace ClinicAssistant.Application.Operations;

public sealed record DashboardUpcomingAppointment(Guid Id, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status, string PatientName, string ProfessionalName, string UnitName);
public sealed record DashboardWhatsAppMetrics(int MessagesReceived, int MessagesSent, int FailedMessages, int OpenConversations);

public sealed record DashboardSummary(int AppointmentsToday, int PendingAppointmentsToday, int WaitingHumanConversations, int ActiveConversations, int FailedOutboxMessages, string? WhatsAppStatus, IReadOnlyDictionary<string, int>? AppointmentStatus = null, IReadOnlyList<DashboardUpcomingAppointment>? UpcomingAppointments = null, DashboardWhatsAppMetrics? WhatsAppMetrics = null);

public interface IDashboardService
{
    Task<DashboardSummary> GetAsync(DateTimeOffset? from, DateTimeOffset? endsAt, CancellationToken ct);
}
