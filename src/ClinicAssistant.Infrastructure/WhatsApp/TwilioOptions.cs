namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";
    public string AccountSid { get; init; } = string.Empty;
    public string AuthToken { get; init; } = string.Empty;
    public string WhatsAppFrom { get; init; } = string.Empty;
    public string? MessagingServiceSid { get; init; }
    public string BaseUrl { get; init; } = "https://api.twilio.com";
    public string? IncomingWebhookBaseUrl { get; init; }
    public string? StatusCallbackBaseUrl { get; init; }
    public string? StatusCallbackUrl { get; init; }
    public bool SignatureValidationEnabled { get; init; } = true;
    public int RequestTimeoutSeconds { get; init; } = 15;
    public string[] TrustedProxyAddresses { get; init; } = [];
}
