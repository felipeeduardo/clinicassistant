using ClinicAssistant.Domain.WhatsApp;

namespace ClinicAssistant.Application.WhatsApp;

public interface IMessageStatusTransitionPolicy
{
    bool CanTransition(ConversationMessageStatus current, ConversationMessageStatus targetStatus);
}

public interface IWhatsAppStatusCallbackService
{
    Task<WhatsAppStatusCallbackResult> ProcessAsync(WhatsAppStatusCallbackRequest request, CancellationToken cancellationToken);
}

public sealed record WhatsAppStatusCallbackRequest(
    string IntegrationKey, string RequestUrl, IReadOnlyDictionary<string, string> Parameters,
    string? Signature);

public sealed record WhatsAppStatusCallbackResult(WhatsAppStatusCallbackResultStatus Status);

public enum WhatsAppStatusCallbackResultStatus { Updated, Unchanged, InvalidSignature, IntegrationNotFound, IntegrationDisabled, InvalidPayload }
