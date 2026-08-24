using ClinicAssistant.Infrastructure.Conversations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicAssistant.Infrastructure.Operations;

public sealed class HumanQueueEscalationHostedService(IServiceScopeFactory scopes, IOptions<HumanQueueOptions> options, ILogger<HumanQueueEscalationHostedService> logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> EscalationFailed = LoggerMessage.Define(LogLevel.Error, new EventId(9301, "HumanQueueEscalationFailed"), "Human queue notification escalation failed.");
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await using var scope = scopes.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<ClinicAssistant.Application.Operations.IOperationalNotificationService>().ProcessEscalationsAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) { EscalationFailed(logger, ex); }
            await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingSeconds), stoppingToken);
        }
    }
}
