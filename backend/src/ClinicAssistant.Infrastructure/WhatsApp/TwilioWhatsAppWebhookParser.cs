using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioWhatsAppWebhookParser(IWhatsAppPhoneNumberFormatter phoneNumberFormatter) : ITwilioWhatsAppWebhookParser
{
    public WhatsAppIncomingMessageReceived Parse(TwilioIncomingWebhook webhook, Guid tenantId, Guid integrationId, Guid inboxMessageId, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(webhook.MessageSid)) throw new InvalidOperationException("Twilio MessageSid is required.");
        if (string.IsNullOrWhiteSpace(webhook.From) || string.IsNullOrWhiteSpace(webhook.To)) throw new InvalidOperationException("Twilio sender and recipient are required.");

        var senderPhone = RemoveProviderPrefix(phoneNumberFormatter.FormatForProvider(webhook.From));
        var recipientPhone = RemoveProviderPrefix(phoneNumberFormatter.FormatForProvider(webhook.To));
        var type = webhook.NumMedia > 0 ? WhatsAppIncomingMessageType.Media
            : !string.IsNullOrWhiteSpace(webhook.Latitude) || !string.IsNullOrWhiteSpace(webhook.Longitude) ? WhatsAppIncomingMessageType.Location
            : !string.IsNullOrWhiteSpace(webhook.ButtonPayload) ? WhatsAppIncomingMessageType.Interactive
            : !string.IsNullOrWhiteSpace(webhook.Body) ? WhatsAppIncomingMessageType.Text
            : WhatsAppIncomingMessageType.Unknown;

        var actionId = string.IsNullOrWhiteSpace(webhook.ButtonPayload) ? null : webhook.ButtonPayload;
        var text = actionId ?? webhook.Body ?? webhook.ButtonText;
        return new(tenantId, integrationId, inboxMessageId, webhook.MessageSid, senderPhone, recipientPhone, type, text,
            webhook.Media, webhook.ProfileName, actionId, DateTimeOffset.UtcNow, correlationId);
    }

    private static string RemoveProviderPrefix(string phoneNumber) => phoneNumber["whatsapp:".Length..];
}
