using System.Globalization;
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
        Assert.Contains("7 - Falar com atendente", response.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Confirmar consulta", response.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void GreetingResponseExposesStableInteractiveActions()
    {
        var transition = Machine().Transition(Input("Olá"));
        var response = new InMemoryConversationResponseComposer().Compose(new(transition.ResponseKey, transition.Options, "pt-BR"));

        Assert.NotNull(response.Interaction);
        Assert.Equal(ConversationInteractionType.List, response.Interaction!.Type);
        Assert.Contains(response.Interaction.Choices, choice => choice.ActionId == "specialties" && choice.Label == "Ver especialidades");
        Assert.Contains(response.Interaction.Choices, choice => choice.ActionId == "schedule" && choice.Label == "Agendar consulta");
    }

    [Fact]
    public void ConfirmationResponseUsesReplyActionsAndKeepsTextFallback()
    {
        var response = new InMemoryConversationResponseComposer().Compose(new(
            "conversation.confirm_slot",
            [
                new ConversationOptionDefinition("1", "confirm_slot", 1),
                new ConversationOptionDefinition("2", "more_slots", 2)
            ],
            "pt-BR",
            "Você escolheu:\nDra. Ana Minimal\n23/08 às 10:00.\n\nPosso confirmar esse agendamento?\n\nPara voltar ao início, escreva menu."));

        Assert.NotNull(response.Interaction);
        Assert.Equal(ConversationInteractionType.ReplyButtons, response.Interaction!.Type);
        Assert.Equal(["confirm_slot", "more_slots"], response.Interaction.Choices.Select(choice => choice.ActionId));
        Assert.Contains("1 - Confirmar agendamento", response.Text, StringComparison.Ordinal);
        Assert.Contains("2 - Mais horários", response.Text, StringComparison.Ordinal);
        Assert.Contains("escreva menu", response.Text, StringComparison.Ordinal);
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
    public void InteractiveActionIdResolvesAgainstTheOptionSnapshot()
    {
        var options = new[]
        {
            new ConversationOptionDefinition("1", "specialty:00000000-0000-0000-0000-000000000001||Cardiologia", 1, "specialty:00000000-0000-0000-0000-000000000001")
        };

        var result = new ConversationIntentResolver().Resolve(
            "specialty:00000000-0000-0000-0000-000000000001",
            new ConversationContext(ConversationIntent.ListSpecialties, ConversationFlowState.AwaitingSelection),
            options);

        Assert.Equal(ConversationIntent.ViewProfessionals, result.Intent);
        Assert.True(result.IsContextualSelection);
    }

    [Fact]
    public void SameNumberUsesTheProfessionalListAfterSpecialtySelection()
    {
        var options = new[] { new ConversationOptionDefinition("1", "professional:00000000-0000-0000-0000-000000000001||Dra. Ana Minimal", 1) };
        var context = new ConversationContext(ConversationIntent.ViewProfessionals, ConversationFlowState.AwaitingSelection, SelectedSpecialtyId: Guid.NewGuid());

        var result = Machine().Transition(new("1", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ViewProfessionals, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationIntent.CheckAvailability, result.Intent);
    }

    [Fact]
    public void ProfessionalSelectionMovesPersistedContextToAvailability()
    {
        var context = new ConversationContext(ConversationIntent.ListSpecialties, ConversationFlowState.AwaitingSelection);
        var options = new[]
        {
            new ConversationOptionDefinition("1", "professional:00000000-0000-0000-0000-000000000001|00000000-0000-0000-0000-000000000002||Dr. Bruno", 1)
        };

        var selected = ConversationOrchestrator.ApplyContextualSelection(context, "1", options);

        Assert.Equal(ConversationIntent.CheckAvailability, selected.CurrentIntent);
        Assert.True(selected.AwaitingAvailableDaySelection);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), selected.SelectedProfessionalId);
    }

    [Fact]
    public void SpecialtySelectionAfterDirectProfessionalKeepsAvailabilityContext()
    {
        var context = new ConversationContext(
            ConversationIntent.CheckAvailability,
            ConversationFlowState.AwaitingSelection,
            SelectedProfessionalId: Guid.NewGuid(),
            AwaitingAvailableDaySelection: true);
        var options = new[]
        {
            new ConversationOptionDefinition("1", "specialty:00000000-0000-0000-0000-000000000003||Clínico Geral", 1)
        };

        var selected = ConversationOrchestrator.ApplyContextualSelection(context, "1", options);

        Assert.Equal(ConversationIntent.CheckAvailability, selected.CurrentIntent);
        Assert.True(selected.AwaitingAvailableDaySelection);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000003"), selected.SelectedSpecialtyId);
    }

    [Fact]
    public void ProfessionalNameIsResolvedAgainstPresentedOptions()
    {
        var options = new[] { new ConversationOptionDefinition("1", "professional:00000000-0000-0000-0000-000000000001||Dra. Ana Minimal", 1) };
        var context = new ConversationContext(ConversationIntent.ViewProfessionals, ConversationFlowState.AwaitingSelection);

        var result = Machine().Transition(new("Dra. Ana Minimal", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ViewProfessionals, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationIntent.CheckAvailability, result.Intent);
    }

    [Theory]
    [InlineData("mais horários")]
    [InlineData("outra data")]
    public void AvailabilityNavigationCommandsRemainContextual(string command)
    {
        var context = new ConversationContext(ConversationIntent.CheckAvailability, ConversationFlowState.AwaitingSelection, SelectedProfessionalId: Guid.NewGuid());

        var result = Machine().Transition(new(command, ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.CheckAvailability, 0, null, DateTimeOffset.UtcNow, [], context));

        Assert.Equal(ConversationIntent.CheckAvailability, result.Intent);
    }

    [Fact]
    public void GroupedAvailabilityTextDoesNotDuplicateRenderedOptions()
    {
        var response = new InMemoryConversationResponseComposer().Compose(new(
            "conversation.availability",
            [new ConversationOptionDefinition("1", "slot:00000000-0000-0000-0000-000000000001|2026-08-21T13:00:00+00:00|2026-08-21T13:30:00+00:00||10:00", 1)],
            "pt-BR",
            "Encontrei estes horários:\n\n*Hoje, 21/08*\n1 - 10:00",
            OptionsAlreadyRendered: true));

        Assert.Equal(1, response.Text.Split("1 - 10:00", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void AvailabilityFallbackTextDoesNotDuplicateRenderedOptions()
    {
        var response = new InMemoryConversationResponseComposer().Compose(new(
            "conversation.availability",
            [
                new ConversationOptionDefinition("1", "specialty:00000000-0000-0000-0000-000000000001||Clínico Geral", 1),
                new ConversationOptionDefinition("2", "specialty:00000000-0000-0000-0000-000000000002||Pediatria", 2),
                new ConversationOptionDefinition("3", "professionals", 3)
            ],
            "pt-BR",
            "Não encontrei essa especialidade.\n\n1 - Clínico Geral\n2 - Pediatria\n3 - Ver profissionais",
            OptionsAlreadyRendered: true));

        Assert.Equal(1, response.Text.Split("1 - Clínico Geral", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, response.Text.Split("2 - Pediatria", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void SelectedSlotConfirmationDoesNotRestartScheduling()
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingSelection,
            SelectedSpecialtyId: Guid.NewGuid(), SelectedProfessionalId: Guid.NewGuid(), SelectedDate: DateOnly.FromDateTime(DateTime.UtcNow.Date), PendingConfirmation: true,
            SelectedSlotStartsAt: DateTimeOffset.Parse("2026-08-23T18:30:00Z", CultureInfo.InvariantCulture), SelectedSlotEndsAt: DateTimeOffset.Parse("2026-08-23T19:00:00Z", CultureInfo.InvariantCulture));
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1),
            new ConversationOptionDefinition("2", "more_slots", 2)
        };

        var result = Machine().Transition(new("1", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active,
            ConversationIntent.ScheduleAppointment, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationIntent.ConfirmSelectedSlot, result.Intent);
        Assert.Equal("conversation.confirm_selected_slot", result.ResponseKey);
    }

    [Theory]
    [InlineData("1", ConversationIntent.ConfirmSelectedSlot)]
    [InlineData("sim", ConversationIntent.ConfirmSelectedSlot)]
    [InlineData("confirmar", ConversationIntent.ConfirmSelectedSlot)]
    public void ConfirmationStateRoutesNewAppointmentConfirmationToItsOwnAction(string input, ConversationIntent expectedIntent)
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingScheduleConfirmation,
            SelectedProfessionalId: Guid.NewGuid(), PendingConfirmation: true,
            SelectedSlotStartsAt: DateTimeOffset.Parse("2026-08-23T12:30:00Z", CultureInfo.InvariantCulture),
            SelectedSlotEndsAt: DateTimeOffset.Parse("2026-08-23T13:00:00Z", CultureInfo.InvariantCulture));
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1, "confirm_slot"),
            new ConversationOptionDefinition("2", "more_slots", 2, "more_slots")
        };

        var result = Machine().Transition(new(input, ConversationFlowState.AwaitingScheduleConfirmation, ConversationStateStatus.Active,
            ConversationIntent.ScheduleAppointment, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(expectedIntent, result.Intent);
        Assert.NotEqual(ConversationAction.ShowMenu, result.Action);
        Assert.Equal(ConversationFlowState.AwaitingScheduleConfirmation, result.FlowState);
    }

    [Fact]
    public void ConfirmationStateKeepsContextForInvalidInput()
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingScheduleConfirmation,
            SelectedProfessionalId: Guid.NewGuid(), PendingConfirmation: true, SelectedSlotStartsAt: DateTimeOffset.UtcNow,
            SelectedSlotEndsAt: DateTimeOffset.UtcNow.AddMinutes(30));
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1, "confirm_slot"),
            new ConversationOptionDefinition("2", "more_slots", 2, "more_slots")
        };

        var result = Machine().Transition(new("9", ConversationFlowState.AwaitingScheduleConfirmation, ConversationStateStatus.Active,
            ConversationIntent.ScheduleAppointment, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationFlowState.AwaitingScheduleConfirmation, result.FlowState);
        Assert.Equal(ConversationIntent.ScheduleAppointment, result.Intent);
        Assert.Equal(options, result.Options);
    }

    [Fact]
    public void RepeatedInvalidConfirmationDoesNotFallBackToMainMenu()
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingScheduleConfirmation,
            SelectedProfessionalId: Guid.NewGuid(), PendingConfirmation: true, SelectedSlotStartsAt: DateTimeOffset.UtcNow,
            SelectedSlotEndsAt: DateTimeOffset.UtcNow.AddMinutes(30));
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1, "confirm_slot"),
            new ConversationOptionDefinition("2", "more_slots", 2, "more_slots")
        };

        var result = Machine().Transition(new("9", ConversationFlowState.AwaitingScheduleConfirmation, ConversationStateStatus.Active,
            ConversationIntent.ScheduleAppointment, 1, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationFlowState.AwaitingScheduleConfirmation, result.FlowState);
        Assert.Equal(ConversationAction.None, result.Action);
        Assert.Equal("conversation.invalid_confirmation", result.ResponseKey);
        Assert.Equal(2, result.Options.Count);
    }

    [Fact]
    public void MainMenuOptionSevenStartsHumanHandoff()
    {
        var result = Machine().Transition(Input("7", ConversationFlowState.Menu));

        Assert.Equal(ConversationIntent.HumanHandoff, result.Intent);
        Assert.Equal(ConversationAction.Handoff, result.Action);
    }

    [Fact]
    public void ConfirmationMenuUsesDistinctSemanticActions()
    {
        var resolver = new ConversationIntentResolver();
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingScheduleConfirmation, PendingConfirmation: true,
            SelectedSlotStartsAt: DateTimeOffset.UtcNow, SelectedSlotEndsAt: DateTimeOffset.UtcNow.AddMinutes(30));
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1),
            new ConversationOptionDefinition("2", "more_slots", 2)
        };

        Assert.Equal(ConversationIntent.ConfirmSelectedSlot, resolver.Resolve("1", context, options).Intent);
        Assert.Equal(ConversationIntent.CheckAvailability, resolver.Resolve("2", context, options).Intent);
        Assert.Equal(ConversationIntent.MainMenu, resolver.Resolve("menu", context, options).Intent);
    }

    [Fact]
    public void LegacyMainMenuOptionEightFallsBackToHumanHandoff()
    {
        var result = Machine().Transition(Input("8", ConversationFlowState.Menu));

        Assert.Equal(ConversationIntent.HumanHandoff, result.Intent);
    }

    [Fact]
    public void MainMenuOptionSevenIsResolvedEvenWhenPersistedOptionsAreStale()
    {
        var context = new ConversationContext(ConversationIntent.MainMenu, ConversationFlowState.Menu, null, 0);
        var staleOptions = ConversationStateMachine.MenuOptions().Where(option => option.Key != "7").ToArray();

        var result = Machine().Transition(new("7", ConversationFlowState.Menu, ConversationStateStatus.Active,
            ConversationIntent.MainMenu, 0, null, DateTimeOffset.UtcNow, staleOptions, context));

        Assert.Equal(ConversationIntent.HumanHandoff, result.Intent);
        Assert.Equal(ConversationAction.Handoff, result.Action);
    }

    [Fact]
    public void InvalidConfirmationKeepsTheCurrentActionMap()
    {
        var options = new[]
        {
            new ConversationOptionDefinition("1", "confirm_slot", 1),
            new ConversationOptionDefinition("2", "more_slots", 2)
        };
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingScheduleConfirmation, PendingConfirmation: true,
            SelectedSlotStartsAt: DateTimeOffset.UtcNow, SelectedSlotEndsAt: DateTimeOffset.UtcNow.AddMinutes(30));

        var result = Machine().Transition(new("9", ConversationFlowState.AwaitingScheduleConfirmation, ConversationStateStatus.Active,
            ConversationIntent.ScheduleAppointment, 0, null, DateTimeOffset.UtcNow, options, context));

        Assert.Equal(ConversationFlowState.AwaitingScheduleConfirmation, result.FlowState);
        Assert.Equal(2, result.Options.Count);
        Assert.Equal(ConversationIntent.ScheduleAppointment, result.Intent);
    }

    [Fact]
    public void MiddleSlotUsesThePersistedDisplayedOptionMap()
    {
        var options = Enumerable.Range(1, 10)
            .Select(index => new ConversationOptionDefinition(
                index.ToString(CultureInfo.InvariantCulture),
                $"slot:00000000-0000-0000-0000-000000000001|00000000-0000-0000-0000-000000000002|2026-08-23T{index + 7:00}:00Z|2026-08-23T{index + 7:00}:30Z||{index + 7:00}:00",
                index))
            .ToArray();
        var context = new ConversationContext(ConversationIntent.CheckAvailability, ConversationFlowState.AwaitingSlotSelection,
            SelectedProfessionalId: Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var resolution = new ConversationIntentResolver().Resolve("5", context, options);

        Assert.True(resolution.IsContextualSelection);
        Assert.Equal(ConversationIntent.CheckAvailability, resolution.Intent);
        Assert.Equal("slot:00000000-0000-0000-0000-000000000001|00000000-0000-0000-0000-000000000002|2026-08-23T12:00Z|2026-08-23T12:30Z||12:00", options[4].Value);

        var selected = ConversationOrchestrator.ApplyContextualSelection(context, "5", options);
        Assert.Equal(DateTimeOffset.Parse("2026-08-23T12:00:00Z", CultureInfo.InvariantCulture), selected.SelectedSlotStartsAt);
        Assert.Equal(DateTimeOffset.Parse("2026-08-23T12:30:00Z", CultureInfo.InvariantCulture), selected.SelectedSlotEndsAt);
        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000002"), selected.SelectedUnitId);
    }

    [Fact]
    public void AvailableDaySelectionUsesThePersistedDayOption()
    {
        var context = new ConversationContext(ConversationIntent.ScheduleAppointment, ConversationFlowState.AwaitingSelection,
            SelectedProfessionalId: Guid.NewGuid(), AwaitingAvailableDaySelection: true);
        var options = new[]
        {
            new ConversationOptionDefinition("1", "day:2026-08-23||Domingo, 23/08", 1, "day:2026-08-23"),
            new ConversationOptionDefinition("2", "day:2026-08-24||Segunda-feira, 24/08", 2, "day:2026-08-24")
        };

        var resolution = new ConversationIntentResolver().Resolve("2", context, options);
        var selected = ConversationOrchestrator.ApplyContextualSelection(context, "2", options);

        Assert.True(resolution.IsContextualSelection);
        Assert.Equal(ConversationIntent.ScheduleAppointment, resolution.Intent);
        Assert.Equal(new DateOnly(2026, 8, 24), selected.SelectedDate);
        Assert.False(selected.AwaitingAvailableDaySelection);
    }

    [Fact]
    public void AvailabilityListsAreNotAppendedTwiceWhenAlreadyRendered()
    {
        var composer = new InMemoryConversationResponseComposer();
        var days = new[]
        {
            new ConversationOptionDefinition("1", "day:2026-08-23||Domingo, 23/08", 1, "day:2026-08-23"),
            new ConversationOptionDefinition("2", "day:2026-08-24||Segunda-feira, 24/08", 2, "day:2026-08-24")
        };
        var response = composer.Compose(new("conversation.availability", days, "pt-BR",
            "Qual dia fica melhor?\n\n1 - Domingo, 23/08\n2 - Segunda-feira, 24/08", true));

        Assert.Equal(1, response.Text.Split("1 - Domingo, 23/08", StringSplitOptions.None).Length - 1);
        Assert.Equal(1, response.Text.Split("2 - Segunda-feira, 24/08", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void BackFromProfessionalSelectionReturnsToSpecialties()
    {
        var context = new ConversationContext(ConversationIntent.ViewProfessionals, ConversationFlowState.AwaitingSelection, SelectedSpecialtyId: Guid.NewGuid());

        var result = Machine().Transition(new("voltar", ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, ConversationIntent.ViewProfessionals, 0, null, DateTimeOffset.UtcNow, [], context));

        Assert.Equal(ConversationIntent.ListSpecialties, result.Intent);
        Assert.Equal(ConversationFlowState.AwaitingSelection, result.FlowState);
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
