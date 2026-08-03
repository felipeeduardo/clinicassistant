# Fila humana e transferência

As operações são limitadas a `ClinicAdmin` e ao tenant da sessão.

- `GET /api/conversation-queue?page=&pageSize=&status=` lista a fila persistida, incluindo paciente, prioridade, responsável, motivo e versão.
- `POST /api/conversations/{id}/transfer` recebe `expectedVersion`, `targetUserId` e motivo opcional. O destino deve ser um usuário ativo do mesmo tenant.

A transferência preserva a concorrência da conversa, atualiza a fila e registra auditoria.

## Operações adicionais

- `POST /api/conversations/{id}/close` e `/reopen` recebem `expectedVersion`.
- `PATCH /api/conversations/{id}/priority` recebe `expectedVersion` e `priority` (`Normal`, `High` ou `Urgent`).
- `POST /api/conversations/{id}/messages` enfileira uma mensagem manual. Recebe `expectedVersion` e conteúdo, exige `Idempotency-Key` e cria `ConversationMessage` e `OutboxMessage` na mesma transação. O controller não chama o provedor diretamente.
- `GET /api/conversations/{id}/appointments` retorna as consultas do paciente associado à conversa.
- `GET /api/conversations/operators` retorna usuários ativos do tenant elegíveis para transferência.
