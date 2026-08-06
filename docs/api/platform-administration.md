# Administração de plataforma

Todas as rotas exigem a policy `PlatformAdmin` e operam acima do contexto de uma única clínica.

| Método | Endpoint | Idempotência | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/platform/tenants`, `/users`, `/clinics` | Não | lista recursos administrativos globais |
| `POST` | `/api/platform/onboarding` | `Idempotency-Key` | cria tenant, clínica, unidade, administrador e integração Fake inicial |
| `POST` | `/api/platform/tenants/{id}/{action}` | Não | altera status conforme as ações suportadas |

Não existem, neste momento, endpoints isolados para editar tenant ou criar usuário fora do onboarding. Essas capacidades não devem ser simuladas pela interface nem pela collection Postman.
