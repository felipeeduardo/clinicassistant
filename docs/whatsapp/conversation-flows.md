# Fluxos informativos

`ViewSpecialties` lista até `Conversation:MaxOptionsPerMessage` especialidades ativas e grava as opções na conversa. `ViewProfessionals` faz o mesmo para profissionais ativos. A seleção numérica de especialidade ou profissional é contextual e preserva o identificador no `ContextJson`; a próxima etapa pode consultar disponibilidade sem perder a seleção.

As expressões `hoje`, `amanhã` e `depois de amanhã` são normalizadas deterministicamente e persistidas como `SelectedDate` quando aparecem na mensagem de disponibilidade.

Os fluxos transacionais de agendamento, reagendamento, cancelamento e confirmação exigem confirmação explícita, idempotência e validação de conflito. O roteiro detalhado de perguntas e respostas está em [conversation-playbook.md](conversation-playbook.md).
