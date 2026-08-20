# Administração da plataforma

As APIs existentes protegidas pela policy `PlatformAdmin` são:

| Método | Rota | Uso |
|---|---|---|
| GET | `/api/platform/tenants` | visão global de tenants |
| GET | `/api/platform/users` | usuários globais |
| GET | `/api/platform/clinics` | clínicas globais |
| GET | `/api/platform/onboarding/{tenantId}` | progresso e readiness do onboarding |
| GET | `/api/platform/tenants/{tenantId}/whatsapp` | estado seguro e somente leitura da integração |
| POST | `/api/platform/tenants/{tenantId}/clinic-admins` | cria ClinicAdmin com `Idempotency-Key` |
| POST | `/api/platform/tenants/{id}/{activate|suspend|disable}` | ciclo de vida global do tenant; ativação exige apenas clínica, unidade e ClinicAdmin |
| POST | `/api/platform/tenants/{tenantId}/purge` | exclusão permanente; exige e-mail de ClinicAdmin ativo e confirmação igual ao slug |
| POST | `/api/platform/onboarding` | provisionamento idempotente de tenant, clínica, unidade e ClinicAdmin |

A exclusão é irreversível e remove os dados operacionais, usuários, agenda,
conversas, mensagens, integrações, auditoria e o próprio tenant. O endpoint
recusa o tenant interno da plataforma e executa a remoção em transação.

Não existem mais endpoints `PlatformAdmin` para especialidades, profissionais,
disponibilidade, agenda, usuários internos ou WhatsApp operacional. Essas APIs
usam o tenant autenticado e exigem `ClinicAdmin` (ou a policy operacional
correspondente). O PlatformAdmin recebe `403` ao tentar executar essas mutações.

O bootstrap não é editável por endpoint. Ele ocorre somente no startup e é
controlado por secrets/configuração. O `ClinicAdmin` não recebe policy
`PlatformAdmin` e não pode criar tenant global ou PlatformAdmin.
