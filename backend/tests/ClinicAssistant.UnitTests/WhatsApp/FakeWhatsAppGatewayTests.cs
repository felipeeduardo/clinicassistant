using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;
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

    [Fact]
    public async Task InteractiveChoicesSurviveOutboxContractSerialization()
    {
        var command = new SendWhatsAppMessageCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), WhatsAppOutgoingMessageType.Interactive,
            "+5581999999999", "Escolha uma especialidade", null, null, null, "outbox-interactive", "correlation-interactive",
            new WhatsAppInteraction(WhatsAppInteractionType.List, [new("specialty:abc", "Clínico Geral")]));

        var roundTrip = JsonSerializer.Deserialize<SendWhatsAppMessageCommand>(JsonSerializer.Serialize(command));

        Assert.NotNull(roundTrip?.Interaction);
        Assert.Equal("specialty:abc", Assert.Single(roundTrip!.Interaction!.Choices).ActionId);
        Assert.Equal("Clínico Geral", Assert.Single(roundTrip.Interaction.Choices).Label);
        Assert.Equal(WhatsAppInteractionType.List, roundTrip.Interaction.Type);
        Assert.Equal(WhatsAppOutgoingMessageType.Interactive, roundTrip.Type);
    }

    private static FakeWhatsAppGateway CreateGateway(FakeWhatsAppFailureMode failureMode = FakeWhatsAppFailureMode.None) =>
        new(Options.Create(new WhatsAppOptions { Fake = new FakeWhatsAppOptions { DelayMilliseconds = 0, FailureMode = failureMode } }), NullLogger<FakeWhatsAppGateway>.Instance);

    private static SendWhatsAppTextRequest CreateTextRequest(string idempotencyKey) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999", "Olá", idempotencyKey, null);
}
