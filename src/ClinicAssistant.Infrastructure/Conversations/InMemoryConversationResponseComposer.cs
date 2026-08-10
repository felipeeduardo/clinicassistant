using ClinicAssistant.Application.Conversations;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class InMemoryConversationResponseComposer : IConversationResponseComposer
{
    private static readonly IReadOnlyDictionary<string, string> OptionLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["specialties"] = "Ver especialidades",
        ["professionals"] = "Ver profissionais",
        ["availability"] = "Consultar disponibilidade",
        ["schedule"] = "Agendar consulta",
        ["reschedule"] = "Reagendar consulta",
        ["cancel_appointment"] = "Cancelar consulta",
        ["confirm"] = "Confirmar consulta",
        ["human"] = "Falar com atendente"
    };

    private static readonly Dictionary<string, string> Responses = new()
    {
        ["conversation.greeting"] = "Olá! Como posso ajudar com o atendimento da clínica?",
        ["conversation.menu"] = "Escolha uma opção do menu para continuar.",
        ["conversation.invalid_answer"] = "Não entendi a sua resposta. Escolha uma opção do menu.",
        ["conversation.expired"] = "Seu atendimento anterior expirou. Vamos recomeçar pelo menu.",
        ["conversation.cancelled"] = "O fluxo atual foi cancelado. Você pode escolher uma nova opção.",
        ["conversation.handoff"] = "Vou encaminhar você para o atendimento humano.",
        ["conversation.closed"] = "Atendimento encerrado. Quando precisar, envie uma nova mensagem.",
        ["conversation.institutional"] = "Posso ajudar com informações da clínica. Escolha outra opção para continuar.",
        ["conversation.specialties"] = "Vou consultar as especialidades disponíveis.",
        ["conversation.professionals"] = "Vou consultar os profissionais disponíveis.",
        ["conversation.availability"] = "Vou precisar de mais dados para consultar a disponibilidade.",
        ["conversation.schedule"] = "Vamos reunir os dados necessários para o agendamento.",
        ["conversation.reschedule"] = "Vamos reunir os dados necessários para o reagendamento.",
        ["conversation.cancel_appointment"] = "Vamos reunir os dados necessários para o cancelamento.",
        ["conversation.confirm"] = "Vamos reunir os dados necessários para a confirmação."
    };

    public ConversationResponse Compose(ConversationResponseRequest request)
    {
        var text = Responses.TryGetValue(request.ResponseKey, out var responseText) ? responseText : Responses["conversation.menu"];
        if (request.Options.Count == 0) return new(text, request.Options);

        var menu = string.Join(Environment.NewLine, request.Options
            .OrderBy(option => option.DisplayOrder)
            .Select(option => $"{option.Key} - {OptionLabels.GetValueOrDefault(option.Value, option.Value)}"));
        return new($"{text}{Environment.NewLine}{Environment.NewLine}{menu}", request.Options);
    }
}
