namespace ClinicAssistant.Application.WhatsApp;

public interface IWhatsAppGateway
{
    WhatsAppGatewayCapabilities Capabilities { get; }
    Task<SendWhatsAppMessageResult> SendTextAsync(SendWhatsAppTextRequest request, CancellationToken cancellationToken);
    Task<SendWhatsAppMessageResult> SendInteractiveAsync(SendWhatsAppInteractiveRequest request, CancellationToken cancellationToken);
    Task<SendWhatsAppMessageResult> SendTemplateAsync(SendWhatsAppTemplateRequest request, CancellationToken cancellationToken);
    Task<SendWhatsAppMessageResult> SendMediaAsync(SendWhatsAppMediaRequest request, CancellationToken cancellationToken);
}

public sealed record SendWhatsAppTextRequest(
    Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId,
    string RecipientPhone, string Text, string IdempotencyKey, string? CorrelationId);

public sealed record SendWhatsAppInteractiveRequest(
    Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId,
    string RecipientPhone, string Text, WhatsAppInteraction Interaction,
    string IdempotencyKey, string? CorrelationId);

public sealed record WhatsAppInteraction(WhatsAppInteractionType Type, IReadOnlyCollection<WhatsAppChoice> Choices);
public sealed record WhatsAppChoice(string ActionId, string Label, string? Description = null);
public enum WhatsAppInteractionType { List = 1, ReplyButtons = 2 }
public sealed record WhatsAppGatewayCapabilities(bool SupportsInteractiveLists, bool SupportsReplyButtons, bool SupportsFreeformText);

public sealed record SendWhatsAppTemplateRequest(
    Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId,
    string RecipientPhone, string ContentSid, IReadOnlyDictionary<string, string> Variables,
    string IdempotencyKey, string? CorrelationId);

public sealed record SendWhatsAppMediaRequest(
    Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId,
    string RecipientPhone, string MediaUrl, string? Caption, string IdempotencyKey, string? CorrelationId);

public sealed record SendWhatsAppMessageResult(
    bool Success, string? ExternalMessageId, string? ProviderStatus, WhatsAppFailure? Failure);

public sealed record WhatsAppFailure(
    WhatsAppFailureType Type, string? ProviderCode, string SafeMessage, bool CanRetry);

public enum WhatsAppFailureType
{
    Unknown = 0,
    Transient = 1,
    Permanent = 2,
    Authentication = 3,
    RateLimit = 4,
    InvalidRecipient = 5,
    InvalidTemplate = 6,
    PolicyViolation = 7,
    IntegrationDisabled = 8
}

public interface ITwilioWebhookSignatureValidator
{
    bool IsValid(string requestUrl, IReadOnlyDictionary<string, string> parameters, string? signature);
}

public interface ITwilioWhatsAppWebhookParser
{
    WhatsAppIncomingMessageReceived Parse(TwilioIncomingWebhook webhook, Guid tenantId, Guid integrationId, Guid inboxMessageId, string correlationId);
}

public interface IWhatsAppIncomingWebhookService
{
    Task<WhatsAppIncomingWebhookResult> ProcessAsync(WhatsAppIncomingWebhookRequest request, CancellationToken cancellationToken);
}

public interface IWhatsAppIncomingMessageProcessor
{
    Task<WhatsAppIncomingMessageProcessingResult> ProcessAsync(WhatsAppIncomingMessageReceived message, CancellationToken cancellationToken);
}

public interface IWhatsAppOutgoingMessageProcessor
{
    Task<WhatsAppOutgoingMessageProcessingResult> ProcessAsync(SendWhatsAppMessageCommand command, CancellationToken cancellationToken);
}

public sealed record TwilioIncomingWebhook(
    string? MessageSid, string? AccountSid, string? From, string? To, string? Body,
    string? ProfileName, string? WaId, int NumMedia, string? ButtonText, string? ButtonPayload,
    string? Latitude, string? Longitude, string? Address, IReadOnlyCollection<WhatsAppIncomingMedia> Media);

public sealed record WhatsAppIncomingWebhookRequest(
    string IntegrationKey, string RequestUrl, IReadOnlyDictionary<string, string> Parameters,
    string? Signature, string RawPayload, string CorrelationId);

public sealed record WhatsAppIncomingWebhookResult(WhatsAppIncomingWebhookStatus Status);

public enum WhatsAppIncomingWebhookStatus { Accepted, Duplicate, InvalidSignature, IntegrationNotFound, IntegrationDisabled, InvalidPayload }
public enum WhatsAppIncomingMessageProcessingResult { Processed, Duplicate, Rejected }
public enum WhatsAppOutgoingMessageProcessingResult { Sent, Failed, Duplicate, Rejected }

public sealed record WhatsAppIncomingMessageReceived(
    Guid TenantId, Guid IntegrationId, Guid InboxMessageId, string ExternalMessageId,
    string SenderPhone, string RecipientPhone, WhatsAppIncomingMessageType Type, string? Text,
    IReadOnlyCollection<WhatsAppIncomingMedia> Media, string? ProfileName, string? ActionId, DateTimeOffset ReceivedAt,
    string CorrelationId);

public enum WhatsAppIncomingMessageType { Unknown = 0, Text = 1, Media = 2, Location = 3, Interactive = 4, Contact = 5 }

public sealed record WhatsAppIncomingMedia(string Url, string? ContentType, int Index, long? ContentLength = null);

public interface IWhatsAppMediaPolicy
{
    WhatsAppMediaPolicyResult Evaluate(string? contentType, long? contentLength);
}

public sealed record WhatsAppMediaPolicyResult(
    WhatsAppMediaDisposition Disposition,
    bool RequiresDeferredSizeValidation,
    string? SafeReason);

public enum WhatsAppMediaDisposition { Accepted, RequiresHuman }

public sealed record ConversationMessageReceived(Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId, string CorrelationId);

public enum WhatsAppOutgoingMessageType { Text = 1, Template = 2, Media = 3, Interactive = 4 }

public sealed record SendWhatsAppMessageCommand(
    Guid TenantId, Guid IntegrationId, Guid ConversationId, Guid ConversationMessageId,
    WhatsAppOutgoingMessageType Type, string RecipientPhone, string? Text, string? ContentSid,
    IReadOnlyDictionary<string, string>? ContentVariables, string? MediaUrl, string IdempotencyKey,
    string CorrelationId, WhatsAppInteraction? Interaction = null);
