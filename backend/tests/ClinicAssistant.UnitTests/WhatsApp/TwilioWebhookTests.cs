using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class TwilioWebhookTests
{
    [Fact]
    public void SignatureValidatorAcceptsAValidTwilioSignature()
    {
        const string token = "token-for-test";
        const string url = "https://api.example.test/api/webhooks/whatsapp/twilio/wha_test";
        const string signature = "+KiwRi1GDgNjYDEoUg9Mt1EncJw=";
        var parameters = new Dictionary<string, string> { ["Body"] = "Olá", ["MessageSid"] = "SM123" };
        var validator = new TwilioWebhookSignatureValidator(Options.Create(new TwilioOptions { AuthToken = token }));

        var isValid = validator.IsValid(url, parameters, signature);

        Assert.True(isValid);
        Assert.False(validator.IsValid(url, parameters, "invalid"));
    }

    [Fact]
    public void ParserRemovesProviderPrefixAndCollectsMedia()
    {
        var parser = new TwilioWhatsAppWebhookParser(new WhatsAppPhoneNumberFormatter());
        var webhook = new TwilioIncomingWebhook("SM123", null, "whatsapp:+5581999999999", "whatsapp:+5581888888888", "Imagem", "Ana", null, 1, null, null, null, null, null, [new("https://media.example.test/1", "image/jpeg", 0)]);

        var message = parser.Parse(webhook, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "correlation-1");

        Assert.Equal("+5581999999999", message.SenderPhone);
        Assert.Equal("+5581888888888", message.RecipientPhone);
        Assert.Equal(WhatsAppIncomingMessageType.Media, message.Type);
        Assert.Single(message.Media);
    }

    [Fact]
    public void ParserNormalizesInteractiveButtonPayloadAsActionId()
    {
        var parser = new TwilioWhatsAppWebhookParser(new WhatsAppPhoneNumberFormatter());
        var webhook = new TwilioIncomingWebhook("SM456", null, "whatsapp:+5581999999999", "whatsapp:+18382501528", null, "Felipe", null, 0, "Confirmar agendamento", "confirm_selected_slot", null, null, null, []);

        var message = parser.Parse(webhook, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "correlation-2");

        Assert.Equal(WhatsAppIncomingMessageType.Interactive, message.Type);
        Assert.Equal("confirm_selected_slot", message.ActionId);
        Assert.Equal("confirm_selected_slot", message.Text);
    }
}
