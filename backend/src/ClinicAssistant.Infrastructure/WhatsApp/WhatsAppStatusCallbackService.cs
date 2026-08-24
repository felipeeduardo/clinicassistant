using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Application.Realtime;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppStatusCallbackService(ClinicAssistantDbContext dbContext, ITwilioWebhookSignatureValidator signatureValidator, IMessageStatusTransitionPolicy transitionPolicy, IOperationalEventPublisher events) : IWhatsAppStatusCallbackService
{
    public async Task<WhatsAppStatusCallbackResult> ProcessAsync(WhatsAppStatusCallbackRequest request, CancellationToken cancellationToken)
    {
        using var activity = WhatsAppTelemetry.ActivitySource.StartActivity("whatsapp.webhook.status");
        var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.IntegrationKey == request.IntegrationKey, cancellationToken);
        if (integration is null) return new(WhatsAppStatusCallbackResultStatus.IntegrationNotFound);
        if (integration.Provider != WhatsAppProvider.Twilio || integration.Status != WhatsAppIntegrationStatus.Connected) return new(WhatsAppStatusCallbackResultStatus.IntegrationDisabled);
        if (!signatureValidator.IsValid(request.RequestUrl, request.Parameters, request.Signature))
        {
            WhatsAppTelemetry.InvalidSignature.Add(1);
            return new(WhatsAppStatusCallbackResultStatus.InvalidSignature);
        }
        var messageSid = Get(request.Parameters, "MessageSid");
        var providerStatus = Get(request.Parameters, "MessageStatus");
        if (string.IsNullOrWhiteSpace(messageSid) || string.IsNullOrWhiteSpace(providerStatus) || !TryMapStatus(providerStatus, out var nextStatus)) return new(WhatsAppStatusCallbackResultStatus.InvalidPayload);

        // MessageSid is the provider correlation key; resolve the tenant from the persisted message,
        // never from a guessed/default tenant.
        var message = await dbContext.ConversationMessages.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Provider == WhatsAppProvider.Twilio && item.ExternalMessageId == messageSid, cancellationToken);
        if (message is null) return new(WhatsAppStatusCallbackResultStatus.Unchanged);
        var conversation = await dbContext.Conversations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == message.ConversationId && item.TenantId == message.TenantId && item.IntegrationId == integration.Id, cancellationToken);
        if (conversation is null) return new(WhatsAppStatusCallbackResultStatus.Unchanged);
        if (!transitionPolicy.CanTransition(message.Status, nextStatus)) return new(WhatsAppStatusCallbackResultStatus.Unchanged);

        var errorCode = Get(request.Parameters, "ErrorCode");
        var safeError = nextStatus == ConversationMessageStatus.Failed ? "The WhatsApp provider reported a delivery failure." : null;
        message.UpdateProviderStatus(nextStatus, providerStatus.ToLowerInvariant(), errorCode, safeError);
        if (nextStatus == ConversationMessageStatus.Failed) integration.MarkSendFailure(safeError!);
        else integration.MarkSuccessfulSend();
        var channel = conversation.WhatsAppChannelId.HasValue ? await dbContext.WhatsAppChannels.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == conversation.WhatsAppChannelId.Value, cancellationToken) : null;
        if (channel is not null) channel.MarkOutbound();
        await dbContext.SaveChangesAsync(cancellationToken);
        await events.PublishAsync(integration.TenantId, "whatsapp.message.status.changed", new { MessageId = message.Id, ConversationId = message.ConversationId, Status = message.Status.ToString() }, cancellationToken);
        WhatsAppTelemetry.StatusUpdates.Add(1);
        return new(WhatsAppStatusCallbackResultStatus.Updated);
    }

    private static string? Get(IReadOnlyDictionary<string, string> parameters, string key) => parameters.TryGetValue(key, out var value) ? value : null;

    private static bool TryMapStatus(string providerStatus, out ConversationMessageStatus status)
    {
        status = providerStatus.ToLowerInvariant() switch
        {
            "queued" or "sending" => ConversationMessageStatus.Queued,
            "accepted" => ConversationMessageStatus.Accepted,
            "sent" => ConversationMessageStatus.Sent,
            "delivered" => ConversationMessageStatus.Delivered,
            "read" => ConversationMessageStatus.Read,
            "failed" or "undelivered" or "canceled" => ConversationMessageStatus.Failed,
            _ => ConversationMessageStatus.Pending
        };
        return providerStatus.Equals("queued", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("sending", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("accepted", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("sent", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("delivered", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("read", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("failed", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("undelivered", StringComparison.OrdinalIgnoreCase) || providerStatus.Equals("canceled", StringComparison.OrdinalIgnoreCase);
    }
}
