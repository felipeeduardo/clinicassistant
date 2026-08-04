using System.Diagnostics.Metrics;

namespace ClinicAssistant.Application.Operations;

public static class OperationalTelemetry
{
    private static readonly Meter Meter = new("ClinicAssistant.Operations");

    public static readonly Counter<long> AuthorizationDenied = Meter.CreateCounter<long>("authorization_denied_total");
    public static readonly Counter<long> PlatformOnboarding = Meter.CreateCounter<long>("platform_onboarding_total");
    public static readonly Counter<long> PlatformOnboardingFailures = Meter.CreateCounter<long>("platform_onboarding_failures_total");
    public static readonly Counter<long> AppointmentsRescheduled = Meter.CreateCounter<long>("appointments_rescheduled_total");
    public static readonly Counter<long> AppointmentConflicts = Meter.CreateCounter<long>("appointment_conflicts_total");
    public static readonly Counter<long> ManualMessages = Meter.CreateCounter<long>("manual_messages_total");
    public static readonly Counter<long> AuditEntries = Meter.CreateCounter<long>("audit_entries_total");
    public static readonly Counter<long> DashboardRequests = Meter.CreateCounter<long>("dashboard_requests_total");
    public static readonly Histogram<double> DashboardRequestDuration = Meter.CreateHistogram<double>("dashboard_request_duration", unit: "ms");
    public static readonly UpDownCounter<long> SignalRConnectionsActive = Meter.CreateUpDownCounter<long>("signalr_connections_active");
    public static readonly Counter<long> SignalREventsPublished = Meter.CreateCounter<long>("signalr_events_published_total");
    public static readonly Counter<long> SignalRPublishFailures = Meter.CreateCounter<long>("signalr_publish_failures_total");
    public static readonly Counter<long> RefreshTokenRotations = Meter.CreateCounter<long>("refresh_token_rotations_total");
    public static readonly Counter<long> RefreshTokenReuseDetected = Meter.CreateCounter<long>("refresh_token_reuse_detected_total");
    public static readonly Counter<long> WhatsAppTemplateSyncFailures = Meter.CreateCounter<long>("whatsapp_template_sync_failures_total");
    public static readonly Counter<long> TwilioConfigurationValidations = Meter.CreateCounter<long>("twilio_configuration_validations_total");
    public static readonly Counter<long> TwilioConfigurationFailures = Meter.CreateCounter<long>("twilio_configuration_failures_total");
}
