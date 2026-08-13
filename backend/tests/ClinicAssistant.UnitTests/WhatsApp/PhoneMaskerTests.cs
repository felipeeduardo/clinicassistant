using ClinicAssistant.Infrastructure.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class PhoneMaskerTests
{
    [Fact]
    public void MasksPhoneWithoutExposingTheFullNumber()
    {
        var value = new PhoneMasker().Mask("whatsapp:+5581999995348");

        Assert.Equal("+55******348", value);
        Assert.DoesNotContain("99999", value);
    }
}
