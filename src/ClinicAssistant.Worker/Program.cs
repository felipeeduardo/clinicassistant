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

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<MessagingWorker>();
builder.Services.AddHostedService<WhatsAppIncomingMessageConsumer>();
builder.Services.AddHostedService<SendWhatsAppMessageConsumer>();
builder.Services.AddHostedService<ConversationMessageReceivedConsumer>();
var rabbitOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
builder.Services.AddSingleton(rabbitOptions);
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ClinicAssistant.Worker"))
    .WithTracing(tracing => tracing.AddSource("ClinicAssistant.WhatsApp").AddSource("ClinicAssistant.Conversations").AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddMeter("ClinicAssistant.Conversations").AddOtlpExporter());
builder.Services.AddSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ClinicAssistant.Worker")
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

await builder.Build().RunAsync();
