using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class FakeWhatsAppGatewayTests
{
    [Fact]
    public async Task SendTextReturnsAStableFakeMessageIdentifierForTheSameIdempotencyKey()
    {
        var gateway = CreateGateway();
        var request = CreateTextRequest("outbox-1");

        var first = await gateway.SendTextAsync(request, CancellationToken.None);
        var second = await gateway.SendTextAsync(request, CancellationToken.None);

        Assert.True(first.Success);
        Assert.StartsWith("SM_FAKE_", first.ExternalMessageId);
        Assert.Equal(first.ExternalMessageId, second.ExternalMessageId);
    }

    [Fact]
    public async Task SendTemplateReturnsARetryableFailureWhenTransientFailureIsConfigured()
    {
        var gateway = CreateGateway(FakeWhatsAppFailureMode.Transient);
        var request = new SendWhatsAppTemplateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999", "HX123", new Dictionary<string, string>(), "outbox-2", null);

        var result = await gateway.SendTemplateAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(WhatsAppFailureType.Transient, result.Failure?.Type);
        Assert.True(result.Failure?.CanRetry);
    }

    private static FakeWhatsAppGateway CreateGateway(FakeWhatsAppFailureMode failureMode = FakeWhatsAppFailureMode.None) =>
        new(Options.Create(new WhatsAppOptions { Fake = new FakeWhatsAppOptions { DelayMilliseconds = 0, FailureMode = failureMode } }), NullLogger<FakeWhatsAppGateway>.Instance);

    private static SendWhatsAppTextRequest CreateTextRequest(string idempotencyKey) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999", "Olá", idempotencyKey, null);
}
