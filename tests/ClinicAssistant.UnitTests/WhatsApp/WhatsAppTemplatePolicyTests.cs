using ClinicAssistant.Infrastructure.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class WhatsAppTemplatePolicyTests
{
    [Fact]
    public void ConversationWindowAllowsTextWithinTwentyFourHours()
    {
        var policy = new WhatsAppConversationWindowPolicy();

        var result = policy.Evaluate(DateTimeOffset.UtcNow.AddHours(-23), DateTimeOffset.UtcNow);

        Assert.True(result.AllowsFreeFormText);
        Assert.False(result.RequiresTemplate);
    }

    [Fact]
    public void ConversationWindowRequiresTemplateWhenExpired()
    {
        var policy = new WhatsAppConversationWindowPolicy();

        var result = policy.Evaluate(DateTimeOffset.UtcNow.AddHours(-25), DateTimeOffset.UtcNow);

        Assert.False(result.AllowsFreeFormText);
        Assert.True(result.RequiresTemplate);
    }

    [Fact]
    public void TemplateVariablesMustMatchTheSchema()
    {
        var validator = new WhatsAppTemplateVariableValidator();

        Assert.True(validator.IsValid("[\"1\",\"2\"]", new Dictionary<string, string> { ["1"] = "Felipe", ["2"] = "Dra. Ana" }));
        Assert.False(validator.IsValid("[\"1\",\"2\"]", new Dictionary<string, string> { ["1"] = "Felipe" }));
    }
}
