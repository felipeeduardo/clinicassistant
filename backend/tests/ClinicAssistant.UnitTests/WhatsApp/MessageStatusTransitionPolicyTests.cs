using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.WhatsApp;

public sealed class MessageStatusTransitionPolicyTests
{
    [Theory]
    [InlineData(ConversationMessageStatus.Pending, ConversationMessageStatus.Queued)]
    [InlineData(ConversationMessageStatus.Queued, ConversationMessageStatus.Accepted)]
    [InlineData(ConversationMessageStatus.Accepted, ConversationMessageStatus.Sent)]
    [InlineData(ConversationMessageStatus.Sent, ConversationMessageStatus.Delivered)]
    [InlineData(ConversationMessageStatus.Delivered, ConversationMessageStatus.Read)]
    public void PolicyAllowsForwardTransitions(ConversationMessageStatus current, ConversationMessageStatus next)
    {
        Assert.True(new MessageStatusTransitionPolicy().CanTransition(current, next));
    }

    [Theory]
    [InlineData(ConversationMessageStatus.Read, ConversationMessageStatus.Delivered)]
    [InlineData(ConversationMessageStatus.Delivered, ConversationMessageStatus.Sent)]
    [InlineData(ConversationMessageStatus.Sent, ConversationMessageStatus.Queued)]
    public void PolicyRejectsRegressions(ConversationMessageStatus current, ConversationMessageStatus next)
    {
        Assert.False(new MessageStatusTransitionPolicy().CanTransition(current, next));
    }
}
