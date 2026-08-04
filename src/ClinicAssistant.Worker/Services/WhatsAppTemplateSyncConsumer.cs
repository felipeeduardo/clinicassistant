using System.Text;
using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClinicAssistant.Worker.Services;

public sealed class WhatsAppTemplateSyncConsumer(IServiceScopeFactory scopeFactory, RabbitMqOptions options, RabbitMqPublisher publisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.DeclareTopologyAsync(stoppingToken); await using var connection = await new ConnectionFactory { HostName = options.Host, Port = options.Port, UserName = options.Username, Password = options.Password }.CreateConnectionAsync(stoppingToken); await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(false, false, null, 1), stoppingToken); var consumer = new AsyncEventingBasicConsumer(channel); consumer.ReceivedAsync += async (_, delivery) => { try { var command = JsonSerializer.Deserialize<SyncWhatsAppTemplatesCommand>(Encoding.UTF8.GetString(delivery.Body.Span)); if (command is null) { OperationalTelemetry.WhatsAppTemplateSyncFailures.Add(1); await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken); return; } await using var scope = scopeFactory.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<IWhatsAppTemplateSyncProcessor>().ProcessAsync(command, stoppingToken); await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken); } catch { OperationalTelemetry.WhatsAppTemplateSyncFailures.Add(1); await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken); } }; await channel.BasicConsumeAsync("whatsapp.templates", false, consumer, stoppingToken); await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
