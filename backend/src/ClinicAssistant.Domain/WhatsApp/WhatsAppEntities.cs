using ClinicAssistant.Domain.Primitives;
using ClinicAssistant.Domain.Conversations;

namespace ClinicAssistant.Domain.WhatsApp;

public enum WhatsAppProvider { Fake = 1, Twilio = 2, Meta = 3 }
public enum WhatsAppIntegrationStatus { Pending, Connected, Disconnected, InvalidCredentials, Suspended, Disabled }
public enum WhatsAppTemplateStatus { Draft, PendingApproval, Approved, Rejected, Paused, Disabled }
public enum ConversationChannel { WhatsApp }
public enum ConversationStatus { Bot, WaitingHuman, Human, Closed }
public enum ConversationMessageDirection { Inbound, Outbound }
public enum ConversationMessageType { Text, Template, Interactive, Image, Audio, Document, Location, Contact, System }
public enum ConversationMessageStatus { Pending, Queued, Accepted, Sent, Delivered, Read, Failed, Received }
public enum WhatsAppMediaStatus { PendingValidation, Accepted, RequiresHuman }

public sealed class WhatsAppIntegration : Entity, ITenantEntity
{
    private WhatsAppIntegration() { }

    public WhatsAppIntegration(Guid tenantId, WhatsAppProvider provider, string integrationKey, string whatsAppFrom, string? displayPhoneNumber = null)
    {
        TenantId = tenantId;
        Provider = provider;
        IntegrationKey = integrationKey;
        WhatsAppFrom = whatsAppFrom;
        DisplayPhoneNumber = displayPhoneNumber;
        Status = WhatsAppIntegrationStatus.Pending;
    }

    public Guid TenantId { get; private set; }
    public WhatsAppProvider Provider { get; private set; }
    public string IntegrationKey { get; private set; } = null!;
    public string? AccountSidReference { get; private set; }
    public string? MessagingServiceSid { get; private set; }
    public string WhatsAppFrom { get; private set; } = null!;
    public string? DisplayPhoneNumber { get; private set; }
    public WhatsAppIntegrationStatus Status { get; private set; }
    public DateTimeOffset? ConnectedAt { get; private set; }
    public DateTimeOffset? LastValidatedAt { get; private set; }
    public DateTimeOffset? LastWebhookAt { get; private set; }
    public DateTimeOffset? LastSuccessfulSendAt { get; private set; }
    public DateTimeOffset? LastFailureAt { get; private set; }
    public string? FailureReason { get; private set; }
    public void MarkConnected()
    {
        Status = WhatsAppIntegrationStatus.Connected;
        ConnectedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void Disable() { Status = WhatsAppIntegrationStatus.Disabled; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkValidated() { LastValidatedAt = DateTimeOffset.UtcNow; FailureReason = null; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkSuccessfulSend() { LastSuccessfulSendAt = DateTimeOffset.UtcNow; FailureReason = null; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkSendFailure(string safeReason) { LastFailureAt = DateTimeOffset.UtcNow; FailureReason = safeReason; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class WhatsAppTemplate : Entity, ITenantEntity
{
    private WhatsAppTemplate() { }

    public WhatsAppTemplate(Guid tenantId, Guid integrationId, WhatsAppProvider provider, string contentSid, string name, string languageCode, string? parametersSchema = null)
    {
        TenantId = tenantId;
        IntegrationId = integrationId;
        Provider = provider;
        ContentSid = contentSid;
        Name = name;
        LanguageCode = languageCode;
        ParametersSchema = parametersSchema;
    }

    public Guid TenantId { get; private set; }
    public Guid IntegrationId { get; private set; }
    public WhatsAppProvider Provider { get; private set; }
    public string? ExternalTemplateId { get; private set; }
    public string ContentSid { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string LanguageCode { get; private set; } = null!;
    public string? Category { get; private set; }
    public WhatsAppTemplateStatus Status { get; private set; } = WhatsAppTemplateStatus.Draft;
    public string? ParametersSchema { get; private set; }
    public void MarkApproved() { Status = WhatsAppTemplateStatus.Approved; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Update(string name, string languageCode, string? category, string? parametersSchema)
    {
        Name = name; LanguageCode = languageCode; Category = category; ParametersSchema = parametersSchema; UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void Activate() { Status = WhatsAppTemplateStatus.Approved; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate() { Status = WhatsAppTemplateStatus.Disabled; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class Conversation : Entity, ITenantEntity
{
    private Conversation() { }

    public Conversation(Guid tenantId, Guid patientId, Guid integrationId, string? externalContactId)
    {
        TenantId = tenantId;
        PatientId = patientId;
        IntegrationId = integrationId;
        ExternalContactId = externalContactId;
        Channel = ConversationChannel.WhatsApp;
        Status = ConversationStatus.Bot;
        AutomationMode = ConversationAutomationMode.Automated;
        Priority = ConversationPriority.Normal;
        Version = 1;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid PatientId { get; private set; }
    public ConversationChannel Channel { get; private set; }
    public Guid IntegrationId { get; private set; }
    public string? ExternalContactId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public ConversationAutomationMode AutomationMode { get; private set; }
    public ConversationPriority Priority { get; private set; }
    public int Version { get; private set; }
    public void RegisterMessage(DateTimeOffset occurredAt) { LastMessageAt = occurredAt; UpdatedAt = DateTimeOffset.UtcNow; }
    public void RequestHumanHandoff()
    {
        Status = ConversationStatus.WaitingHuman;
        AutomationMode = ConversationAutomationMode.Human;
        AssignedUserId = null;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void ApplyAutomationMode(ConversationAutomationMode automationMode)
    {
        AutomationMode = automationMode;
        if (automationMode == ConversationAutomationMode.Human) Status = ConversationStatus.WaitingHuman;
        else if (Status != ConversationStatus.Closed) Status = ConversationStatus.Bot;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void Close()
    {
        AutomationMode = ConversationAutomationMode.Paused;
        Status = ConversationStatus.Closed;
        AssignedUserId = null;
        ClosedAt = DateTimeOffset.UtcNow;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void Reopen()
    {
        AutomationMode = ConversationAutomationMode.Automated;
        Status = ConversationStatus.Bot;
        ClosedAt = null;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void Assign(Guid userId) { AssignedUserId = userId; Status = ConversationStatus.Human; AutomationMode = ConversationAutomationMode.Human; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Release() { AssignedUserId = null; Status = ConversationStatus.WaitingHuman; AutomationMode = ConversationAutomationMode.Human; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void PauseAutomation() { AutomationMode = ConversationAutomationMode.Paused; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void ResumeAutomation() { AutomationMode = ConversationAutomationMode.Automated; if (Status != ConversationStatus.Closed) Status = ConversationStatus.Bot; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetPriority(ConversationPriority priority) { Priority = priority; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class ConversationMessage : Entity, ITenantEntity
{
    private ConversationMessage() { }

    public ConversationMessage(Guid tenantId, Guid conversationId, ConversationMessageType type, string? content, WhatsAppProvider provider, string externalMessageId, DateTimeOffset receivedAt)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        Direction = ConversationMessageDirection.Inbound;
        Type = type;
        Content = content;
        ContentSanitized = content;
        Provider = provider;
        ExternalMessageId = externalMessageId;
        Status = ConversationMessageStatus.Received;
        ReceivedAt = receivedAt;
    }

    public ConversationMessage(Guid tenantId, Guid conversationId, ConversationMessageType type, string content, WhatsAppProvider provider)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        Direction = ConversationMessageDirection.Outbound;
        Type = type;
        Content = content;
        ContentSanitized = content;
        Provider = provider;
        Status = ConversationMessageStatus.Pending;
    }

    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public ConversationMessageDirection Direction { get; private set; }
    public ConversationMessageType Type { get; private set; }
    public string? Content { get; private set; }
    public string? ContentSanitized { get; private set; }
    public WhatsAppProvider Provider { get; private set; }
    public string? ExternalMessageId { get; private set; }
    public string? ExternalReplyToMessageId { get; private set; }
    public ConversationMessageStatus Status { get; private set; }
    public string? ProviderStatus { get; private set; }
    public string? ProviderErrorCode { get; private set; }
    public string? ProviderErrorMessage { get; private set; }
    public DateTimeOffset? QueuedAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public DateTimeOffset? ReceivedAt { get; private set; }
    public void MarkAccepted(string externalMessageId, string? providerStatus)
    {
        ExternalMessageId = externalMessageId;
        ProviderStatus = providerStatus;
        Status = ConversationMessageStatus.Accepted;
        AcceptedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void MarkFailed(string? providerErrorCode, string safeError)
    {
        ProviderErrorCode = providerErrorCode;
        ProviderErrorMessage = safeError;
        Status = ConversationMessageStatus.Failed;
        FailedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void UpdateProviderStatus(ConversationMessageStatus status, string providerStatus, string? providerErrorCode, string? safeError)
    {
        Status = status;
        ProviderStatus = providerStatus;
        ProviderErrorCode = providerErrorCode;
        ProviderErrorMessage = safeError;
        var now = DateTimeOffset.UtcNow;
        switch (status)
        {
            case ConversationMessageStatus.Queued: QueuedAt ??= now; break;
            case ConversationMessageStatus.Accepted: AcceptedAt ??= now; break;
            case ConversationMessageStatus.Sent: SentAt ??= now; break;
            case ConversationMessageStatus.Delivered: DeliveredAt ??= now; break;
            case ConversationMessageStatus.Read: ReadAt ??= now; break;
            case ConversationMessageStatus.Failed: FailedAt ??= now; break;
        }
        UpdatedAt = now;
    }
    public void MarkReadByOperator() { ReadAt ??= DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class WhatsAppMedia : Entity, ITenantEntity
{
    private WhatsAppMedia() { }

    public WhatsAppMedia(Guid tenantId, Guid conversationMessageId, string sourceUrl, string? contentType, long? contentLength, int index, WhatsAppMediaStatus status, string? safeReason)
    {
        TenantId = tenantId;
        ConversationMessageId = conversationMessageId;
        SourceUrl = sourceUrl;
        ContentType = contentType;
        ContentLength = contentLength;
        Index = index;
        Status = status;
        SafeReason = safeReason;
    }

    public Guid TenantId { get; private set; }
    public Guid ConversationMessageId { get; private set; }
    public string SourceUrl { get; private set; } = null!;
    public string? ContentType { get; private set; }
    public long? ContentLength { get; private set; }
    public int Index { get; private set; }
    public WhatsAppMediaStatus Status { get; private set; }
    public string? SafeReason { get; private set; }
}
