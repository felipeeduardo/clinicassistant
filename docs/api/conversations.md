# Conversas e fila humana

| Método | Endpoint | Permissão | Idempotência | Descrição |
| --- | --- | --- | --- | --- |
| `GET` | `/api/conversations`, `/{id}`, `/{id}/messages`, `/{id}/appointments` | `ClinicStaff` | Não | consulta paginada e detalhe isolado por tenant |
| `GET` | `/api/conversation-queue` | `ClinicAdmin` | Não | lista fila humana persistida |
| `POST` | `/{id}/assign`, `/release`, `/transfer`, `/automation/pause`, `/automation/resume`, `/close`, `/reopen` | `ClinicAdmin` | Não | operações humanas com `expectedVersion` |
| `PATCH` | `/{id}/priority` | `ClinicAdmin` | Não | altera prioridade com `expectedVersion` |
| `POST` | `/{id}/messages` | `ClinicAdmin` | `Idempotency-Key` | enfileira mensagem manual na Outbox |
| `POST` | `/{id}/messages/{messageId}/read` | `ClinicStaff` | Não | registra leitura operacional |

Transferência exige usuário ativo do mesmo tenant. Reconsulte o detalhe após cada mutation: versão desatualizada retorna `409`. Para eventos em tempo real, veja [Realtime](realtime.md).
