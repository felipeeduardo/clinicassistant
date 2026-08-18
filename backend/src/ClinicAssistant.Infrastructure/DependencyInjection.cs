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
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Infrastructure.Operations;
using ClinicAssistant.Application.Leads;
using ClinicAssistant.Infrastructure.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ClinicAssistant.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options =>
            {
                options.Validate();
                if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase) && !options.UseTls)
                    throw new InvalidOperationException("RabbitMq:UseTls must be true in Production.");
                return true;
            })
            .ValidateOnStart();
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
        services.AddSingleton<RabbitMqConnectionFactory>();
        var target = configuration["Database:Target"]?.Trim().ToLowerInvariant() ?? "primary";
        if (target == "test" && string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Database target 'test' is blocked in Production.");
        var connectionString = DatabaseConnectionStringResolver.Resolve(configuration);

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
        services.AddOptions<PasswordRecoveryOptions>().Bind(configuration.GetSection(PasswordRecoveryOptions.SectionName));
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IPasswordResetEmailSender, PasswordResetEmailSender>();
        services.AddOptions<PlatformBootstrapOptions>()
            .Bind(configuration.GetSection(PlatformBootstrapOptions.SectionName));
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
        services.AddSingleton<IConversationIntentResolver, ConversationIntentResolver>();
        services.AddSingleton<IConversationStateMachine, ConversationStateMachine>();
        services.AddSingleton<IConversationResponseComposer, InMemoryConversationResponseComposer>();
        services.AddScoped<IConversationLockManager, RedisConversationLock>();
        services.AddScoped<IConversationOrchestrator, ConversationOrchestrator>();
        services.AddScoped<IConversationAdministrationService, ConversationAdministrationService>();
        services.AddScoped<IPlatformAdministrationService, PlatformAdministrationService>();
        services.AddScoped<IPlatformBootstrapService, PlatformBootstrapService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDemoLeadService, DemoLeadService>();
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
        services.AddScoped<IWhatsAppTemplateAdministrationService, WhatsAppTemplateAdministrationService>();
        services.AddScoped<IWhatsAppTemplateSyncProcessor, WhatsAppTemplateSyncProcessor>();
        services.AddSingleton<FakeWhatsAppGateway>();
        services.AddHttpClient<ITwilioMessageClient, TwilioHttpMessageClient>((serviceProvider, client) =>
        {
            var twilioOptions = configuration.GetSection(TwilioOptions.SectionName).Get<TwilioOptions>() ?? new TwilioOptions();
            client.BaseAddress = new Uri(twilioOptions.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(twilioOptions.RequestTimeoutSeconds);
        });
        services.AddHttpClient<ITwilioTemplateClient, TwilioHttpTemplateClient>(client => client.BaseAddress = new Uri("https://content.twilio.com/"));
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
