using System.Security.Cryptography;
using System.Text;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Identity;

public sealed class PasswordRecoveryService(ClinicAssistantDbContext db, IPasswordResetEmailSender emailSender, IOptions<PasswordRecoveryOptions> options) : IPasswordRecoveryService
{
    private readonly PasswordRecoveryOptions settings = options.Value;
    public async Task RequestAsync(string email, string? remoteIp, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Email == normalized && x.Status == UserStatus.Active, cancellationToken);
        if (user is null) return;
        var raw = ToBase64Url(RandomNumberGenerator.GetBytes(32));
        var reset = new PasswordResetToken(user.Id, Hash(raw), DateTimeOffset.UtcNow.AddMinutes(Math.Clamp(settings.TokenExpirationMinutes, 5, 120)));
        db.PasswordResetTokens.Add(reset);
        db.AuditRecords.Add(new Domain.Operations.AuditRecord(user.TenantId, null, "auth.password_reset_requested", "User", user.Id, "Succeeded", "Password reset requested."));
        await db.SaveChangesAsync(cancellationToken);
        var url = $"{settings.FrontendBaseUrl.TrimEnd('/')}/redefinir-senha?token={Uri.EscapeDataString(raw)}";
        await emailSender.SendAsync(user.Email, url, cancellationToken);
    }

    public async Task ResetAsync(string token, string newPassword, CancellationToken cancellationToken)
    {
        ValidatePassword(newPassword);
        var reset = await db.PasswordResetTokens.IgnoreQueryFilters().Include(x => x.User).ThenInclude(x => x.RefreshTokens).SingleOrDefaultAsync(x => x.TokenHash == Hash(token), cancellationToken);
        if (reset is null || !reset.IsActive(DateTimeOffset.UtcNow) || reset.User.Status != UserStatus.Active) throw new InvalidOperationException("O link de redefinição é inválido ou expirou.");
        reset.User.SetPasswordHash(PasswordHasher.Hash(newPassword));
        foreach (var refresh in reset.User.RefreshTokens.Where(x => x.IsActive(DateTimeOffset.UtcNow))) refresh.Revoke();
        reset.Consume();
        db.AuditRecords.Add(new Domain.Operations.AuditRecord(reset.User.TenantId, null, "auth.password_reset_completed", "User", reset.UserId, "Succeeded", "Password reset completed."));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static string ToBase64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static void ValidatePassword(string value) { if (value.Length < 12 || !value.Any(char.IsUpper) || !value.Any(char.IsLower) || !value.Any(char.IsDigit) || value.All(char.IsLetterOrDigit)) throw new InvalidOperationException("A senha deve ter pelo menos 12 caracteres, maiúscula, minúscula, número e símbolo."); }
}
