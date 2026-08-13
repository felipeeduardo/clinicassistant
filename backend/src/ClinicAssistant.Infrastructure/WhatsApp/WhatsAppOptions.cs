using ClinicAssistant.Domain.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";
    public WhatsAppProvider Provider { get; init; } = WhatsAppProvider.Fake;
    public int MaximumRetryAttempts { get; init; } = 3;
    public int RequestTimeoutSeconds { get; init; } = 15;
    public int RawPayloadRetentionDays { get; init; } = 30;
    public int MaxWebhookBodySizeBytes { get; init; } = 1_048_576;
    public string? TestRecipient { get; init; }
    public WhatsAppMediaOptions Media { get; init; } = new();
    public FakeWhatsAppOptions Fake { get; init; } = new();
}

public sealed class WhatsAppMediaOptions
{
    public long MaxFileSizeBytes { get; init; } = 10_485_760;
    public string AllowedTypes { get; init; } = "image/jpeg,image/png,application/pdf,audio/ogg";
}

public sealed class FakeWhatsAppOptions
{
    public int DelayMilliseconds { get; init; } = 100;
    public FakeWhatsAppFailureMode FailureMode { get; init; } = FakeWhatsAppFailureMode.None;
    public decimal FailureRate { get; init; }
}

public enum FakeWhatsAppFailureMode { None, Transient, Permanent, Timeout }
