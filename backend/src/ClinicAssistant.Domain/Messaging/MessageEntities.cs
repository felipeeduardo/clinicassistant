using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Messaging;

public enum MessageStatus { Pending, Received, Queued, Processing, Processed, Ignored, Failed, DeadLettered, Duplicate }

public sealed class InboxMessage : Entity, ITenantEntity
{
    private InboxMessage() { }
    public InboxMessage(Guid tenantId, Guid integrationId, string provider, string eventType, string externalMessageId, string payloadHash, string rawPayload, string? correlationId)
    {
        TenantId = tenantId;
        IntegrationId = integrationId;
        Provider = provider;
        EventType = eventType;
        ExternalMessageId = externalMessageId;
        PayloadHash = payloadHash;
        RawPayload = rawPayload;
        CorrelationId = correlationId;
        Status = MessageStatus.Received;
        ReceivedAt = DateTimeOffset.UtcNow;
    }
    public Guid TenantId { get; private set; } public Guid IntegrationId { get; private set; } public string Provider { get; private set; } = null!; public string EventType { get; private set; } = null!; public string ExternalMessageId { get; private set; } = null!; public string? ExternalEventId { get; private set; } public string PayloadHash { get; private set; } = null!; public string RawPayload { get; private set; } = null!; public MessageStatus Status { get; private set; } public DateTimeOffset ReceivedAt { get; private set; } public DateTimeOffset? QueuedAt { get; private set; } public DateTimeOffset? ProcessingStartedAt { get; private set; } public DateTimeOffset? ProcessedAt { get; private set; } public int RetryCount { get; private set; } public string? LastErrorCode { get; private set; } public string? LastErrorMessage { get; private set; } public string? CorrelationId { get; private set; }
    public void MarkProcessing() { Status = MessageStatus.Processing; ProcessingStartedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkProcessed() { Status = MessageStatus.Processed; ProcessedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
}
public sealed class OutboxMessage : Entity, ITenantEntity
{
    private OutboxMessage() { }
    public OutboxMessage(Guid tenantId, string type, string payload, Guid? whatsAppChannelId = null) { TenantId = tenantId; Type = type; Payload = payload; WhatsAppChannelId = whatsAppChannelId; }
    public Guid TenantId { get; private set; } public Guid? WhatsAppChannelId { get; private set; } public string Type { get; private set; } = null!; public string Payload { get; private set; } = null!; public MessageStatus Status { get; private set; } = MessageStatus.Pending; public DateTimeOffset? ProcessedAt { get; private set; } public int RetryCount { get; private set; } public string? LastError { get; private set; } public DateTimeOffset? FirstFailureAt { get; private set; } public DateTimeOffset? NextAttemptAt { get; private set; }
    public void MarkProcessed() { Status = MessageStatus.Processed; ProcessedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkFailure(string error, int maximumRetryAttempts)
    {
        RetryCount++;
        LastError = error;
        FirstFailureAt ??= DateTimeOffset.UtcNow;
        Status = RetryCount >= maximumRetryAttempts ? MessageStatus.DeadLettered : MessageStatus.Pending;
        NextAttemptAt = Status == MessageStatus.DeadLettered ? null : DateTimeOffset.UtcNow.Add(GetRetryDelay(RetryCount));
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    private static TimeSpan GetRetryDelay(int retryCount) => retryCount switch { 1 => TimeSpan.FromSeconds(30), 2 => TimeSpan.FromMinutes(2), _ => TimeSpan.FromMinutes(10) };
}
