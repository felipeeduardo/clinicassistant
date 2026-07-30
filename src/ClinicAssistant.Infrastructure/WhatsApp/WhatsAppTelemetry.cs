using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public static class WhatsAppTelemetry
{
    private static readonly Meter Meter = new("ClinicAssistant.WhatsApp");
    public static readonly ActivitySource ActivitySource = new("ClinicAssistant.WhatsApp");
    public static readonly Counter<long> WebhookRequests = Meter.CreateCounter<long>("twilio_webhook_requests_total");
    public static readonly Counter<long> InvalidSignature = Meter.CreateCounter<long>("twilio_webhook_invalid_signature_total");
    public static readonly Counter<long> Duplicate = Meter.CreateCounter<long>("twilio_webhook_duplicate_total");
    public static readonly Histogram<double> WebhookDuration = Meter.CreateHistogram<double>("twilio_webhook_duration", unit: "ms");
    public static readonly Counter<long> IncomingMessages = Meter.CreateCounter<long>("whatsapp_incoming_messages_total");
    public static readonly Counter<long> OutgoingMessages = Meter.CreateCounter<long>("whatsapp_outgoing_messages_total");
    public static readonly Counter<long> SendSuccess = Meter.CreateCounter<long>("whatsapp_send_success_total");
    public static readonly Counter<long> SendFailure = Meter.CreateCounter<long>("whatsapp_send_failure_total");
    public static readonly Counter<long> StatusUpdates = Meter.CreateCounter<long>("whatsapp_status_updates_total");

    public static void RecordWebhook(string webhookType, TimeSpan duration)
    {
        var tags = new TagList { { "webhook.type", webhookType } };
        WebhookRequests.Add(1, tags);
        WebhookDuration.Record(duration.TotalMilliseconds, tags);
    }
}
