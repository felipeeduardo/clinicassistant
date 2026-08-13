using System.Text;
using System.Text.Json;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClinicAssistant.Worker.Services;

public sealed partial class ConversationMessageReceivedConsumer(ILogger<ConversationMessageReceivedConsumer> logger, IServiceScopeFactory scopeFactory, RabbitMqOptions options, RabbitMqPublisher publisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.DeclareTopologyAsync(stoppingToken);
        await using var connection = await CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(false, false, null, 1), stoppingToken);
        await channel.BasicQosAsync(0, options.ConsumerPrefetchCount, global: false, cancellationToken: stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ProcessAsync(channel, eventArgs, stoppingToken);
        await channel.BasicConsumeAsync("whatsapp.conversation", false, consumer, stoppingToken);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task ProcessAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken stoppingToken)
    {
        try
        {
            var message = JsonSerializer.Deserialize<ConversationMessageReceived>(Encoding.UTF8.GetString(eventArgs.Body.Span));
            if (message is null)
            {
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<IConversationOrchestrator>();
            var result = await orchestrator.ProcessAsync(new(message.TenantId, message.IntegrationId, message.ConversationId, message.ConversationMessageId, message.CorrelationId), stoppingToken);
            if (result is ConversationOrchestrationResult.LockUnavailable or ConversationOrchestrationResult.ConcurrencyConflict)
            {
                LogRetrying(logger, message.TenantId, message.ConversationId, result);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                return;
            }
            if (result == ConversationOrchestrationResult.Rejected)
            {
                LogRejected(logger, message.TenantId, message.ConversationId, message.ConversationMessageId);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }
            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
        catch (JsonException)
        {
            LogInvalidPayload(logger);
            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }
        catch (Exception exception)
        {
            LogProcessingFailure(logger, exception);
            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
        }
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken) => new ConnectionFactory { HostName = options.Host, Port = options.Port, UserName = options.Username, Password = options.Password }.CreateConnectionAsync(cancellationToken);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Conversation processing will retry. TenantId: {TenantId}; ConversationId: {ConversationId}; Result: {Result}")]
    private static partial void LogRetrying(ILogger logger, Guid tenantId, Guid conversationId, ConversationOrchestrationResult result);
    [LoggerMessage(Level = LogLevel.Warning, Message = "Conversation processing was rejected. TenantId: {TenantId}; ConversationId: {ConversationId}; ConversationMessageId: {ConversationMessageId}")]
    private static partial void LogRejected(ILogger logger, Guid tenantId, Guid conversationId, Guid conversationMessageId);
    [LoggerMessage(Level = LogLevel.Warning, Message = "Conversation event has an invalid payload.")]
    private static partial void LogInvalidPayload(ILogger logger);
    [LoggerMessage(Level = LogLevel.Error, Message = "Conversation processing failed and will be retried.")]
    private static partial void LogProcessingFailure(ILogger logger, Exception exception);
}
