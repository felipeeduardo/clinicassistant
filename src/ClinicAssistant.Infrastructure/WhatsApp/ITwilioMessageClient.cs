namespace ClinicAssistant.Infrastructure.WhatsApp;

public interface ITwilioMessageClient
{
    Task<TwilioMessageResult> SendTextAsync(TwilioSendTextRequest request, CancellationToken cancellationToken);
    Task<TwilioMessageResult> SendTemplateAsync(TwilioSendTemplateRequest request, CancellationToken cancellationToken);
    Task<TwilioMessageResult> SendMediaAsync(TwilioSendMediaRequest request, CancellationToken cancellationToken);
}

public sealed record TwilioSendTextRequest(string To, string From, string Body, string? MessagingServiceSid);
public sealed record TwilioSendTemplateRequest(string To, string From, string ContentSid, IReadOnlyDictionary<string, string> Variables, string? MessagingServiceSid);
public sealed record TwilioSendMediaRequest(string To, string From, string MediaUrl, string? Caption, string? MessagingServiceSid);
public sealed record TwilioMessageResult(bool Success, string? MessageSid, string? Status, TwilioFailure? Failure);
public sealed record TwilioFailure(string? Code, string SafeMessage, int? HttpStatusCode);
