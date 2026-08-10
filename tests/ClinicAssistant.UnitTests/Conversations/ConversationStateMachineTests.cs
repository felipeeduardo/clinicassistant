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
    public void GreetingResponseContainsNumberedMenu()
    {
        var transition = Machine().Transition(Input("Olá"));
        var response = new InMemoryConversationResponseComposer().Compose(new(transition.ResponseKey, transition.Options, "pt-BR"));

        Assert.Contains("4 - Agendar consulta", response.Text, StringComparison.Ordinal);
        Assert.Contains("8 - Falar com atendente", response.Text, StringComparison.Ordinal);
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

    [Theory]
    [InlineData("quais especialidades?", ConversationIntent.ListSpecialties)]
    [InlineData("quem atende cardiologia?", ConversationIntent.ListProfessionals)]
    [InlineData("tem cardiologista amanhã?", ConversationIntent.CheckAvailability)]
    [InlineData("quero falar com alguém", ConversationIntent.TalkToHuman)]
    public void NaturalLanguageResolvesToTheExpectedIntent(string text, ConversationIntent expected)
    {
        var result = Machine().Transition(Input(text, ConversationFlowState.Menu));

        Assert.Equal(expected, result.Intent);
    }

    [Theory]
    [InlineData("menu", ConversationAction.ShowMenu)]
    [InlineData("voltar", ConversationAction.GoBack)]
    [InlineData("ajuda", ConversationAction.None)]
    [InlineData("atendente", ConversationAction.Handoff)]
    public void GlobalCommandsWorkFromAnyFlow(string text, ConversationAction expected)
    {
        var result = Machine().Transition(Input(text, ConversationFlowState.AwaitingSelection));

        Assert.Equal(expected, result.Action);
    }

    [Fact]
    public void NumberUsesPersistedOptionsWhenSelectingAProfessional()
    {
        var options = new[] { new ConversationOptionDefinition("1", "professional:00000000-0000-0000-0000-000000000001", 1) };
        var result = Machine().Transition(Input("1", ConversationFlowState.AwaitingSelection, options: options));

        Assert.Equal(ConversationIntent.CheckAvailability, result.Intent);
    }

    [Fact]
    public void SpecialtySelectionAdvancesToProfessionalsInsteadOfRepeatingTheList()
    {
        var options = new[] { new ConversationOptionDefinition("1", "specialty:00000000-0000-0000-0000-000000000001||Cardiologia", 1) };
        var input = new ConversationInput("1", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ListSpecialties, 0, null, DateTimeOffset.UtcNow, options, new ConversationContext(ConversationIntent.ListSpecialties, ConversationFlowState.AwaitingSelection));

        var result = Machine().Transition(input);

        Assert.Equal(ConversationIntent.ViewProfessionals, result.Intent);
    }

    [Fact]
    public void ScheduleFlowAsksForTheNextMissingContextValue()
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingSelection, null, 0, Guid.NewGuid(), Guid.NewGuid(), null, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        var result = Machine().Transition(new("amanhã", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ScheduleAppointment, 0, null, DateTimeOffset.UtcNow, [], context));

        Assert.Equal(ConversationIntent.ScheduleAppointment, result.Intent);
        Assert.Equal("conversation.schedule_slot", result.ResponseKey);
    }

    [Fact]
    public void DynamicOptionsExposeHumanLabelsInsteadOfInternalIdentifiers()
    {
        var response = new InMemoryConversationResponseComposer().Compose(new(
            "conversation.schedule_slot",
            [new ConversationOptionDefinition("1", "slot:00000000-0000-0000-0000-000000000001|2026-08-11T08:00:00+00:00|2026-08-11T08:30:00+00:00||08:00", 1)],
            "pt-BR"));

        Assert.Contains("1 - 08:00", response.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("slot:", response.Text, StringComparison.Ordinal);
    }

    private static ConversationStateMachine Machine() => new(Options.Create(new ConversationOptions { MaximumInvalidAttempts = 3 }));

    private static ConversationInput Input(string text, ConversationFlowState flowState = ConversationFlowState.Initial, int invalidAttempts = 0, DateTimeOffset? expiresAt = null, IReadOnlyCollection<ConversationOptionDefinition>? options = null) =>
        new(text, flowState, ConversationStateStatus.Active, ConversationIntent.Unknown, invalidAttempts, expiresAt, DateTimeOffset.UtcNow, options);
}
