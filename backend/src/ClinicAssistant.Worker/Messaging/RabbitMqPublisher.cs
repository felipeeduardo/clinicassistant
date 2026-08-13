using System.Diagnostics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using ClinicAssistant.Infrastructure.Messaging;

namespace ClinicAssistant.Worker.Messaging;

public sealed class RabbitMqPublisher(RabbitMqConnectionFactory connectionFactory)
{
    public const string LegacyExchange = "clinic.events";
    public const string WhatsAppExchange = "clinicassistant.whatsapp";
    public const string WhatsAppDeadLetterExchange = "clinicassistant.deadletter";

    public async Task DeclareTopologyAsync(CancellationToken cancellationToken)
    {
        await using var connection = await CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(LegacyExchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        foreach (var queue in new[] { "appointments.notifications", "appointments.reminders", "human-handoff" })
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queue, LegacyExchange, queue, cancellationToken: cancellationToken);
        }

        await channel.ExchangeDeclareAsync(WhatsAppExchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(WhatsAppDeadLetterExchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync("whatsapp.deadletter", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync("whatsapp.deadletter", WhatsAppDeadLetterExchange, "#", cancellationToken: cancellationToken);
        foreach (var queue in new[] { "whatsapp.incoming", "whatsapp.outgoing", "whatsapp.status", "whatsapp.conversation", "whatsapp.templates" })
        {
            var arguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = WhatsAppDeadLetterExchange,
                ["x-dead-letter-routing-key"] = "whatsapp.deadletter"
            };
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments, cancellationToken: cancellationToken);
        }
        await channel.QueueBindAsync("whatsapp.incoming", WhatsAppExchange, "whatsapp.incoming", cancellationToken: cancellationToken);
        await channel.QueueBindAsync("whatsapp.outgoing", WhatsAppExchange, "whatsapp.outgoing.#", cancellationToken: cancellationToken);
        await channel.QueueBindAsync("whatsapp.status", WhatsAppExchange, "whatsapp.status.#", cancellationToken: cancellationToken);
        await channel.QueueBindAsync("whatsapp.conversation", WhatsAppExchange, "whatsapp.conversation.#", cancellationToken: cancellationToken);
        await channel.QueueBindAsync("whatsapp.templates", WhatsAppExchange, "whatsapp.templates.#", cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(string messageType, string payload, Guid messageId, Guid tenantId, CancellationToken cancellationToken)
    {
        var destination = GetDestination(messageType, payload);
        await using var connection = await CreateConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(true, true, null, null);
        await using var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);
        var metadata = ReadMetadata(payload);
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId.ToString(),
            ContentType = "application/json",
            Headers = new Dictionary<string, object?>
            {
                ["tenant-id"] = tenantId.ToString(),
                ["integration-id"] = metadata.IntegrationId,
                ["correlation-id"] = metadata.CorrelationId,
                ["trace-id"] = Activity.Current?.TraceId.ToString() ?? string.Empty
            }
        };
        await channel.BasicPublishAsync(destination.Exchange, destination.RoutingKey, mandatory: true, basicProperties: properties, body: Encoding.UTF8.GetBytes(payload), cancellationToken: cancellationToken);
    }

    public async Task PublishDeadLetterAsync(OutboxDeadLetterMessage message, CancellationToken cancellationToken)
    {
        await using var connection = await CreateConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(true, true, null, null);
        await using var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);
        var properties = new BasicProperties { Persistent = true, MessageId = message.OriginalMessageId.ToString(), ContentType = "application/json", Headers = new Dictionary<string, object?> { ["tenant-id"] = message.TenantId.ToString(), ["correlation-id"] = message.CorrelationId, ["trace-id"] = message.TraceId } };
        await channel.BasicPublishAsync(WhatsAppDeadLetterExchange, "whatsapp.deadletter", mandatory: true, basicProperties: properties, body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)), cancellationToken: cancellationToken);
    }

    private static (string Exchange, string RoutingKey) GetDestination(string messageType, string payload) => messageType switch
    {
        "WhatsAppIncomingMessageReceived" => (WhatsAppExchange, "whatsapp.incoming"),
        "ConversationMessageReceived" => (WhatsAppExchange, "whatsapp.conversation.received"),
        "SendWhatsAppMessageCommand" => (WhatsAppExchange, GetOutgoingRoutingKey(payload)),
        "SyncWhatsAppTemplatesCommand" => (WhatsAppExchange, "whatsapp.templates.sync"),
        _ => (LegacyExchange, messageType)
    };

    private static string GetOutgoingRoutingKey(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            return document.RootElement.TryGetProperty("Type", out var type) && type.GetInt32() == 2 ? "whatsapp.outgoing.template" : "whatsapp.outgoing.text";
        }
        catch (JsonException) { return "whatsapp.outgoing.text"; }
    }

    private static (string? IntegrationId, string CorrelationId) ReadMetadata(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var integrationId = root.TryGetProperty("IntegrationId", out var integration) ? integration.ToString() : null;
            var correlationId = root.TryGetProperty("CorrelationId", out var correlation) && !string.IsNullOrWhiteSpace(correlation.GetString()) ? correlation.GetString()! : Guid.NewGuid().ToString("N");
            return (integrationId, correlationId);
        }
        catch (JsonException) { return (null, Guid.NewGuid().ToString("N")); }
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken) => connectionFactory.Create("ClinicAssistant.Worker").CreateConnectionAsync(cancellationToken);
}

public sealed record OutboxDeadLetterMessage(Guid OriginalMessageId, Guid TenantId, string RoutingKey, int RetryCount, DateTimeOffset? FirstFailureAt, DateTimeOffset LastFailureAt, string SafeError, string CorrelationId, string TraceId);
