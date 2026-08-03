using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Infrastructure.Identity;
using ClinicAssistant.Application.Clinics;
using ClinicAssistant.Infrastructure.Clinics;
using ClinicAssistant.Application.Scheduling;
using ClinicAssistant.Infrastructure.Scheduling;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using ClinicAssistant.Infrastructure.Conversations;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Infrastructure.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClinicAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default must be configured.");

        services.AddDbContext<ClinicAssistantDbContext>(options => options.UseNpgsql(connectionString));
        var redisHost = configuration["Redis:Host"] ?? "localhost";
        var redisPort = configuration.GetValue<int?>("Redis:Port") ?? 6379;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { { redisHost, redisPort } },
            AbortOnConnectFail = false
        }));
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<TenantAccessGuard>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClinicCatalogService, ClinicCatalogService>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.SectionName));
        services.AddOptions<ConversationOptions>()
            .Bind(configuration.GetSection(ConversationOptions.SectionName))
            .Validate(options => options.StateExpirationMinutes is > 0 and <= 1_440, "Conversation:StateExpirationMinutes must be between 1 and 1440.")
            .Validate(options => options.IdleCloseHours is > 0 and <= 720, "Conversation:IdleCloseHours must be between 1 and 720.")
            .Validate(options => options.MaximumInvalidAttempts is > 0 and <= 10, "Conversation:MaximumInvalidAttempts must be between 1 and 10.")
            .Validate(options => options.LockTimeoutSeconds > 0 && options.LockTimeoutSeconds <= options.LockTtlSeconds, "Conversation lock timeout must be positive and no greater than its TTL.")
            .Validate(options => options.LockTtlSeconds is > 0 and <= 300, "Conversation:LockTtlSeconds must be between 1 and 300.")
            .Validate(options => options.MaxOptionsPerMessage is > 0 and <= 10, "Conversation:MaxOptionsPerMessage must be between 1 and 10.")
            .Validate(options => options.MaxMessageLength is > 0 and <= 4_000, "Conversation:MaxMessageLength must be between 1 and 4000.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultLanguage), "Conversation:DefaultLanguage is required.")
            .ValidateOnStart();
        services.AddSingleton<IConversationStateMachine, ConversationStateMachine>();
        services.AddSingleton<IConversationResponseComposer, InMemoryConversationResponseComposer>();
        services.AddScoped<IConversationLockManager, RedisConversationLock>();
        services.AddScoped<IConversationOrchestrator, ConversationOrchestrator>();
        services.AddScoped<IConversationAdministrationService, ConversationAdministrationService>();
        services.AddScoped<IPlatformAdministrationService, PlatformAdministrationService>();
        services.AddSingleton<IOperationalEventPublisher, NoOpOperationalEventPublisher>();
        services.AddSingleton<IWhatsAppPhoneNumberFormatter, WhatsAppPhoneNumberFormatter>();
        services.AddSingleton<IPhoneMasker, PhoneMasker>();
        services.AddSingleton<IWhatsAppConversationWindowPolicy, WhatsAppConversationWindowPolicy>();
        services.AddSingleton<IWhatsAppMediaPolicy, WhatsAppMediaPolicy>();
        services.AddSingleton<IWhatsAppTemplateVariableValidator, WhatsAppTemplateVariableValidator>();
        services.AddSingleton<IMessageStatusTransitionPolicy, MessageStatusTransitionPolicy>();
        services.AddSingleton<ITwilioWebhookSignatureValidator, TwilioWebhookSignatureValidator>();
        services.AddSingleton<ITwilioWhatsAppWebhookParser, TwilioWhatsAppWebhookParser>();
        services.AddSingleton<TwilioWebhookUrlResolver>();
        services.AddScoped<IWhatsAppIncomingWebhookService, WhatsAppIncomingWebhookService>();
        services.AddScoped<IWhatsAppIncomingMessageProcessor, WhatsAppIncomingMessageProcessor>();
        services.AddScoped<IWhatsAppOutgoingMessageProcessor, WhatsAppOutgoingMessageProcessor>();
        services.AddScoped<IWhatsAppStatusCallbackService, WhatsAppStatusCallbackService>();
        services.AddScoped<IWhatsAppIntegrationStatusService, WhatsAppIntegrationStatusService>();
        services.AddSingleton<FakeWhatsAppGateway>();
        services.AddHttpClient<ITwilioMessageClient, TwilioHttpMessageClient>((serviceProvider, client) =>
        {
            var twilioOptions = configuration.GetSection(TwilioOptions.SectionName).Get<TwilioOptions>() ?? new TwilioOptions();
            client.BaseAddress = new Uri(twilioOptions.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(twilioOptions.RequestTimeoutSeconds);
        });
        services.AddScoped<TwilioWhatsAppGateway>();
        services.AddScoped<IWhatsAppGateway>(serviceProvider =>
        {
            var whatsAppOptions = configuration.GetSection(WhatsAppOptions.SectionName).Get<WhatsAppOptions>() ?? new WhatsAppOptions();
            return whatsAppOptions.Provider switch
            {
                WhatsAppProvider.Fake => serviceProvider.GetRequiredService<FakeWhatsAppGateway>(),
                WhatsAppProvider.Twilio => serviceProvider.GetRequiredService<TwilioWhatsAppGateway>(),
                _ => throw new InvalidOperationException($"WhatsApp provider '{whatsAppOptions.Provider}' is not supported in this release.")
            };
        });
        return services;
    }
}
