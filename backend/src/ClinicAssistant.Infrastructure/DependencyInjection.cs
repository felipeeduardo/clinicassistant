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
using Microsoft.Extensions.Hosting;

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
        var redisConnectionString = configuration["Redis:ConnectionString"]
            ?? configuration["REDIS_URL"]
            ?? configuration["REDIS_PRIVATE_URL"];
        var configuredRedisHost = (configuration["Redis:Host"] ?? configuration["REDISHOST"])?.Trim();
        var hasExplicitRedisHost = !string.IsNullOrWhiteSpace(configuredRedisHost)
            && !string.Equals(configuredRedisHost, "localhost", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(configuredRedisHost, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(configuredRedisHost, "::1", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(redisConnectionString) && !hasExplicitRedisHost)
            throw new InvalidOperationException("Production Redis connection is required. Configure Redis:ConnectionString or Redis:Host/Redis:Port.");
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
                return ConnectionMultiplexer.Connect(ParseRedisConfiguration(redisConnectionString));
            var redisHost = configuration["Redis:Host"] ?? configuration["REDISHOST"] ?? "localhost";
            var redisPort = configuration.GetValue<int?>("Redis:Port")
                ?? configuration.GetValue<int?>("REDISPORT")
                ?? 6379;
            var redisUser = configuration["Redis:User"] ?? configuration["REDISUSER"];
            var redisPassword = configuration["Redis:Password"] ?? configuration["REDISPASSWORD"];
            var redisSsl = configuration.GetValue<bool?>("Redis:Ssl")
                ?? configuration.GetValue<bool?>("REDIS_TLS")
                ?? false;
            return ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { { redisHost, redisPort } },
                AbortOnConnectFail = false,
                User = redisUser,
                Password = redisPassword,
                Ssl = redisSsl
            });
        });
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
        services.AddOptions<WhatsAppOptions>()
            .Bind(configuration.GetSection(WhatsAppOptions.SectionName))
            .Validate(options => !string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase) || options.Provider == WhatsAppProvider.Twilio, "Production WhatsApp provider must be configured as Twilio.")
            .ValidateOnStart();
        services.AddOptions<TwilioOptions>()
            .Bind(configuration.GetSection(TwilioOptions.SectionName))
            .Validate(options =>
            {
                if (!string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)) return true;
                if (string.IsNullOrWhiteSpace(options.AccountSid) || string.IsNullOrWhiteSpace(options.AuthToken) || string.IsNullOrWhiteSpace(options.WhatsAppFrom))
                    throw new InvalidOperationException("Production WhatsApp provider must be configured.");
                if (string.IsNullOrWhiteSpace(options.IncomingWebhookBaseUrl) || string.IsNullOrWhiteSpace(options.StatusCallbackBaseUrl))
                    throw new InvalidOperationException("Production WhatsApp public webhook URLs must be configured.");
                if (!Uri.TryCreate(options.IncomingWebhookBaseUrl, UriKind.Absolute, out var inbound) || inbound.Scheme != Uri.UriSchemeHttps || !Uri.TryCreate(options.StatusCallbackBaseUrl, UriKind.Absolute, out var callback) || callback.Scheme != Uri.UriSchemeHttps)
                    throw new InvalidOperationException("Production WhatsApp public webhook URLs must use HTTPS.");
                return true;
            })
            .ValidateOnStart();
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
        services.AddOptions<HumanQueueOptions>().Bind(configuration.GetSection(HumanQueueOptions.SectionName))
            .Validate(x => x.ReminderMinutes > 0 && x.SlaMinutes > x.ReminderMinutes && x.PollingSeconds is >= 10 and <= 300, "HumanQueue SLA settings are invalid.").ValidateOnStart();
        services.AddSingleton<IConversationIntentResolver, ConversationIntentResolver>();
        services.AddSingleton<IConversationStateMachine, ConversationStateMachine>();
        services.AddSingleton<IConversationResponseComposer, InMemoryConversationResponseComposer>();
        services.AddScoped<IConversationLockManager, RedisConversationLock>();
        services.AddScoped<IConversationOrchestrator, ConversationOrchestrator>();
        services.AddScoped<IConversationAdministrationService, ConversationAdministrationService>();
        services.AddScoped<IOperationalNotificationService, OperationalNotificationService>();
        services.AddHostedService<HumanQueueEscalationHostedService>();
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

    private static ConfigurationOptions ParseRedisConfiguration(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("redis" or "rediss"))
            return ConfigurationOptions.Parse(value);

        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Ssl = uri.Scheme == "rediss"
        };
        options.EndPoints.Add(uri.Host, uri.IsDefaultPort ? 6379 : uri.Port);
        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var credentials = uri.UserInfo.Split(':', 2);
            options.User = Uri.UnescapeDataString(credentials[0]);
            if (credentials.Length == 2)
                options.Password = Uri.UnescapeDataString(credentials[1]);
        }
        if (int.TryParse(uri.AbsolutePath.Trim('/'), out var database))
            options.DefaultDatabase = database;
        return options;
    }
}
