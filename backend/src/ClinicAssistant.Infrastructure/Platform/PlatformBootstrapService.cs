using System.Net.Mail;
using ClinicAssistant.Application.Platform;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Infrastructure.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Platform;

public sealed class PlatformBootstrapService(
    ClinicAssistantDbContext db,
    IOptions<PlatformBootstrapOptions> options,
    ILogger<PlatformBootstrapService> logger) : IPlatformBootstrapService
{
    private const string PlatformTenantSlug = "platform-system";

    public async Task<PlatformBootstrapResult> RunAsync(CancellationToken cancellationToken)
    {
        var configuration = options.Value;
        if (!configuration.Enabled) return new PlatformBootstrapResult(false, 0, 0);

        Validate(configuration);
        PlatformBootstrapLog.Started(logger, configuration.Admins.Count);
        db.AuditRecords.Add(new AuditRecord(null, null, "PlatformBootstrapStarted", "PlatformBootstrap", null, "Started", "Platform bootstrap started."));
        await db.SaveChangesAsync(cancellationToken);
        var created = 0;
        var existing = 0;

        foreach (var configuredAdmin in configuration.Admins)
        {
            var email = NormalizeEmail(configuredAdmin.Email);
            var user = await db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
            if (user is not null)
            {
                if (user.Role != UserRole.PlatformAdmin)
                {
                    PlatformBootstrapLog.IncompatibleRole(logger, email, user.Role);
                    throw new InvalidOperationException("A configured platform administrator email is already assigned to another role. Resolve it administratively before enabling bootstrap.");
                }

                existing++;
                await WriteAuditAsync("PlatformAdminAlreadyExists", user.Id, email, cancellationToken);
                continue;
            }

            var tenant = await EnsurePlatformTenantAsync(cancellationToken);
            var createdUser = new User(tenant.Id, "Platform Administrator", email, PasswordHasher.Hash(configuredAdmin.Password), UserRole.PlatformAdmin);
            db.Users.Add(createdUser);
            db.AuditRecords.Add(new AuditRecord(null, createdUser.Id, "PlatformAdminCreated", "User", createdUser.Id, "Succeeded", $"Platform administrator created for {email}."));
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                created++;
                PlatformBootstrapLog.Created(logger, email);
            }
            catch (DbUpdateException)
            {
                db.Entry(createdUser).State = EntityState.Detached;
                var concurrentUser = await db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
                if (concurrentUser?.Role != UserRole.PlatformAdmin)
                    throw new InvalidOperationException("A configured platform administrator email could not be created safely because it is already assigned to another role.");
                existing++;
                PlatformBootstrapLog.Concurrent(logger, email);
            }
        }

        PlatformBootstrapLog.Completed(logger, created, existing);
        db.AuditRecords.Add(new AuditRecord(null, null, "PlatformBootstrapCompleted", "PlatformBootstrap", null, "Succeeded", $"Created {created}; existing {existing}."));
        await db.SaveChangesAsync(cancellationToken);
        return new PlatformBootstrapResult(true, created, existing);
    }

    private async Task<Tenant> EnsurePlatformTenantAsync(CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.IgnoreQueryFilters().SingleOrDefaultAsync(candidate => candidate.Slug == PlatformTenantSlug, cancellationToken);
        if (tenant is not null) return tenant;
        tenant = new Tenant("IA Recepção Platform", PlatformTenantSlug);
        db.Tenants.Add(tenant);
        return tenant;
    }

    private async Task WriteAuditAsync(string action, Guid userId, string email, CancellationToken cancellationToken)
    {
        db.AuditRecords.Add(new AuditRecord(null, userId, action, "User", userId, "Succeeded", $"Platform bootstrap checked {email}."));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(PlatformBootstrapOptions configuration)
    {
        if (configuration.Admins.Count is < 1 or > 2) throw new InvalidOperationException("PlatformBootstrap requires one or two administrators when enabled.");
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var admin in configuration.Admins)
        {
            var email = NormalizeEmail(admin.Email);
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("PlatformBootstrap administrator emails are required. Configure PlatformBootstrap__Admins__0__Email (and __1__Email when applicable).");
            try { _ = new MailAddress(email); }
            catch (FormatException) { throw new InvalidOperationException("PlatformBootstrap contains an invalid administrator email."); }
            if (!emails.Add(email)) throw new InvalidOperationException("PlatformBootstrap administrator emails must be unique.");
            if (string.IsNullOrWhiteSpace(admin.Password)) throw new InvalidOperationException("PlatformBootstrap administrator passwords are required.");
            if (admin.Password.Length < 12 || !admin.Password.Any(char.IsUpper) || !admin.Password.Any(char.IsLower) || !admin.Password.Any(char.IsDigit) || !admin.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                throw new InvalidOperationException("PlatformBootstrap administrator passwords must contain at least 12 characters, upper/lower case, a number and a symbol.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

internal static partial class PlatformBootstrapLog
{
    [LoggerMessage(LogLevel.Information, "Platform bootstrap started for {adminCount} configured administrator(s).")]
    public static partial void Started(ILogger logger, int adminCount);

    [LoggerMessage(LogLevel.Error, "Platform bootstrap stopped: configured administrator email {email} already belongs to role {role}.")]
    public static partial void IncompatibleRole(ILogger logger, string email, UserRole role);

    [LoggerMessage(LogLevel.Information, "Platform administrator {email} created.")]
    public static partial void Created(ILogger logger, string email);

    [LoggerMessage(LogLevel.Information, "Platform administrator {email} was created concurrently; treating bootstrap as idempotent.")]
    public static partial void Concurrent(ILogger logger, string email);

    [LoggerMessage(LogLevel.Information, "Platform bootstrap completed. Created {createdCount}; already existing {existingCount}.")]
    public static partial void Completed(ILogger logger, int createdCount, int existingCount);
}
