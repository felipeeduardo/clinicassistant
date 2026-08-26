using ClinicAssistant.Application.Identity;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Identity;

public interface IPasswordResetEmailSender
{
    Task SendAsync(string recipient, string resetUrl, CancellationToken cancellationToken);
}

public sealed class PasswordResetEmailSender(IEmailSender emailSender, IOptions<PasswordRecoveryOptions> options) : IPasswordResetEmailSender
{
    private readonly PasswordRecoveryOptions settings = options.Value;

    public async Task SendAsync(string recipient, string resetUrl, CancellationToken cancellationToken)
    {
        var subject = "Redefina sua senha — IA Recepção";
        var text = $"Recebemos uma solicitação para redefinir a senha da sua conta no IA Recepção.\n\nCrie uma nova senha: {resetUrl}\n\nEste link expira em {Math.Clamp(settings.TokenExpirationMinutes, 5, 120)} minutos.\nSe você não solicitou essa alteração, ignore este e-mail.";
        var html = $"<div style=\"font-family:Arial,sans-serif;color:#17233c;max-width:600px;margin:auto\"><h1 style=\"color:#123b73\">Redefina sua senha</h1><p>Recebemos uma solicitação para redefinir a senha da sua conta no IA Recepção.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\" style=\"background:#1769e0;color:#fff;padding:12px 20px;text-decoration:none;border-radius:6px;display:inline-block\">Criar nova senha</a></p><p>Este link expira em {Math.Clamp(settings.TokenExpirationMinutes, 5, 120)} minutos.</p><p style=\"color:#667085;font-size:13px\">Se você não solicitou essa alteração, ignore este e-mail.</p></div>";
        await emailSender.SendAsync(recipient, subject, html, text, cancellationToken);
    }
}
