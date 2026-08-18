using System.Net;
using System.Net.Mail;
using ClinicAssistant.Application.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ClinicAssistant.Infrastructure.Identity;

public interface IPasswordResetEmailSender
{
    Task SendAsync(string recipient, string resetUrl, CancellationToken cancellationToken);
}

public sealed class PasswordResetEmailSender(IOptions<PasswordRecoveryOptions> options) : IPasswordResetEmailSender
{
    private readonly PasswordRecoveryOptions settings = options.Value;

    public async Task SendAsync(string recipient, string resetUrl, CancellationToken cancellationToken)
    {
        if (!settings.Provider.Equals("Smtp", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.From)) throw new InvalidOperationException("PasswordRecovery SMTP is not configured.");
        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort) { EnableSsl = settings.EnableSsl, Credentials = string.IsNullOrWhiteSpace(settings.SmtpUser) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(settings.SmtpUser, settings.SmtpPassword) };
        using var message = new MailMessage(settings.From, recipient, "Redefinição de senha — IA Recepção", $"Use este link para redefinir sua senha (válido por tempo limitado):\n\n{resetUrl}\n\nSe você não solicitou esta alteração, ignore este e-mail.");
        await client.SendMailAsync(message, cancellationToken);
    }
}
