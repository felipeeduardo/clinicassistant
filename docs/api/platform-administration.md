# Administração da plataforma

As APIs existentes protegidas pela policy `PlatformAdmin` são:

| Método | Rota | Uso |
|---|---|---|
| GET | `/api/platform/tenants` | visão global de tenants |
| GET | `/api/platform/users` | usuários globais |
| GET | `/api/platform/clinics` | clínicas globais |
| GET | `/api/platform/onboarding/{tenantId}` | progresso e readiness do onboarding |
| POST | `/api/platform/tenants/{tenantId}/clinic-admins` | cria ClinicAdmin com `Idempotency-Key` |
| POST | `/api/platform/tenants/{id}/{activate|suspend|disable}` | status do tenant; ativação exige readiness mínimo |
| POST | `/api/platform/onboarding` | operação idempotente existente para tenant, clínica, unidade e ClinicAdmin |

O bootstrap não é editável por endpoint. Ele ocorre somente no startup e é
controlado por secrets/configuração. O `ClinicAdmin` não recebe policy
`PlatformAdmin` e não pode criar tenant global ou PlatformAdmin.
