using ClinicAssistant.Api.Health;
using ClinicAssistant.Application;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Contracts.Identity;
using ClinicAssistant.Contracts.Clinics;
using ClinicAssistant.Application.Clinics;
using ClinicAssistant.Application.Scheduling;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Infrastructure;
using ClinicAssistant.Infrastructure.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Globalization;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using System.Net;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ClinicAssistant.Api")
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt configuration is required.");
    if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
        throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("PlatformAdmin", policy => policy.RequireRole("PlatformAdmin"));
        options.AddPolicy("ClinicStaff", policy => policy.RequireRole("ClinicAdmin", "Receptionist", "Professional"));
        options.AddPolicy("ClinicAdmin", policy => policy.RequireRole("ClinicAdmin"));
    });
    builder.Services.AddProblemDetails();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var frontendOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));
    builder.Services.AddHealthChecks()
        .AddCheck("postgresql", new PostgreSqlHealthCheck(builder.Configuration), tags: ["ready"])
        .AddCheck("rabbitmq", new TcpHealthCheck(builder.Configuration, "RabbitMq", 5672), tags: ["ready"])
        .AddCheck("redis", new TcpHealthCheck(builder.Configuration, "Redis", 6379), tags: ["ready"]);

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("ClinicAssistant.Api"))
        .WithTracing(tracing => tracing
            .AddSource("ClinicAssistant.WhatsApp")
            .AddSource("ClinicAssistant.Conversations")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddMeter("ClinicAssistant.WhatsApp")
            .AddMeter("ClinicAssistant.Conversations")
            .AddOtlpExporter());

    var app = builder.Build();
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ClinicAssistantDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    var twilioOptions = app.Services.GetRequiredService<IOptions<TwilioOptions>>().Value;
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
    };
    foreach (var proxyAddress in twilioOptions.TrustedProxyAddresses.Where(address => !string.IsNullOrWhiteSpace(address))) forwardedHeadersOptions.KnownProxies.Add(IPAddress.Parse(proxyAddress));
    app.UseForwardedHeaders(forwardedHeadersOptions);
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        await Results.Problem(statusCode: statusCode, title: statusCode == 500 ? "Unexpected error" : exception?.Message).ExecuteAsync(context);
    }));
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Ok(new { service = "Clinic AI Assistant API", status = "running" }))
        .ExcludeFromDescription();
    var auth = app.MapGroup("/api/auth").WithTags("Authentication");
    auth.MapPost("/register", async (RegisterClinicRequest request, IAuthService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.RegisterClinicAsync(request, cancellationToken))).AllowAnonymous();
    auth.MapPost("/login", async (LoginRequest request, IAuthService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.LoginAsync(request, cancellationToken))).AllowAnonymous();
    auth.MapPost("/refresh", async (RefreshRequest request, IAuthService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.RefreshAsync(request, cancellationToken))).AllowAnonymous();
    auth.MapPost("/logout", async (LogoutRequest request, IAuthService service, CancellationToken cancellationToken) =>
    {
        await service.LogoutAsync(request, cancellationToken);
        return Results.NoContent();
    }).AllowAnonymous();
    auth.MapGet("/me", async (IAuthService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.GetCurrentUserAsync(cancellationToken))).RequireAuthorization();
    var clinic = app.MapGroup("/api").RequireAuthorization("ClinicStaff");
    clinic.MapGet("/clinics/current", async (IClinicCatalogService service, CancellationToken ct) => (await service.GetClinicAsync(ct)) is { } item ? Results.Ok(item) : Results.NotFound());
    clinic.MapPut("/clinics/current", async (ClinicRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateClinicAsync(request, ct)));
    clinic.MapGet("/units", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetUnitsAsync(ct)));
    clinic.MapGet("/units/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => (await service.GetUnitAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound());
    clinic.MapPost("/units", async (UnitRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/units", await service.CreateUnitAsync(request, ct)));
    clinic.MapPut("/units/{id:guid}", async (Guid id, UnitRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateUnitAsync(id, request, ct)));
    clinic.MapDelete("/units/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteUnitAsync(id, ct); return Results.NoContent(); });
    clinic.MapGet("/specialties", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetSpecialtiesAsync(ct)));
    clinic.MapPost("/specialties", async (SpecialtyRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/specialties", await service.CreateSpecialtyAsync(request, ct)));
    clinic.MapPut("/specialties/{id:guid}", async (Guid id, SpecialtyRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateSpecialtyAsync(id, request, ct)));
    clinic.MapDelete("/specialties/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteSpecialtyAsync(id, ct); return Results.NoContent(); });
    clinic.MapGet("/professionals", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetProfessionalsAsync(ct)));
    clinic.MapGet("/professionals/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => (await service.GetProfessionalAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound());
    clinic.MapPost("/professionals", async (ProfessionalRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/professionals", await service.CreateProfessionalAsync(request, ct)));
    clinic.MapPut("/professionals/{id:guid}", async (Guid id, ProfessionalRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateProfessionalAsync(id, request, ct)));
    clinic.MapDelete("/professionals/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteProfessionalAsync(id, ct); return Results.NoContent(); });
    clinic.MapGet("/patients", async (ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetPatientsAsync(ct)));
    clinic.MapPost("/patients", async (PatientRequest request, ISchedulingService service, CancellationToken ct) => Results.Created("/api/patients", await service.CreatePatientAsync(request, ct)));
    clinic.MapPut("/patients/{id:guid}", async (Guid id, PatientRequest request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.UpdatePatientAsync(id, request, ct)));
    clinic.MapGet("/professionals/{id:guid}/availability", async (Guid id, DateOnly date, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetAvailabilityAsync(id, date, ct)));
    clinic.MapPost("/professionals/{id:guid}/availability", async (Guid id, AvailabilityRuleRequest request, ISchedulingService service, CancellationToken ct) => { await service.AddAvailabilityRuleAsync(id, request, ct); return Results.NoContent(); });
    clinic.MapPost("/professionals/{id:guid}/blocks", async (Guid id, ScheduleBlockRequest request, ISchedulingService service, CancellationToken ct) => { await service.AddScheduleBlockAsync(id, request, ct); return Results.NoContent(); });
    clinic.MapPost("/appointments", async (AppointmentRequest request, ISchedulingService service, CancellationToken ct) => Results.Created("/api/appointments", await service.CreateAppointmentAsync(request, ct)));
    clinic.MapPost("/appointments/{id:guid}/confirm", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.ConfirmAsync(id, ct)));
    clinic.MapPost("/appointments/{id:guid}/cancel", async (Guid id, CancelAppointmentRequest request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.CancelAsync(id, request, ct)));
    clinic.MapGet("/whatsapp/integration/status", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) =>
        (await service.GetCurrentAsync(ct)) is { } status ? Results.Ok(status) : Results.NotFound());
    var conversations = app.MapGroup("/api/conversations").RequireAuthorization("ClinicStaff").WithTags("Conversations");
    conversations.MapGet("/", async ([AsParameters] ConversationListQuery query, IConversationAdministrationService service, CancellationToken ct) => Results.Ok(await service.ListAsync(query, ct)));
    conversations.MapGet("/{id:guid}", async (Guid id, IConversationAdministrationService service, CancellationToken ct) => (await service.GetAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound());
    conversations.MapGet("/{id:guid}/messages", async (Guid id, int page, int pageSize, IConversationAdministrationService service, CancellationToken ct) => (await service.GetMessagesAsync(id, page, pageSize, ct)) is { } items ? Results.Ok(items) : Results.NotFound());
    conversations.MapPost("/{id:guid}/messages/{messageId:guid}/read", async (Guid id, Guid messageId, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.MarkReadAsync(id, messageId, request.ExpectedVersion, ct); return Results.NoContent(); });
    conversations.MapPost("/{id:guid}/assign", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.AssignAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/release", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.ReleaseAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/automation/pause", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.PauseAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/automation/resume", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.ResumeAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    app.MapPost("/api/webhooks/whatsapp/twilio/{integrationKey}", async (string integrationKey, HttpRequest request, IWhatsAppIncomingWebhookService service, TwilioWebhookUrlResolver urlResolver, IOptions<WhatsAppOptions> options, CancellationToken cancellationToken) =>
    {
        var stopwatch = Stopwatch.StartNew();
        if (!request.HasFormContentType || request.ContentLength > options.Value.MaxWebhookBodySizeBytes)
        {
            WhatsAppTelemetry.RecordWebhook("incoming", stopwatch.Elapsed);
            return Results.BadRequest();
        }
        var maxBodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxBodySizeFeature is { IsReadOnly: false }) maxBodySizeFeature.MaxRequestBodySize = options.Value.MaxWebhookBodySizeBytes;
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;
        var form = await request.ReadFormAsync(cancellationToken);
        var parameters = form.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        var correlationId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var result = await service.ProcessAsync(new(integrationKey, urlResolver.Resolve(request), parameters, request.Headers["X-Twilio-Signature"].ToString(), rawPayload, correlationId), cancellationToken);
        WhatsAppTelemetry.RecordWebhook("incoming", stopwatch.Elapsed);
        return result.Status switch
        {
            WhatsAppIncomingWebhookStatus.Accepted or WhatsAppIncomingWebhookStatus.Duplicate => Results.Ok(),
            WhatsAppIncomingWebhookStatus.InvalidSignature => Results.Unauthorized(),
            WhatsAppIncomingWebhookStatus.InvalidPayload => Results.BadRequest(),
            _ => Results.NotFound()
        };
    }).AllowAnonymous().WithTags("WhatsApp");
    app.MapPost("/api/webhooks/whatsapp/twilio/status/{integrationKey}", async (string integrationKey, HttpRequest request, IWhatsAppStatusCallbackService service, TwilioWebhookUrlResolver urlResolver, IOptions<WhatsAppOptions> options, CancellationToken cancellationToken) =>
    {
        var stopwatch = Stopwatch.StartNew();
        if (!request.HasFormContentType || request.ContentLength > options.Value.MaxWebhookBodySizeBytes)
        {
            WhatsAppTelemetry.RecordWebhook("status", stopwatch.Elapsed);
            return Results.BadRequest();
        }
        var maxBodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (maxBodySizeFeature is { IsReadOnly: false }) maxBodySizeFeature.MaxRequestBodySize = options.Value.MaxWebhookBodySizeBytes;
        var form = await request.ReadFormAsync(cancellationToken);
        var parameters = form.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        var result = await service.ProcessAsync(new(integrationKey, urlResolver.ResolveStatusCallback(request), parameters, request.Headers["X-Twilio-Signature"].ToString()), cancellationToken);
        WhatsAppTelemetry.RecordWebhook("status", stopwatch.Elapsed);
        return result.Status switch
        {
            WhatsAppStatusCallbackResultStatus.Updated or WhatsAppStatusCallbackResultStatus.Unchanged => Results.Ok(),
            WhatsAppStatusCallbackResultStatus.InvalidSignature => Results.Unauthorized(),
            WhatsAppStatusCallbackResultStatus.InvalidPayload => Results.BadRequest(),
            _ => Results.NotFound()
        };
    }).AllowAnonymous().WithTags("WhatsApp");
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Clinic Assistant API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
