using System.Globalization;
using System.Text;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Domain.Conversations;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationStateMachine(IOptions<ConversationOptions> options) : IConversationStateMachine
{
    private readonly ConversationOptions _options = options.Value;

    public ConversationTransitionResult Transition(ConversationInput input)
    {
        var text = Normalize(input.Text);
        if (IsExpired(input)) return Menu(ConversationIntent.Unknown, 0, "conversation.expired");
        if (IsMenuCommand(text)) return Menu(ConversationIntent.Unknown, 0, "conversation.menu");
        if (IsBackCommand(text)) return Menu(input.CurrentIntent, 0, "conversation.menu", ConversationAction.GoBack);
        if (IsCancelFlowCommand(text)) return Menu(ConversationIntent.Unknown, 0, "conversation.cancelled", ConversationAction.CancelFlow);

        var intent = IdentifyIntent(text);
        if (intent == ConversationIntent.TalkToHuman) return new(ConversationFlowState.HandedOff, ConversationStateStatus.HandedOff, intent, ConversationAction.Handoff, 0, "conversation.handoff", []);
        if (intent == ConversationIntent.Farewell) return new(ConversationFlowState.Closed, ConversationStateStatus.Completed, intent, ConversationAction.CloseConversation, 0, "conversation.closed", []);
        if (intent == ConversationIntent.Greeting || input.CurrentFlowState == ConversationFlowState.Initial && intent == ConversationIntent.Unknown)
            return Menu(intent == ConversationIntent.Unknown ? ConversationIntent.Greeting : intent, 0, "conversation.greeting");
        if (intent == ConversationIntent.Unknown || intent == ConversationIntent.Unsupported) return Invalid(input);

        return intent switch
        {
            ConversationIntent.InstitutionalQuestion => new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.institutional", MenuOptions()),
            ConversationIntent.ListSpecialties => new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.specialties", MenuOptions()),
            ConversationIntent.ListProfessionals => new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.professionals", MenuOptions()),
            ConversationIntent.CheckAvailability => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.availability", []),
            ConversationIntent.ScheduleAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.schedule", []),
            ConversationIntent.RescheduleAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.reschedule", []),
            ConversationIntent.CancelAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.cancel_appointment", []),
            ConversationIntent.ConfirmAppointment => new(ConversationFlowState.AwaitingSelection, ConversationStateStatus.Active, intent, ConversationAction.None, 0, "conversation.confirm", []),
            _ => Invalid(input)
        };
    }

    private ConversationTransitionResult Invalid(ConversationInput input)
    {
        var attempts = input.InvalidAttempts + 1;
        return attempts >= _options.MaximumInvalidAttempts
            ? new(ConversationFlowState.HandedOff, ConversationStateStatus.HandedOff, ConversationIntent.TalkToHuman, ConversationAction.Handoff, attempts, "conversation.handoff", [])
            : new(ConversationFlowState.Menu, ConversationStateStatus.Active, ConversationIntent.Unknown, ConversationAction.ShowMenu, attempts, "conversation.invalid_answer", MenuOptions());
    }

    private static ConversationTransitionResult Menu(ConversationIntent intent, int invalidAttempts, string responseKey, ConversationAction action = ConversationAction.ShowMenu) =>
        new(ConversationFlowState.Menu, ConversationStateStatus.Active, intent, action, invalidAttempts, responseKey, MenuOptions());

    private static IReadOnlyCollection<ConversationOptionDefinition> MenuOptions() =>
    [
        new("1", "specialties", 1), new("2", "professionals", 2), new("3", "availability", 3),
        new("4", "schedule", 4), new("5", "reschedule", 5), new("6", "cancel_appointment", 6),
        new("7", "confirm", 7), new("8", "human", 8)
    ];

    private static bool IsExpired(ConversationInput input) => input.ExpiresAt.HasValue && input.ExpiresAt.Value <= input.ReceivedAt;
    private static bool IsMenuCommand(string text) => text is "menu" or "inicio" or "início";
    private static bool IsBackCommand(string text) => text is "voltar" or "retornar";
    private static bool IsCancelFlowCommand(string text) => text is "cancelar fluxo" or "cancelar atendimento" or "sair";

    private static ConversationIntent IdentifyIntent(string text) => text switch
    {
        "1" => ConversationIntent.ListSpecialties,
        "2" => ConversationIntent.ListProfessionals,
        "3" => ConversationIntent.CheckAvailability,
        "4" => ConversationIntent.ScheduleAppointment,
        "5" => ConversationIntent.RescheduleAppointment,
        "6" => ConversationIntent.CancelAppointment,
        "7" => ConversationIntent.ConfirmAppointment,
        "8" => ConversationIntent.TalkToHuman,
        _ when ContainsAny(text, "humano", "atendente", "pessoa") || ContainsAny(text, "dor", "sintoma", "diagnostico", "diagnóstico", "receita", "medicamento", "tratamento") => ConversationIntent.TalkToHuman,
        _ when IsGreeting(text) => ConversationIntent.Greeting,
        _ when ContainsAny(text, "endereco", "endereço", "horario", "horário", "telefone", "localizacao", "localização") => ConversationIntent.InstitutionalQuestion,
        _ when ContainsAny(text, "especialidade", "especialidades") => ConversationIntent.ListSpecialties,
        _ when ContainsAny(text, "profissional", "medico", "médico", "doutor", "doutora") => ConversationIntent.ListProfessionals,
        _ when ContainsAny(text, "disponibilidade", "horario livre", "horário livre") => ConversationIntent.CheckAvailability,
        _ when ContainsAny(text, "reagendar", "remarcar") => ConversationIntent.RescheduleAppointment,
        _ when ContainsAny(text, "cancelar consulta", "desmarcar") => ConversationIntent.CancelAppointment,
        _ when ContainsAny(text, "confirmar", "confirmacao", "confirmação") => ConversationIntent.ConfirmAppointment,
        _ when ContainsAny(text, "agendar", "marcar consulta", "consulta") => ConversationIntent.ScheduleAppointment,
        _ when ContainsAny(text, "tchau", "adeus", "obrigado", "obrigada") => ConversationIntent.Farewell,
        _ => ConversationIntent.Unknown
    };

    private static bool ContainsAny(string text, params string[] values) => values.Any(text.Contains);
    private static bool IsGreeting(string text) => text is "oi" or "ola" || text.StartsWith("bom dia", StringComparison.Ordinal) || text.StartsWith("boa tarde", StringComparison.Ordinal) || text.StartsWith("boa noite", StringComparison.Ordinal);

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Trim().ToLower(CultureInfo.InvariantCulture).Normalize(NormalizationForm.FormD);
        return string.Concat(normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark));
    }
}
