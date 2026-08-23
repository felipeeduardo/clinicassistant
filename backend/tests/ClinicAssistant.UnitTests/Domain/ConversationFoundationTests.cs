using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.Domain;

public sealed class ConversationFoundationTests
{
    [Fact]
    public void NewConversationDefaultsToAutomatedModeAndNormalPriority()
    {
        var conversation = new Conversation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999");

        Assert.Equal(ConversationAutomationMode.Automated, conversation.AutomationMode);
        Assert.Equal(ConversationPriority.Normal, conversation.Priority);
        Assert.Equal(1, conversation.Version);
    }

    [Fact]
    public void NewConversationStateStartsAtTheInitialFlowState()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var state = new ConversationState(Guid.NewGuid(), Guid.NewGuid(), expiresAt);

        Assert.Equal(ConversationStateStatus.Active, state.Status);
        Assert.Equal(ConversationFlowState.Initial, state.FlowState);
        Assert.Equal(ConversationIntent.Unknown, state.Intent);
        Assert.Equal(expiresAt, state.ExpiresAt);
    }

    [Fact]
    public void HumanHandoffDisablesAutomationAndClearsOwnership()
    {
        var conversation = new Conversation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999");
        conversation.Assign(Guid.NewGuid());

        conversation.RequestHumanHandoff();

        Assert.Equal(ConversationStatus.WaitingHuman, conversation.Status);
        Assert.Equal(ConversationAutomationMode.Human, conversation.AutomationMode);
        Assert.Null(conversation.AssignedUserId);
    }

    [Fact]
    public void ClosingHumanConversationClearsOwnership()
    {
        var conversation = new Conversation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+5581999999999");
        conversation.Assign(Guid.NewGuid());

        conversation.Close();

        Assert.Equal(ConversationStatus.Closed, conversation.Status);
        Assert.Null(conversation.AssignedUserId);
    }
}
