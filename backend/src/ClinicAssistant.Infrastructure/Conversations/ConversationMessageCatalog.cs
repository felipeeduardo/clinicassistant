namespace ClinicAssistant.Infrastructure.Conversations;

internal static class ConversationMessageCatalog
{
    public static readonly IReadOnlyDictionary<string, string> Text = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["conversation.greeting"] = "Olá! 👋\nPosso ajudar você com sua consulta.\n\nComo posso ajudar?",
        ["conversation.menu"] = "Como posso ajudar?",
        ["conversation.invalid_answer"] = "Não consegui identificar essa opção. Você pode escrever o que precisa ou escolher uma opção.",
        ["conversation.invalid_confirmation"] = "Não reconheci essa resposta. Escolha 1 para confirmar, 2 para escolher outro horário ou escreva menu para sair.",
        ["conversation.invalid_again"] = "Ainda não consegui entender. Quer voltar ao menu ou falar com um atendente?",
        ["conversation.expired"] = "Vamos continuar por aqui. Como posso ajudar?",
        ["conversation.cancelled"] = "Tudo bem, interrompi essa operação. Como posso ajudar agora?",
        ["conversation.back"] = "Voltamos um passo. Como posso ajudar?",
        ["conversation.handoff"] = "Tudo bem. Vou encaminhar sua conversa para nossa equipe.\nAssim que alguém assumir, você continuará por aqui.",
        ["conversation.closed"] = "Atendimento encerrado. Quando precisar, é só enviar uma nova mensagem.",
        ["conversation.help"] = "Posso ajudar a consultar especialidades, profissionais e horários, além de marcar, reagendar, cancelar ou confirmar consultas.",
        ["conversation.help_contextual"] = "Você pode escrever o que deseja fazer, usar um número da lista, voltar ou cancelar a operação.",
        ["conversation.repeat"] = "Claro. Vou repetir a orientação anterior.",
        ["conversation.institutional"] = "Posso ajudar com informações da clínica. O que você gostaria de consultar?",
        ["conversation.specialties"] = "Claro. Vou mostrar as especialidades disponíveis.",
        ["conversation.professionals"] = "Claro. Vou mostrar os profissionais disponíveis.",
        ["conversation.availability"] = "Vamos consultar os horários disponíveis. Qual especialidade ou profissional você procura?",
        ["conversation.schedule"] = "Vamos reunir os dados necessários para o agendamento.",
        ["conversation.reschedule"] = "Vamos encontrar um novo horário para sua consulta.",
        ["conversation.cancel_appointment"] = "Vou localizar sua consulta antes de qualquer cancelamento.",
        ["conversation.confirm"] = "Vou localizar sua próxima consulta para confirmar sua presença."
    };
}
