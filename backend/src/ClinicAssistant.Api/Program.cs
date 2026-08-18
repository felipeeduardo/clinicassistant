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
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using System.Net;
using ClinicAssistant.Api.Realtime;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Authorization;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Contracts.Platform;
using ClinicAssistant.Api.Authorization;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Infrastructure.Messaging;
using ClinicAssistant.Application.Leads;
using ClinicAssistant.Contracts.Leads;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    const string RefreshCookieName = "clinic_assistant_refresh";
    static CookieOptions CookieOptions(HttpRequest request) => new()
    {
        HttpOnly = true,
        Secure = request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/api/auth",
        MaxAge = TimeSpan.FromDays(14)
    };
    static void SetRefreshCookie(HttpResponse response, string refreshToken) => response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = response.HttpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/api/auth",
        MaxAge = TimeSpan.FromDays(14)
    });

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
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.HttpContext.Request.Path.StartsWithSegments("/hubs/operations")) context.Token = context.Request.Query["access_token"];
                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("PlatformAdmin", policy => policy.RequireRole("PlatformAdmin"));
        options.AddPolicy("ClinicStaff", policy => policy.RequireRole("ClinicAdmin", "Receptionist", "Professional"));
        options.AddPolicy("ClinicAdmin", policy => policy.RequireRole("ClinicAdmin"));
        options.AddClinicPolicies();
    });
    builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler, OperationalAuthorizationResultHandler>();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IOperationalEventPublisher, SignalROperationalEventPublisher>();
    builder.Services.AddProblemDetails();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var frontendOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("public-demo-lead", context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    });
    builder.Services.AddHealthChecks()
        .AddCheck("postgresql", new PostgreSqlHealthCheck(builder.Configuration), tags: ["ready"])
        .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"])
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
            .AddMeter("ClinicAssistant.Operations")
            .AddOtlpExporter());

    var app = builder.Build();
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ClinicAssistantDbContext>();
        await dbContext.Database.MigrateAsync();
        var bootstrap = scope.ServiceProvider.GetRequiredService<IPlatformBootstrapService>();
        await bootstrap.RunAsync(CancellationToken.None);
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
            SchedulingConflictException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var code = exception switch
        {
            SchedulingConflictException => "scheduling_conflict",
            UnauthorizedAccessException => "unauthorized",
            KeyNotFoundException => "resource_not_found",
            InvalidOperationException => "invalid_operation",
            _ => "unexpected_error"
        };
        await Results.Problem(statusCode: statusCode, title: statusCode == 500 ? "Unexpected error" : exception?.Message,
            extensions: new Dictionary<string, object?> { ["code"] = code, ["traceId"] = traceId }).ExecuteAsync(context);
    }));
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapHub<OperationsHub>("/hubs/operations");
    app.MapGet("/", () => Results.Ok(new { service = "Clinic AI Assistant API", status = "running" }))
        .ExcludeFromDescription();
    var auth = app.MapGroup("/api/auth").WithTags("Authentication");
    auth.MapPost("/register", async (RegisterClinicRequest request, HttpResponse response, IAuthService service, CancellationToken cancellationToken) => { var result = await service.RegisterClinicAsync(request, cancellationToken); SetRefreshCookie(response, result.RefreshToken); return Results.Ok(result); }).AllowAnonymous();
    auth.MapPost("/login", async (LoginRequest request, HttpResponse response, IAuthService service, CancellationToken cancellationToken) => { var result = await service.LoginAsync(request, cancellationToken); SetRefreshCookie(response, result.RefreshToken); return Results.Ok(result); }).AllowAnonymous();
    auth.MapPost("/refresh", async Task<IResult> (HttpRequest request, HttpResponse response, IAuthService service, CancellationToken cancellationToken) =>
    {
        if (!request.Cookies.TryGetValue(RefreshCookieName, out var token) || string.IsNullOrWhiteSpace(token))
            return Results.Unauthorized();

        try
        {
            var result = await service.RefreshAsync(new RefreshRequest(token), cancellationToken);
            SetRefreshCookie(response, result.RefreshToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            response.Cookies.Delete(RefreshCookieName, CookieOptions(request));
            return Results.Unauthorized();
        }
    }).AllowAnonymous();
    auth.MapPost("/logout", async (HttpRequest request, HttpResponse response, IAuthService service, CancellationToken cancellationToken) => { var token = request.Cookies[RefreshCookieName]; if (!string.IsNullOrWhiteSpace(token)) await service.LogoutAsync(new LogoutRequest(token), cancellationToken); response.Cookies.Delete(RefreshCookieName, CookieOptions(request)); return Results.NoContent(); }).AllowAnonymous();
    auth.MapGet("/me", async (IAuthService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.GetCurrentUserAsync(cancellationToken))).RequireAuthorization();
    auth.MapPost("/forgot-password", async (ForgotPasswordRequest request, IPasswordRecoveryService service, HttpContext context, CancellationToken ct) => { await service.RequestAsync(request.Email, context.Connection.RemoteIpAddress?.ToString(), ct); return Results.Accepted(value: new { message = "Se existir uma conta com esse e-mail, enviaremos instruções para redefinir a senha." }); }).AllowAnonymous().RequireRateLimiting("public-demo-lead");
    auth.MapPost("/reset-password", async (ResetPasswordRequest request, IPasswordRecoveryService service, CancellationToken ct) => { await service.ResetAsync(request.Token, request.NewPassword, ct); return Results.NoContent(); }).AllowAnonymous().RequireRateLimiting("public-demo-lead");
    var publicLeads = app.MapGroup("/api/leads").WithTags("Commercial Leads");
    publicLeads.MapPost("/demo-requests", async (CreateDemoLeadRequest request, HttpRequest httpRequest, IDemoLeadService service, CancellationToken ct) =>
    {
        if (httpRequest.ContentLength > 16 * 1024) return Results.BadRequest(new { message = "Payload excede o limite permitido." });
        await service.CreateAsync(request, ct);
        return Results.Accepted(value: new { message = "Recebemos seus dados. Nossa equipe entrará em contato em breve." });
    }).AllowAnonymous().RequireRateLimiting("public-demo-lead");
    var platform = app.MapGroup("/api/platform").RequireAuthorization("PlatformAdmin").WithTags("Platform Administration");
    platform.MapGet("/tenants", async (IPlatformAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetTenantsAsync(ct)));
    platform.MapGet("/users", async (IPlatformAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetUsersAsync(ct)));
    platform.MapGet("/clinics", async (IPlatformAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetClinicsAsync(ct)));
    platform.MapGet("/onboarding/{tenantId:guid}", async (Guid tenantId, IPlatformAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetOnboardingStatusAsync(tenantId, ct)));
    platform.MapPost("/tenants/{tenantId:guid}/clinic-admins", async (Guid tenantId, CreateClinicAdminRequest request, HttpRequest httpRequest, IPlatformAdministrationService service, CancellationToken ct) => Results.Ok(await service.CreateClinicAdminAsync(tenantId, request, httpRequest.Headers["Idempotency-Key"].ToString(), ct)));
    platform.MapPost("/tenants/{id:guid}/{action}", async (Guid id, string action, IPlatformAdministrationService service, CancellationToken ct) => { await service.SetTenantStatusAsync(id, action, ct); return Results.NoContent(); });
    platform.MapPost("/onboarding", async (OnboardTenantRequest request, HttpRequest httpRequest, IPlatformAdministrationService service, CancellationToken ct) => Results.Created("/api/platform/onboarding", await service.OnboardAsync(request, httpRequest.Headers["Idempotency-Key"].ToString(), ct)));
    var platformLeads = platform.MapGroup("/leads");
    platformLeads.MapGet("", async ([AsParameters] DemoLeadListQuery query, IDemoLeadService service, CancellationToken ct) => Results.Ok(await service.SearchAsync(query, ct)));
    platformLeads.MapGet("/summary", async (IDemoLeadService service, CancellationToken ct) => Results.Ok(await service.GetSummaryAsync(ct)));
    platformLeads.MapGet("/{id:guid}", async (Guid id, IDemoLeadService service, CancellationToken ct) => (await service.GetAsync(id, ct)) is { } lead ? Results.Ok(lead) : Results.NotFound());
    platformLeads.MapPost("/{id:guid}/status", async (Guid id, UpdateDemoLeadStatusRequest request, IDemoLeadService service, CancellationToken ct) => { await service.UpdateStatusAsync(id, request.Status, ct); return Results.NoContent(); });
    platformLeads.MapPost("/{id:guid}/assignment", async (Guid id, AssignDemoLeadRequest request, IDemoLeadService service, CancellationToken ct) => { await service.AssignAsync(id, request.UserId, ct); return Results.NoContent(); });
    platformLeads.MapPost("/{id:guid}/notes", async (Guid id, AddDemoLeadNoteRequest request, IDemoLeadService service, CancellationToken ct) => { await service.AddNoteAsync(id, request.Note, ct); return Results.NoContent(); });
    var clinic = app.MapGroup("/api");
    clinic.MapGet("/clinics/current", async (IClinicCatalogService service, CancellationToken ct) => (await service.GetClinicAsync(ct)) is { } item ? Results.Ok(item) : Results.NotFound()).RequireAuthorization(ClinicPolicies.ClinicsView);
    clinic.MapPut("/clinics/current", async (ClinicRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateClinicAsync(request, ct))).RequireAuthorization(ClinicPolicies.ClinicsManage);
    clinic.MapGet("/units", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetUnitsAsync(ct))).RequireAuthorization(ClinicPolicies.UnitsView);
    clinic.MapGet("/units/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => (await service.GetUnitAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound()).RequireAuthorization(ClinicPolicies.UnitsView);
    clinic.MapGet("/units/{id:guid}/details", async (Guid id, IClinicCatalogService service, CancellationToken ct) => (await service.GetUnitDetailAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound()).RequireAuthorization(ClinicPolicies.UnitsView);
    clinic.MapPost("/units", async (UnitRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/units", await service.CreateUnitAsync(request, ct))).RequireAuthorization(ClinicPolicies.UnitsManage);
    clinic.MapPut("/units/{id:guid}", async (Guid id, UnitRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateUnitAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.UnitsManage);
    clinic.MapDelete("/units/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteUnitAsync(id, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.UnitsManage);
    clinic.MapPost("/units/{id:guid}/status/{status}", async (Guid id, string status, IClinicCatalogService service, CancellationToken ct) => { await service.SetUnitStatusAsync(id, status, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.UnitsManage);
    clinic.MapPut("/units/{id:guid}/business-hours", async (Guid id, IReadOnlyList<UnitBusinessHourRequest> request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.ReplaceUnitBusinessHoursAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.UnitsManage);
    clinic.MapGet("/specialties", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetSpecialtiesAsync(ct))).RequireAuthorization(ClinicPolicies.SpecialtiesView);
    clinic.MapPost("/specialties", async (SpecialtyRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/specialties", await service.CreateSpecialtyAsync(request, ct))).RequireAuthorization(ClinicPolicies.SpecialtiesManage);
    clinic.MapPut("/specialties/{id:guid}", async (Guid id, SpecialtyRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateSpecialtyAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.SpecialtiesManage);
    clinic.MapDelete("/specialties/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteSpecialtyAsync(id, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.SpecialtiesManage);
    clinic.MapGet("/specialties/{id:guid}/dependencies", async (Guid id, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetSpecialtyDependenciesAsync(id, ct))).RequireAuthorization(ClinicPolicies.SpecialtiesView);
    clinic.MapPost("/specialties/{id:guid}/status/{status}", async (Guid id, string status, IClinicCatalogService service, CancellationToken ct) => { await service.SetSpecialtyStatusAsync(id, status, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.SpecialtiesManage);
    clinic.MapGet("/professionals", async (IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.GetProfessionalsAsync(ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapGet("/professionals/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => (await service.GetProfessionalAsync(id, ct) is { } item ? Results.Ok(item) : Results.NotFound())).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapPost("/professionals", async (ProfessionalRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Created("/api/professionals", await service.CreateProfessionalAsync(request, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapPut("/professionals/{id:guid}", async (Guid id, ProfessionalRequest request, IClinicCatalogService service, CancellationToken ct) => Results.Ok(await service.UpdateProfessionalAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapDelete("/professionals/{id:guid}", async (Guid id, IClinicCatalogService service, CancellationToken ct) => { await service.DeleteProfessionalAsync(id, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapGet("/patients", async (ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetPatientsAsync(ct))).RequireAuthorization(ClinicPolicies.PatientsView);
    clinic.MapGet("/patients/search", async ([AsParameters] PatientSearchRequest request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.SearchPatientsAsync(request, ct))).RequireAuthorization(ClinicPolicies.PatientsView);
    clinic.MapGet("/patients/{id:guid}", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetPatientDetailAsync(id, ct))).RequireAuthorization(ClinicPolicies.PatientsView);
    clinic.MapPost("/patients", async (PatientRequest request, ISchedulingService service, CancellationToken ct) => Results.Created("/api/patients", await service.CreatePatientAsync(request, ct))).RequireAuthorization(ClinicPolicies.PatientsManage);
    clinic.MapPut("/patients/{id:guid}", async (Guid id, PatientRequest request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.UpdatePatientAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.PatientsManage);
    clinic.MapGet("/professionals/{id:guid}/availability", async (Guid id, DateOnly date, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetAvailabilityAsync(id, date, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapPost("/professionals/{id:guid}/availability", async (Guid id, AvailabilityRuleRequest request, ISchedulingService service, CancellationToken ct) => { await service.AddAvailabilityRuleAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapGet("/professionals/{id:guid}/availability/rules", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetAvailabilityRulesAsync(id, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapPut("/professionals/{id:guid}/availability/rules", async (Guid id, IReadOnlyList<AvailabilityRuleRequest> request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.ReplaceAvailabilityRulesAsync(id, request, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapPost("/professionals/{id:guid}/blocks", async (Guid id, ScheduleBlockRequest request, ISchedulingService service, CancellationToken ct) => { await service.AddScheduleBlockAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapGet("/professionals/{id:guid}/blocks", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetScheduleBlocksAsync(id, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapDelete("/professionals/{id:guid}/blocks/{blockId:guid}", async (Guid id, Guid blockId, ISchedulingService service, CancellationToken ct) => { await service.DeleteScheduleBlockAsync(id, blockId, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapGet("/professionals/{id:guid}/vacations", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetVacationsAsync(id, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapPost("/professionals/{id:guid}/vacations", async (Guid id, VacationRequest request, ISchedulingService service, CancellationToken ct) => { await service.AddVacationAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapDelete("/professionals/{id:guid}/vacations/{vacationId:guid}", async (Guid id, Guid vacationId, ISchedulingService service, CancellationToken ct) => { await service.DeleteVacationAsync(id, vacationId, ct); return Results.NoContent(); }).RequireAuthorization(ClinicPolicies.ProfessionalsManage);
    clinic.MapGet("/professionals/{id:guid}/schedule", async (Guid id, DateTimeOffset startsAt, DateTimeOffset endsAt, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetProfessionalScheduleAsync(id, startsAt, endsAt, ct))).RequireAuthorization(ClinicPolicies.ProfessionalsView);
    clinic.MapGet("/appointments", async (DateTimeOffset startsAt, DateTimeOffset endsAt, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetAppointmentsAsync(startsAt, endsAt, ct))).RequireAuthorization("ClinicStaff");
    clinic.MapGet("/appointments/search", async ([AsParameters] AppointmentSearchRequest request, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.SearchAppointmentsAsync(request, ct))).RequireAuthorization("ClinicStaff");
    clinic.MapGet("/appointments/{id:guid}", async (Guid id, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.GetAppointmentDetailAsync(id, ct))).RequireAuthorization("ClinicStaff");
    clinic.MapPost("/appointments", async (AppointmentRequest request, HttpRequest httpRequest, ISchedulingService service, CancellationToken ct) => Results.Created("/api/appointments", await service.CreateAppointmentAsync(request, httpRequest.Headers["Idempotency-Key"].ToString(), ct))).RequireAuthorization("ClinicStaff");
    clinic.MapPost("/appointments/{id:guid}/confirm", async (Guid id, AppointmentOperationRequest request, HttpRequest httpRequest, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.ConfirmAsync(id, request, httpRequest.Headers["Idempotency-Key"].ToString(), ct))).RequireAuthorization("ClinicStaff");
    clinic.MapPost("/appointments/{id:guid}/cancel", async (Guid id, CancelAppointmentRequest request, HttpRequest httpRequest, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.CancelAsync(id, request, httpRequest.Headers["Idempotency-Key"].ToString(), ct))).RequireAuthorization("ClinicStaff");
    clinic.MapPost("/appointments/{id:guid}/reschedule", async (Guid id, RescheduleAppointmentRequest request, HttpRequest httpRequest, ISchedulingService service, CancellationToken ct) => Results.Ok(await service.RescheduleAsync(id, request, httpRequest.Headers["Idempotency-Key"].ToString(), ct))).RequireAuthorization("ClinicStaff");
    clinic.MapGet("/whatsapp/integration/status", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) =>
        (await service.GetCurrentAsync(ct)) is { } status ? Results.Ok(status) : Results.NotFound()).RequireAuthorization("ClinicStaff");
    clinic.MapGet("/whatsapp/integration/twilio/configuration", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) => Results.Ok(await service.GetTwilioConfigurationAsync(ct))).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/integration/validate", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) => { await service.ValidateCurrentAsync(ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/integration/enable", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) => { await service.EnableCurrentAsync(ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/integration/disable", async (IWhatsAppIntegrationStatusService service, CancellationToken ct) => { await service.DisableCurrentAsync(ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/integration/test-message", async (HttpRequest request, IWhatsAppIntegrationStatusService service, CancellationToken ct) => { await service.QueueTestMessageAsync(request.Headers["Idempotency-Key"].ToString(), ct); return Results.Accepted(); }).RequireAuthorization("ClinicAdmin");
    clinic.MapGet("/whatsapp/templates", async ([AsParameters] WhatsAppTemplateQuery query, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => Results.Ok(await service.SearchAsync(query, ct))).RequireAuthorization("ClinicAdmin");
    clinic.MapGet("/whatsapp/templates/{templateId:guid}", async (Guid templateId, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => (await service.GetAsync(templateId, ct)) is { } template ? Results.Ok(template) : Results.NotFound()).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/templates", async (WhatsAppTemplateRequest request, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => Results.Created("/api/whatsapp/templates", await service.CreateAsync(request, ct))).RequireAuthorization("ClinicAdmin");
    clinic.MapPut("/whatsapp/templates/{templateId:guid}", async (Guid templateId, WhatsAppTemplateRequest request, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => (await service.UpdateAsync(templateId, request, ct)) is { } template ? Results.Ok(template) : Results.NotFound()).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/templates/{templateId:guid}/activate", async (Guid templateId, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => await service.ActivateAsync(templateId, ct) ? Results.NoContent() : Results.NotFound()).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/templates/{templateId:guid}/deactivate", async (Guid templateId, IWhatsAppTemplateAdministrationService service, CancellationToken ct) => await service.DeactivateAsync(templateId, ct) ? Results.NoContent() : Results.NotFound()).RequireAuthorization("ClinicAdmin");
    clinic.MapPost("/whatsapp/templates/sync", async (IWhatsAppTemplateAdministrationService service, CancellationToken ct) => { await service.QueueSyncAsync(ct); return Results.Accepted(); }).RequireAuthorization("ClinicAdmin");
    clinic.MapGet("/audit", async ([AsParameters] AuditQuery query, IAuditQueryService service, CancellationToken ct) => Results.Ok(await service.SearchAsync(query, ct))).RequireAuthorization("ClinicAdmin");
    clinic.MapGet("/dashboard", async (DateTimeOffset? from, DateTimeOffset? to, IDashboardService service, CancellationToken ct) => Results.Ok(await service.GetAsync(from, to, ct))).RequireAuthorization("ClinicStaff");
    var conversations = app.MapGroup("/api/conversations").RequireAuthorization("ClinicStaff").WithTags("Conversations");
    conversations.MapGet("/", async ([AsParameters] ConversationListQuery query, IConversationAdministrationService service, CancellationToken ct) => Results.Ok(await service.ListAsync(query, ct)));
    conversations.MapGet("/{id:guid}", async (Guid id, IConversationAdministrationService service, CancellationToken ct) => (await service.GetAsync(id, ct)) is { } item ? Results.Ok(item) : Results.NotFound());
    conversations.MapGet("/{id:guid}/messages", async (Guid id, int page, int pageSize, IConversationAdministrationService service, CancellationToken ct) => (await service.GetMessagesAsync(id, page, pageSize, ct)) is { } items ? Results.Ok(items) : Results.NotFound());
    conversations.MapGet("/{id:guid}/appointments", async (Guid id, IConversationAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetAppointmentsAsync(id, ct)));
    conversations.MapGet("/operators", async (IConversationAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetAssignableUsersAsync(ct))).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/messages/{messageId:guid}/read", async (Guid id, Guid messageId, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.MarkReadAsync(id, messageId, request.ExpectedVersion, ct); return Results.NoContent(); });
    conversations.MapPost("/{id:guid}/assign", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.AssignAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/release", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.ReleaseAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/transfer", async (Guid id, ConversationTransferRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.TransferAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/automation/pause", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.PauseAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/automation/resume", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.ResumeAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/close", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.CloseAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/reopen", async (Guid id, ConversationOperationRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.ReopenAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPatch("/{id:guid}/priority", async (Guid id, ConversationPriorityRequest request, IConversationAdministrationService service, CancellationToken ct) => { await service.SetPriorityAsync(id, request, ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    conversations.MapPost("/{id:guid}/messages", async (Guid id, ManualConversationMessageRequest request, HttpRequest httpRequest, IConversationAdministrationService service, CancellationToken ct) => { await service.SendManualMessageAsync(id, request, httpRequest.Headers["Idempotency-Key"].ToString(), ct); return Results.NoContent(); }).RequireAuthorization("ClinicAdmin");
    app.MapGet("/api/conversation-queue", async ([AsParameters] HumanQueueListQuery query, IConversationAdministrationService service, CancellationToken ct) => Results.Ok(await service.GetHumanQueueAsync(query, ct))).RequireAuthorization("ClinicAdmin");
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
