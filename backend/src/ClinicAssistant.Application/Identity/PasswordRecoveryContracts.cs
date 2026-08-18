namespace ClinicAssistant.Application.Identity;

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
public sealed record PasswordRecoveryOptions
{
    public const string SectionName = "PasswordRecovery";
    public int TokenExpirationMinutes { get; set; } = 30;
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
    public string Provider { get; set; } = "Disabled";
    public string From { get; set; } = "";
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public bool EnableSsl { get; set; } = true;
}

public interface IPasswordRecoveryService
{
    Task RequestAsync(string email, string? remoteIp, CancellationToken cancellationToken);
    Task ResetAsync(string token, string newPassword, CancellationToken cancellationToken);
}
