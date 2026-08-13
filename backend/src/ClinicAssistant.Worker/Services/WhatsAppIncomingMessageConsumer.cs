using System.Text;
using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClinicAssistant.Worker.Services;

public sealed partial class WhatsAppIncomingMessageConsumer(ILogger<WhatsAppIncomingMessageConsumer> logger, IServiceScopeFactory scopeFactory, RabbitMqOptions options, RabbitMqPublisher publisher, RabbitMqConnectionFactory connectionFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.DeclareTopologyAsync(stoppingToken);
        await using var connection = await CreateConnectionAsync(stoppingToken);
        var channelOptions = new CreateChannelOptions(false, false, null, 1);
        await using var channel = await connection.CreateChannelAsync(channelOptions, stoppingToken);
        await channel.BasicQosAsync(0, options.ConsumerPrefetchCount, global: false, cancellationToken: stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ProcessAsync(channel, eventArgs, stoppingToken);
        await channel.BasicConsumeAsync("whatsapp.incoming", false, consumer, stoppingToken);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task ProcessAsync(IChannel channel, BasicDeliverEventArgs eventArgs, CancellationToken stoppingToken)
    {
        try
        {
            var message = JsonSerializer.Deserialize<WhatsAppIncomingMessageReceived>(Encoding.UTF8.GetString(eventArgs.Body.Span));
            if (message is null)
            {
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IWhatsAppIncomingMessageProcessor>();
            var result = await processor.ProcessAsync(message, stoppingToken);
            if (result == WhatsAppIncomingMessageProcessingResult.Rejected)
            {
                LogRejectedMessage(logger, message.TenantId, message.IntegrationId, message.ExternalMessageId);
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
            await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken) => connectionFactory.Create("ClinicAssistant.Worker").CreateConnectionAsync(cancellationToken);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WhatsApp incoming message rejected. TenantId: {TenantId}; IntegrationId: {IntegrationId}; ExternalMessageId: {ExternalMessageId}")]
    private static partial void LogRejectedMessage(ILogger logger, Guid tenantId, Guid integrationId, string externalMessageId);
    [LoggerMessage(Level = LogLevel.Warning, Message = "WhatsApp incoming message has an invalid payload.")]
    private static partial void LogInvalidPayload(ILogger logger);
    [LoggerMessage(Level = LogLevel.Error, Message = "WhatsApp incoming message processing failed and was sent to the dead-letter exchange.")]
    private static partial void LogProcessingFailure(ILogger logger, Exception exception);
}
