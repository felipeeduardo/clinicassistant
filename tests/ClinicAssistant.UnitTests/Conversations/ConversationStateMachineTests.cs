using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Infrastructure.Conversations;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.Conversations;

public sealed class ConversationStateMachineTests
{
    [Fact]
    public void GreetingStartsTheMenu()
    {
        var result = Machine().Transition(Input("Olá"));

        Assert.Equal(ConversationFlowState.Menu, result.FlowState);
        Assert.Equal(ConversationIntent.Greeting, result.Intent);
        Assert.Equal("conversation.greeting", result.ResponseKey);
        Assert.NotEmpty(result.Options);
    }

    [Fact]
    public void NumericMenuOptionIdentifiesSchedulingIntent()
    {
        var result = Machine().Transition(Input("4", ConversationFlowState.Menu));

        Assert.Equal(ConversationIntent.ScheduleAppointment, result.Intent);
        Assert.Equal(ConversationFlowState.AwaitingSelection, result.FlowState);
    }

    [Fact]
    public void MenuAndBackCommandsReturnToTheMenu()
    {
        var menu = Machine().Transition(Input("menu", ConversationFlowState.AwaitingSelection));
        var back = Machine().Transition(Input("voltar", ConversationFlowState.AwaitingSelection));

        Assert.Equal(ConversationAction.ShowMenu, menu.Action);
        Assert.Equal(ConversationAction.GoBack, back.Action);
        Assert.Equal(ConversationFlowState.Menu, back.FlowState);
    }

    [Fact]
    public void InvalidAnswersEscalateAfterConfiguredLimit()
    {
        var result = Machine().Transition(Input("qualquer coisa", ConversationFlowState.Menu, invalidAttempts: 2));

        Assert.Equal(ConversationAction.Handoff, result.Action);
        Assert.Equal(ConversationStateStatus.HandedOff, result.Status);
    }

    [Fact]
    public void ClinicalQuestionRequestsHumanHandoff()
    {
        var result = Machine().Transition(Input("Estou com dor e preciso de diagnóstico"));

        Assert.Equal(ConversationIntent.TalkToHuman, result.Intent);
        Assert.Equal(ConversationAction.Handoff, result.Action);
    }

    [Fact]
    public void ExpiredStateRestartsAtMenu()
    {
        var result = Machine().Transition(Input("agendar", ConversationFlowState.AwaitingSelection, expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1)));

        Assert.Equal("conversation.expired", result.ResponseKey);
        Assert.Equal(ConversationFlowState.Menu, result.FlowState);
    }

    private static ConversationStateMachine Machine() => new(Options.Create(new ConversationOptions { MaximumInvalidAttempts = 3 }));

    private static ConversationInput Input(string text, ConversationFlowState flowState = ConversationFlowState.Initial, int invalidAttempts = 0, DateTimeOffset? expiresAt = null) =>
        new(text, flowState, ConversationStateStatus.Active, ConversationIntent.Unknown, invalidAttempts, expiresAt, DateTimeOffset.UtcNow);
}
