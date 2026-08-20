namespace ClinicAssistant.Application.WhatsApp;

public interface IWhatsAppIntegrationStatusService
{
    Task<WhatsAppIntegrationOperationalStatus?> GetCurrentAsync(CancellationToken cancellationToken);
    Task ValidateCurrentAsync(CancellationToken cancellationToken);
    Task EnableCurrentAsync(CancellationToken cancellationToken);
    Task DisableCurrentAsync(CancellationToken cancellationToken);
    Task QueueTestMessageAsync(string idempotencyKey, CancellationToken cancellationToken);
}

public sealed record WhatsAppIntegrationOperationalStatus(
    string Status,
    string? DisplayPhoneNumber,
    DateTimeOffset? LastWebhookAt,
    DateTimeOffset? LastSuccessfulSendAt,
    DateTimeOffset? LastFailureAt,
    string? FailureReason);


public interface IPhoneMasker
{
    string Mask(string? phoneNumber);
}
