using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class WhatsAppMediaPolicyTests
{
    [Fact]
    public void AllowsConfiguredMediaAndDefersSizeValidationWhenSizeIsUnavailable()
    {
        var result = CreatePolicy().Evaluate("image/jpeg", null);

        Assert.Equal(WhatsAppMediaDisposition.Accepted, result.Disposition);
        Assert.True(result.RequiresDeferredSizeValidation);
    }

    [Fact]
    public void RoutesUnsupportedMediaToHumanAssistance()
    {
        var result = CreatePolicy().Evaluate("video/mp4", 100);

        Assert.Equal(WhatsAppMediaDisposition.RequiresHuman, result.Disposition);
        Assert.False(result.RequiresDeferredSizeValidation);
    }

    [Fact]
    public void RoutesOversizedMediaToHumanAssistance()
    {
        var result = CreatePolicy().Evaluate("application/pdf", 1_001);

        Assert.Equal(WhatsAppMediaDisposition.RequiresHuman, result.Disposition);
    }

    private static WhatsAppMediaPolicy CreatePolicy() => new(Options.Create(new WhatsAppOptions
    {
        Media = new WhatsAppMediaOptions { MaxFileSizeBytes = 1_000, AllowedTypes = "image/jpeg,application/pdf" }
    }));
}
