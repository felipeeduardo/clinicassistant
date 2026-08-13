using System.Diagnostics.Metrics;
using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppIncomingMessageProcessor(ClinicAssistantDbContext dbContext, IWhatsAppMediaPolicy mediaPolicy, IOperationalEventPublisher events) : IWhatsAppIncomingMessageProcessor
{
    public async Task<WhatsAppIncomingMessageProcessingResult> ProcessAsync(WhatsAppIncomingMessageReceived message, CancellationToken cancellationToken)
    {
        using var activity = WhatsAppTelemetry.ActivitySource.StartActivity("whatsapp.incoming.process");
        if (message.TenantId == Guid.Empty || message.IntegrationId == Guid.Empty || message.InboxMessageId == Guid.Empty || string.IsNullOrWhiteSpace(message.ExternalMessageId)) return WhatsAppIncomingMessageProcessingResult.Rejected;
        var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == message.IntegrationId && item.TenantId == message.TenantId && item.Provider == WhatsAppProvider.Twilio && item.Status == WhatsAppIntegrationStatus.Connected, cancellationToken);
        if (integration is null) return WhatsAppIncomingMessageProcessingResult.Rejected;
        var inboxMessage = await dbContext.InboxMessages.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == message.InboxMessageId && item.TenantId == message.TenantId && item.IntegrationId == message.IntegrationId && item.ExternalMessageId == message.ExternalMessageId, cancellationToken);
        if (inboxMessage is null) return WhatsAppIncomingMessageProcessingResult.Rejected;
        if (inboxMessage.Status == MessageStatus.Processed) return WhatsAppIncomingMessageProcessingResult.Duplicate;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            inboxMessage.MarkProcessing();
            var patient = await dbContext.Patients.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.TenantId == message.TenantId && item.Phone == message.SenderPhone, cancellationToken);
            if (patient is null)
            {
                patient = new Patient(message.TenantId, string.IsNullOrWhiteSpace(message.ProfileName) ? "Paciente WhatsApp" : message.ProfileName, message.SenderPhone, null, null, ConsentStatus.Unknown, PatientSource.WhatsApp);
                dbContext.Patients.Add(patient);
            }
            patient.RegisterContact(message.ReceivedAt);

            var conversation = await dbContext.Conversations.IgnoreQueryFilters().Where(item => item.TenantId == message.TenantId && item.PatientId == patient.Id && item.IntegrationId == message.IntegrationId && item.Channel == ConversationChannel.WhatsApp && item.Status != ConversationStatus.Closed).OrderByDescending(item => item.LastMessageAt).FirstOrDefaultAsync(cancellationToken);
            if (conversation is null)
            {
                conversation = new Conversation(message.TenantId, patient.Id, message.IntegrationId, message.SenderPhone);
                dbContext.Conversations.Add(conversation);
            }
            conversation.RegisterMessage(message.ReceivedAt);

            var conversationMessage = new ConversationMessage(message.TenantId, conversation.Id, ToConversationMessageType(message), message.Text, WhatsAppProvider.Twilio, message.ExternalMessageId, message.ReceivedAt);
            dbContext.ConversationMessages.Add(conversationMessage);
            var mediaEvaluations = message.Media.Select(media => new { Media = media, Policy = mediaPolicy.Evaluate(media.ContentType, media.ContentLength) }).ToArray();
            foreach (var evaluation in mediaEvaluations)
            {
                var status = evaluation.Policy.Disposition == WhatsAppMediaDisposition.Accepted
                    ? evaluation.Policy.RequiresDeferredSizeValidation ? WhatsAppMediaStatus.PendingValidation : WhatsAppMediaStatus.Accepted
                    : WhatsAppMediaStatus.RequiresHuman;
                dbContext.WhatsAppMedia.Add(new WhatsAppMedia(message.TenantId, conversationMessage.Id, evaluation.Media.Url, evaluation.Media.ContentType, evaluation.Media.ContentLength, evaluation.Media.Index, status, evaluation.Policy.SafeReason));
            }
            if (mediaEvaluations.Any(evaluation => evaluation.Policy.Disposition == WhatsAppMediaDisposition.RequiresHuman)) conversation.RequestHumanHandoff();
            if (mediaEvaluations.Length > 0) WhatsAppMediaMetrics.Received.Add(mediaEvaluations.Length);
            dbContext.OutboxMessages.Add(new OutboxMessage(message.TenantId, nameof(ConversationMessageReceived), JsonSerializer.Serialize(new ConversationMessageReceived(message.TenantId, message.IntegrationId, conversation.Id, conversationMessage.Id, message.CorrelationId))));
            inboxMessage.MarkProcessed();
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            WhatsAppTelemetry.IncomingMessages.Add(1);
            await events.PublishAsync(message.TenantId, "whatsapp.inbound.received", new
            {
                ConversationId = conversation.Id,
                MessageId = conversationMessage.Id,
                Type = conversationMessage.Type.ToString(),
                conversation.Version
            }, cancellationToken);
            return WhatsAppIncomingMessageProcessingResult.Processed;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await transaction.RollbackAsync(cancellationToken);
            return WhatsAppIncomingMessageProcessingResult.Duplicate;
        }
    }

    private static ConversationMessageType ToConversationMessageType(WhatsAppIncomingMessageReceived message) => message.Type switch
    {
        WhatsAppIncomingMessageType.Media => ToMediaMessageType(message.Media.FirstOrDefault()?.ContentType),
        WhatsAppIncomingMessageType.Location => ConversationMessageType.Location,
        WhatsAppIncomingMessageType.Contact => ConversationMessageType.Contact,
        WhatsAppIncomingMessageType.Interactive => ConversationMessageType.Interactive,
        WhatsAppIncomingMessageType.Text => ConversationMessageType.Text,
        _ => ConversationMessageType.System
    };

    private static ConversationMessageType ToMediaMessageType(string? contentType) => contentType?.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/png" => ConversationMessageType.Image,
        "audio/ogg" => ConversationMessageType.Audio,
        "application/pdf" => ConversationMessageType.Document,
        _ => ConversationMessageType.System
    };
}

internal static class WhatsAppMediaMetrics
{
    private static readonly Meter Meter = new("ClinicAssistant.WhatsApp");
    internal static readonly Counter<long> Received = Meter.CreateCounter<long>("whatsapp_media_received_total");
}
