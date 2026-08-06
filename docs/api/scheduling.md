# Agenda e disponibilidade

Esta é a referência canônica de agendamento. O detalhe de contratos e campos está sempre no [OpenAPI](openapi.md).

| Método | Endpoint | Permissão | Idempotência | Descrição |
| --- | --- | --- | --- | --- |
| `GET` | `/api/appointments`, `/search`, `/{id}` | `ClinicStaff` | Não | lista, busca ou detalha consultas do tenant |
| `POST` | `/api/appointments` | `ClinicStaff` | `Idempotency-Key` | cria consulta após validar o slot |
| `POST` | `/api/appointments/{id}/confirm` | `ClinicStaff` | `Idempotency-Key` | confirma com `expectedVersion` |
| `POST` | `/api/appointments/{id}/cancel` | `ClinicStaff` | `Idempotency-Key` | cancela com motivo e versão |
| `POST` | `/api/appointments/{id}/reschedule` | `ClinicStaff` | `Idempotency-Key` | preserva a original como `Rescheduled` e cria substituta |
| `GET/POST/PUT` | `/api/professionals/{id}/availability...` | `Professionals.View/Manage` | Não | consulta e administra regras de disponibilidade |
| `GET/POST/DELETE` | `/api/professionals/{id}/blocks` e `/vacations` | `Professionals.View/Manage` | Não | administra bloqueios e férias |
| `GET` | `/api/professionals/{id}/schedule` | `Professionals.View` | Não | retorna agenda, bloqueios e férias do período |

Operações que modificam uma consulta recebem `expectedVersion`; consulte o detalhe após cada alteração. Conflitos de agenda ou concorrência retornam `409`.
