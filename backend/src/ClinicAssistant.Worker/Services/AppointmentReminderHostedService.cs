using ClinicAssistant.Infrastructure.Scheduling;

namespace ClinicAssistant.Worker.Services;

public sealed partial class AppointmentReminderHostedService(IServiceScopeFactory scopeFactory, Microsoft.Extensions.Options.IOptions<AppointmentReminderOptions> options, ILogger<AppointmentReminderHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await using var scope = scopeFactory.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<AppointmentReminderDispatcher>().DispatchDueAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) { LogDispatchFailure(logger, ex); }
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(options.Value.PollingSeconds, 5, 300)), stoppingToken);
        }
    }
    [LoggerMessage(Level = LogLevel.Error, Message = "Appointment reminder dispatch failed.")]
    private static partial void LogDispatchFailure(ILogger logger, Exception exception);
}
