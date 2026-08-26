using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using ClinicAssistant.Application.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Identity;

public sealed class DisabledEmailSender : IEmailSender
{
    public Task SendAsync(string recipient, string subject, string htmlBody, string textBody, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class FakeEmailSender : IEmailSender
{
    private readonly List<FakeEmail> sent = [];
    public IReadOnlyList<FakeEmail> Sent => sent;
    public Task SendAsync(string recipient, string subject, string htmlBody, string textBody, CancellationToken cancellationToken)
    {
        sent.Add(new FakeEmail(recipient, subject, "PasswordReset", DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }
}

public sealed record FakeEmail(string To, string Subject, string EmailType, DateTimeOffset Timestamp);

public sealed class SendGridEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> emailOptions,
    IOptions<SendGridOptions> sendGridOptions,
    ILogger<SendGridEmailSender> logger) : IEmailSender
{
    private static readonly Action<ILogger, string, int, Exception?> SendFailed = LoggerMessage.Define<string, int>(LogLevel.Error, new EventId(6101, "EmailSendFailed"), "Transactional email provider failed. RecipientDomain={RecipientDomain} Status={Status}");
    private static readonly Action<ILogger, string, int, Exception?> SendSucceeded = LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(6102, "EmailSent"), "Transactional email sent. RecipientDomain={RecipientDomain} Status={Status}");
    private readonly EmailOptions email = emailOptions.Value;
    private readonly SendGridOptions sendGrid = sendGridOptions.Value;

    public async Task SendAsync(string recipient, string subject, string htmlBody, string textBody, CancellationToken cancellationToken)
    {
        if (!email.Enabled || !email.Provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase)) return;
        if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Recipient and subject are required.");
        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = recipient } } } },
            from = new { email = email.FromAddress, name = email.FromName },
            subject,
            content = new[] { new { type = "text/plain", value = textBody }, new { type = "text/html", value = htmlBody } }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/mail/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sendGrid.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (logger.IsEnabled(LogLevel.Error)) SendFailed(logger, Domain(recipient), (int)response.StatusCode, null);
            throw new InvalidOperationException("Transactional email provider failed.");
        }
        if (logger.IsEnabled(LogLevel.Information)) SendSucceeded(logger, Domain(recipient), (int)response.StatusCode, null);
    }

    private static string Domain(string address)
    {
        try { return new MailAddress(address).Host; }
        catch (FormatException) { return "invalid"; }
    }
}
