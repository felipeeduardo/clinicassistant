namespace ClinicAssistant.Application.WhatsApp;

public interface IWhatsAppIntegrationStatusService
{
    Task<WhatsAppIntegrationOperationalStatus?> GetCurrentAsync(CancellationToken cancellationToken);
    Task<TwilioConfigurationStatus> GetTwilioConfigurationAsync(CancellationToken cancellationToken);
    Task ValidateCurrentAsync(CancellationToken cancellationToken);
    Task EnableCurrentAsync(CancellationToken cancellationToken);
    Task DisableCurrentAsync(CancellationToken cancellationToken);
    Task QueueTestMessageAsync(string idempotencyKey, CancellationToken cancellationToken);
}

public sealed record WhatsAppIntegrationOperationalStatus(
    string Provider,
    string Status,
    string DisplayPhoneNumber,
    DateTimeOffset? LastWebhookAt,
    DateTimeOffset? LastSuccessfulSendAt,
    DateTimeOffset? LastFailureAt,
    string? FailureReason);

public sealed record TwilioConfigurationStatus(
    string Provider,
    string AccountSidMasked,
    bool AuthTokenConfigured,
    string WhatsAppFromMasked,
    string? IncomingWebhookBaseUrl,
    string? StatusCallbackBaseUrl,
    string Environment,
    bool SignatureValidationEnabled,
    bool Enabled,
    DateTimeOffset? LastValidatedAt);

public interface IPhoneMasker
{
    string Mask(string? phoneNumber);
}
