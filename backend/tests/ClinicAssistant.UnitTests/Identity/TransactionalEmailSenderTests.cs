using System.Net;
using System.Net.Http;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Infrastructure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.Identity;

public sealed class TransactionalEmailSenderTests
{
    [Fact]
    public async Task SendGridSendsBearerRequestWithHtmlAndText()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.sendgrid.com/") };
        var sender = new SendGridEmailSender(client,
            Options.Create(new EmailOptions { Enabled = true, Provider = "SendGrid", FromAddress = "no-reply@example.test", FromName = "IA Recepção" }),
            Options.Create(new SendGridOptions { ApiKey = "secret", RequestTimeoutSeconds = 30 }),
            NullLogger<SendGridEmailSender>.Instance);

        await sender.SendAsync("qa@example.test", "Assunto", "<p>html</p>", "texto", CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal("Bearer", handler.Request?.Headers.Authorization?.Scheme);
        Assert.Equal("secret", handler.Request?.Headers.Authorization?.Parameter);
        Assert.Contains("qa@example.test", handler.Body);
        Assert.Contains("html", handler.Body);
        Assert.Contains("texto", handler.Body);
    }

    [Fact]
    public async Task DisabledEmailDoesNotCallProvider()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.sendgrid.com/") };
        var sender = new SendGridEmailSender(client,
            Options.Create(new EmailOptions { Enabled = false, Provider = "SendGrid" }),
            Options.Create(new SendGridOptions { ApiKey = "" }),
            NullLogger<SendGridEmailSender>.Instance);

        await sender.SendAsync("qa@example.test", "Assunto", "html", "texto", CancellationToken.None);

        Assert.Null(handler.Request);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string Body { get; private set; } = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
