using ClinicAssistant.Application.Conversations;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class InMemoryConversationResponseComposer : IConversationResponseComposer
{
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

    public ConversationResponse Compose(ConversationResponseRequest request) =>
        new(Responses.TryGetValue(request.ResponseKey, out var text) ? text : Responses["conversation.menu"], request.Options);
}
