using ClinicAssistant.Application;
using ClinicAssistant.Infrastructure;
using ClinicAssistant.Worker.Services;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Worker.Messaging;
using System.Globalization;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using ClinicAssistant.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var migrationCheckLog = LoggerMessage.Define<int, string>(LogLevel.Information, new EventId(9501, "WorkerMigrationCheck"), "Worker database migration check: {PendingCount} pending migration(s): {PendingMigrations}");
var migrationCompletedLog = LoggerMessage.Define(LogLevel.Information, new EventId(9502, "WorkerMigrationsCompleted"), "Worker database migrations completed successfully.");
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Platform administration is registered by the shared infrastructure module and
// depends on HealthCheckService for its operational dashboard. The Worker does
// not expose health endpoints, but it still needs the health-check service in
// its container so the host can validate the complete dependency graph.
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<MessagingWorker>();
builder.Services.AddHostedService<WhatsAppIncomingMessageConsumer>();
builder.Services.AddHostedService<SendWhatsAppMessageConsumer>();
builder.Services.AddHostedService<ConversationMessageReceivedConsumer>();
builder.Services.AddHostedService<AppointmentReminderHostedService>();
builder.Services.AddHostedService<WhatsAppTemplateSyncConsumer>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ClinicAssistant.Worker"))
    .WithTracing(tracing => tracing.AddSource("ClinicAssistant.WhatsApp").AddSource("ClinicAssistant.Conversations").AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddMeter("ClinicAssistant.Conversations").AddMeter("ClinicAssistant.WhatsApp").AddMeter("ClinicAssistant.Worker").AddMeter("ClinicAssistant.Operations").AddOtlpExporter());
builder.Services.AddSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ClinicAssistant.Worker")
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

var host = builder.Build();
await using (var scope = host.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicAssistantDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigrations");
    var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
    if (logger.IsEnabled(LogLevel.Information)) migrationCheckLog(logger, pendingMigrations.Length, string.Join(", ", pendingMigrations), null);
    await dbContext.Database.MigrateAsync();
    migrationCompletedLog(logger, null);
}
await host.RunAsync();
