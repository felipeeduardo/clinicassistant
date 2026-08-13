using Microsoft.EntityFrameworkCore;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Worker.Messaging;
using System.Diagnostics.Metrics;

namespace ClinicAssistant.Worker.Services;

public sealed class MessagingWorker(ILogger<MessagingWorker> logger, IServiceScopeFactory scopeFactory, RabbitMqPublisher publisher, RabbitMqOptions options) : BackgroundService
{
    private static readonly Meter Meter = new("ClinicAssistant.Worker");
    private static readonly Counter<long> OutboxPublished = Meter.CreateCounter<long>("outbox_published_total");
    private static readonly Counter<long> OutboxFailures = Meter.CreateCounter<long>("outbox_failures_total");
    private static readonly Counter<long> OutboxDeadLetters = Meter.CreateCounter<long>("outbox_dead_letters_total");
    private static readonly Action<ILogger, Exception?> WorkerStarted = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, "MessagingWorkerStarted"),
        "Messaging worker started; transactional outbox publishing is active.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        WorkerStarted(logger, null);
        await publisher.DeclareTopologyAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClinicAssistantDbContext>();
            var now = DateTimeOffset.UtcNow;
            var pending = await dbContext.OutboxMessages.IgnoreQueryFilters().Where(message => message.Status == ClinicAssistant.Domain.Messaging.MessageStatus.Pending && (message.NextAttemptAt == null || message.NextAttemptAt <= now)).OrderBy(message => message.CreatedAt).Take(options.OutboxBatchSize).ToListAsync(stoppingToken);
            foreach (var message in pending)
            {
                try { await publisher.PublishAsync(message.Type, message.Payload, message.Id, message.TenantId, stoppingToken); message.MarkProcessed(); OutboxPublished.Add(1); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                catch (Exception exception)
                {
                    message.MarkFailure(exception.GetType().Name, options.MaximumRetryAttempts);
                    OutboxFailures.Add(1);
                    if (message.Status == ClinicAssistant.Domain.Messaging.MessageStatus.DeadLettered)
                    {
                        OutboxDeadLetters.Add(1);
                        var deadLetter = new OutboxDeadLetterMessage(message.Id, message.TenantId, message.Type, message.RetryCount, message.FirstFailureAt, DateTimeOffset.UtcNow, exception.GetType().Name, message.Id.ToString("N"), System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty);
                        await publisher.PublishDeadLetterAsync(deadLetter, stoppingToken);
                    }
                }
            }
            if (pending.Count > 0) await dbContext.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(options.OutboxPollingSeconds), stoppingToken);
        }
    }
}
