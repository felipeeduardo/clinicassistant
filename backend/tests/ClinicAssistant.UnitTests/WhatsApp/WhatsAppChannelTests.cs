using ClinicAssistant.Domain.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class WhatsAppChannelTests
{
    [Fact]
    public void NormalizeRemovesProviderPrefixAndFormatting()
    {
        Assert.Equal("+5511987654321", WhatsAppChannel.Normalize("whatsapp: +55 (11) 98765-4321"));
    }

    [Fact]
    public void ActivateMarksChannelActiveAndDefault()
    {
        var channel = new WhatsAppChannel(Guid.NewGuid(), null, null, WhatsAppProvider.Twilio, "+15551234567");
        channel.Activate();
        Assert.Equal(WhatsAppChannelStatus.Active, channel.Status);
        Assert.True(channel.IsDefault);
    }

    [Theory]
    [InlineData(WhatsAppCurrentUsage.WhatsAppBusinessApp, WhatsAppOnboardingStatus.MigrationRequired)]
    [InlineData(WhatsAppCurrentUsage.WhatsAppBusinessPlatformOtherProvider, WhatsAppOnboardingStatus.ProviderMigrationRequired)]
    [InlineData(WhatsAppCurrentUsage.None, WhatsAppOnboardingStatus.ReadyForRegistration)]
    [InlineData(WhatsAppCurrentUsage.TwilioWhatsApp, WhatsAppOnboardingStatus.ReadyForValidation)]
    public void AssessMapsCurrentUsageToOperationalOnboardingStatus(WhatsAppCurrentUsage usage, WhatsAppOnboardingStatus expected)
    {
        var channel = new WhatsAppChannel(Guid.NewGuid(), null, null, WhatsAppProvider.Twilio, "+15551234567");
        channel.Assess(WhatsAppNumberOrigin.ExistingClinicNumber, usage);
        Assert.Equal(expected, channel.OnboardingStatus);
        Assert.False(string.IsNullOrWhiteSpace(channel.ValidationMessage));
    }
}
