using System.Security.Cryptography;
using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppIncomingWebhookService(
    ClinicAssistantDbContext dbContext,
    ITwilioWebhookSignatureValidator signatureValidator,
    ITwilioWhatsAppWebhookParser parser) : IWhatsAppIncomingWebhookService
{
    public async Task<WhatsAppIncomingWebhookResult> ProcessAsync(WhatsAppIncomingWebhookRequest request, CancellationToken cancellationToken)
    {
        using var activity = WhatsAppTelemetry.ActivitySource.StartActivity("whatsapp.webhook.incoming");
        var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.IntegrationKey == request.IntegrationKey, cancellationToken);
        if (integration is null) return new(WhatsAppIncomingWebhookStatus.IntegrationNotFound);
        if (integration.Provider != WhatsAppProvider.Twilio || integration.Status != WhatsAppIntegrationStatus.Connected)
            return new(WhatsAppIncomingWebhookStatus.IntegrationDisabled);
        if (!signatureValidator.IsValid(request.RequestUrl, request.Parameters, request.Signature))
        {
            WhatsAppTelemetry.InvalidSignature.Add(1);
            return new(WhatsAppIncomingWebhookStatus.InvalidSignature);
        }

        var webhook = ToWebhook(request.Parameters);
        if (string.IsNullOrWhiteSpace(webhook.MessageSid)) return new(WhatsAppIncomingWebhookStatus.InvalidPayload);
        var inboxMessage = new InboxMessage(integration.TenantId, integration.Id, "Twilio", "incoming_message", webhook.MessageSid,
            Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.RawPayload))), request.RawPayload, request.CorrelationId);

        WhatsAppIncomingMessageReceived message;
        try { message = parser.Parse(webhook, integration.TenantId, integration.Id, inboxMessage.Id, request.CorrelationId); }
        catch (InvalidOperationException) { return new(WhatsAppIncomingWebhookStatus.InvalidPayload); }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.InboxMessages.Add(inboxMessage);
        dbContext.OutboxMessages.Add(new OutboxMessage(integration.TenantId, nameof(WhatsAppIncomingMessageReceived), JsonSerializer.Serialize(message)));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(WhatsAppIncomingWebhookStatus.Accepted);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            WhatsAppTelemetry.Duplicate.Add(1);
            return new(WhatsAppIncomingWebhookStatus.Duplicate);
        }
    }

    private static TwilioIncomingWebhook ToWebhook(IReadOnlyDictionary<string, string> parameters)
    {
        var media = Enumerable.Range(0, GetInt(parameters, "NumMedia"))
            .Select(index => new WhatsAppIncomingMedia(Get(parameters, $"MediaUrl{index}") ?? string.Empty, Get(parameters, $"MediaContentType{index}"), index))
            .Where(mediaItem => !string.IsNullOrWhiteSpace(mediaItem.Url))
            .ToArray();
        return new(Get(parameters, "MessageSid") ?? Get(parameters, "SmsMessageSid"), Get(parameters, "AccountSid"), Get(parameters, "From"), Get(parameters, "To"), Get(parameters, "Body"),
            Get(parameters, "ProfileName"), Get(parameters, "WaId"), media.Length, Get(parameters, "ButtonText"), Get(parameters, "ButtonPayload"),
            Get(parameters, "Latitude"), Get(parameters, "Longitude"), Get(parameters, "Address"), media);
    }

    private static string? Get(IReadOnlyDictionary<string, string> parameters, string key) => parameters.TryGetValue(key, out var value) ? value : null;
    private static int GetInt(IReadOnlyDictionary<string, string> parameters, string key) => int.TryParse(Get(parameters, key), out var value) ? Math.Clamp(value, 0, 10) : 0;
    private static bool IsUniqueViolation(DbUpdateException exception) => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
