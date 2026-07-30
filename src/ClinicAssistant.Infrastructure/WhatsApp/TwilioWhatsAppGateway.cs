using System.Net;
using ClinicAssistant.Application.WhatsApp;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioWhatsAppGateway(ITwilioMessageClient client, IWhatsAppPhoneNumberFormatter phoneNumberFormatter, IOptions<TwilioOptions> options) : IWhatsAppGateway
{
    private readonly TwilioOptions _options = options.Value;

    public async Task<SendWhatsAppMessageResult> SendTextAsync(SendWhatsAppTextRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) return Invalid("A text message cannot be empty.");
        var result = await client.SendTextAsync(new(phoneNumberFormatter.FormatForProvider(request.RecipientPhone), ResolveFrom(), request.Text, _options.MessagingServiceSid), cancellationToken);
        return Map(result);
    }

    public async Task<SendWhatsAppMessageResult> SendTemplateAsync(SendWhatsAppTemplateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ContentSid)) return Invalid("A template ContentSid is required.");
        var result = await client.SendTemplateAsync(new(phoneNumberFormatter.FormatForProvider(request.RecipientPhone), ResolveFrom(), request.ContentSid, request.Variables, _options.MessagingServiceSid), cancellationToken);
        return Map(result);
    }

    public async Task<SendWhatsAppMessageResult> SendMediaAsync(SendWhatsAppMediaRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.MediaUrl, UriKind.Absolute, out _)) return Invalid("A valid media URL is required.");
        var result = await client.SendMediaAsync(new(phoneNumberFormatter.FormatForProvider(request.RecipientPhone), ResolveFrom(), request.MediaUrl, request.Caption, _options.MessagingServiceSid), cancellationToken);
        return Map(result);
    }

    private string ResolveFrom() => phoneNumberFormatter.FormatForProvider(_options.WhatsAppFrom);

    private static SendWhatsAppMessageResult Invalid(string message) => new(false, null, "failed", new(WhatsAppFailureType.Permanent, "invalid_request", message, false));

    private static SendWhatsAppMessageResult Map(TwilioMessageResult result)
    {
        if (result.Success) return new(true, result.MessageSid, result.Status, null);
        var failure = result.Failure;
        var type = failure?.HttpStatusCode switch
        {
            (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden => WhatsAppFailureType.Authentication,
            (int)HttpStatusCode.TooManyRequests => WhatsAppFailureType.RateLimit,
            >= 500 and <= 599 => WhatsAppFailureType.Transient,
            _ => WhatsAppFailureType.Permanent
        };
        var canRetry = type is WhatsAppFailureType.Transient or WhatsAppFailureType.RateLimit;
        return new(false, null, result.Status, new(type, failure?.Code, failure?.SafeMessage ?? "Twilio could not send the message.", canRetry));
    }
}
