# Atendimento humano

## Fluxo MVP determinístico

`atendente`, `humano`, `recepção`, `falar com alguém` e a opção `7` do menu
solicitam atendimento humano. O orquestrador, sem LLM:

1. mantém o tenant, a conversa e o histórico atuais;
2. muda a conversa para `WaitingHuman` com `AutomationMode = Human`;
3. cria ou reutiliza um único item em `human_queue_items` (FIFO por prioridade e
   `CreatedAt`);
4. grava `conversation.handoff_requested` na auditoria;
5. publica `queue.item.created`/`queue.item.updated` e `conversation.updated`;
6. envia ao paciente a confirmação de encaminhamento pela Outbox e pelo gateway
   configurado.

O item `8` é aceito somente como alias silencioso para mensagens do menu antigo;
ele nunca é exibido no menu atual.

## Invariantes de segurança

- Enquanto `AutomationMode = Human`, mensagens recebidas são persistidas e
  publicadas em tempo real, mas não passam pela máquina de estados e não geram
  resposta automática.
- `WaitingHuman` não possui `AssignedUserId`; `Human` exige ownership.
- O controle otimista de `Conversation.Version` garante que apenas uma tentativa
  de assumir a conversa vença em concorrência.
- Mensagem manual só pode ser enviada pelo operador que possui a conversa e
  sempre percorre API → Outbox → worker → gateway.
- Ao encerrar, a conversa perde o ownership e o item da fila é concluído.
- O isolamento por tenant é aplicado nas consultas de conversas, fila, usuários
  e mensagens.

## Operações da equipe

`ClinicAdmin` e `Receptionist` podem consultar a fila, assumir, liberar,
transferir, pausar/retomar automação, enviar mensagens manuais e encerrar uma
conversa, conforme as policies de operações. `Professional` pode visualizar as
conversas permitidas, mas não opera a fila humana.

O painel deve mostrar paciente/telefone mascarado, status, prioridade, tempo de
espera, última mensagem e operador atribuído. Os estados visuais são:
`Waiting` (aguardando), `Assigned` (em atendimento) e `Completed` (encerrado).

## Disponibilidade e horário

Ainda não existe presença persistida/heartbeat de recepcionistas no modelo atual.
Também não há um vínculo de unidade na conversa para determinar horário de forma
inequívoca. Por isso, a primeira versão não inventa disponibilidade nem diz que
a recepção está fechada: toda solicitação válida é enfileirada deterministicamente.
O horário cadastrado da unidade continua disponível para uma próxima evolução
de decisão `fora do horário` versus `todos ocupados`.

## Próxima evolução

Adicionar presença declarada (`Available`, `Busy`, `Away`, `Offline`) com
heartbeat e a decisão explícita `aguardar`/`continuar com o bot`, sem alterar o
contrato atual de fila, ownership, Outbox ou SignalR.
