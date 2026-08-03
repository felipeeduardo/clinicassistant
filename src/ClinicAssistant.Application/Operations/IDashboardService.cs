namespace ClinicAssistant.Application.Operations;

public sealed record DashboardSummary(int AppointmentsToday, int PendingAppointmentsToday, int WaitingHumanConversations, int ActiveConversations, int FailedOutboxMessages, string? WhatsAppStatus);

public interface IDashboardService
{
    Task<DashboardSummary> GetAsync(CancellationToken ct);
}
