using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class TwilioWhatsAppGatewayTests
{
    [Fact]
    public async Task SendTextFormatsPhoneAndReturnsTwilioMessageSid()
    {
        var client = new RecordingTwilioMessageClient(new(true, "SM123", "queued", null));
        var gateway = new TwilioWhatsAppGateway(client, new WhatsAppPhoneNumberFormatter(), Options.Create(new TwilioOptions { WhatsAppFrom = "+5581888888888" }));

        var result = await gateway.SendTextAsync(CreateRequest(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("SM123", result.ExternalMessageId);
        Assert.Equal("whatsapp:+5581999999999", client.LastTextRequest?.To);
        Assert.Equal("whatsapp:+5581888888888", client.LastTextRequest?.From);
    }

    [Fact]
    public async Task SendTextClassifiesRateLimitAsRetryable()
    {
        var client = new RecordingTwilioMessageClient(new(false, null, "failed", new("20429", "Twilio rejected the message.", 429)));
        var gateway = new TwilioWhatsAppGateway(client, new WhatsAppPhoneNumberFormatter(), Options.Create(new TwilioOptions { WhatsAppFrom = "+5581888888888" }));

        var result = await gateway.SendTextAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(WhatsAppFailureType.RateLimit, result.Failure?.Type);
        Assert.True(result.Failure?.CanRetry);
    }

    private static SendWhatsAppTextRequest CreateRequest() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999", "Olá", "outbox-1", null);

    private sealed class RecordingTwilioMessageClient(TwilioMessageResult result) : ITwilioMessageClient
    {
        public TwilioSendTextRequest? LastTextRequest { get; private set; }
        public Task<TwilioMessageResult> SendTextAsync(TwilioSendTextRequest request, CancellationToken cancellationToken) { LastTextRequest = request; return Task.FromResult(result); }
        public Task<TwilioMessageResult> SendTemplateAsync(TwilioSendTemplateRequest request, CancellationToken cancellationToken) => Task.FromResult(result);
        public Task<TwilioMessageResult> SendMediaAsync(TwilioSendMediaRequest request, CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
