using System.Text;
using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClinicAssistant.Worker.Services;

public sealed partial class SendWhatsAppMessageConsumer(ILogger<SendWhatsAppMessageConsumer> logger, IServiceScopeFactory scopeFactory, RabbitMqOptions options, RabbitMqPublisher publisher, RabbitMqConnectionFactory connectionFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.DeclareTopologyAsync(stoppingToken);
        await using var connection = await CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(false, false, null, 1), stoppingToken);
        await channel.BasicQosAsync(0, options.ConsumerPrefetchCount, global: false, cancellationToken: stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ProcessAsync(channel, eventArgs, stoppingToken);
        await channel.BasicConsumeAsync("whatsapp.outgoing", false, consumer, stoppingToken);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task ProcessAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken stoppingToken)
    {
        try
        {
            var command = JsonSerializer.Deserialize<SendWhatsAppMessageCommand>(Encoding.UTF8.GetString(eventArgs.Body.Span));
            if (command is null)
            {
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IWhatsAppOutgoingMessageProcessor>();
            var result = await processor.ProcessAsync(command, stoppingToken);
            if (result == WhatsAppOutgoingMessageProcessingResult.Rejected)
            {
                LogRejectedCommand(logger, command.TenantId, command.IntegrationId, command.ConversationMessageId);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }
            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
        catch (JsonException)
        {
            LogInvalidCommand(logger);
            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }
        catch (Exception exception)
        {
            LogSendFailure(logger, exception);
            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken) => connectionFactory.Create("ClinicAssistant.Worker").CreateConnectionAsync(cancellationToken);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WhatsApp outgoing command rejected. TenantId: {TenantId}; IntegrationId: {IntegrationId}; ConversationMessageId: {ConversationMessageId}")]
    private static partial void LogRejectedCommand(ILogger logger, Guid tenantId, Guid integrationId, Guid conversationMessageId);
    [LoggerMessage(Level = LogLevel.Warning, Message = "WhatsApp outgoing command has an invalid payload.")]
    private static partial void LogInvalidCommand(ILogger logger);
    [LoggerMessage(Level = LogLevel.Error, Message = "WhatsApp outgoing command processing failed and was sent to the dead-letter exchange.")]
    private static partial void LogSendFailure(ILogger logger, Exception exception);
}
