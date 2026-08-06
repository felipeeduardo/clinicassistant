# Frontend operacional e E2E

O frontend utiliza apenas contratos existentes. O backend é a fonte de verdade para autorização, isolamento por tenant, concorrência e regras de negócio.

## Matriz tela x API

| Área | Rotas e capacidades | Acesso |
| --- | --- | --- |
| Clínica e catálogo | Clínica atual, unidades, especialidades e profissionais | Policies de catálogo `View` e `Manage` |
| Pacientes | Listagem, busca, detalhe, criação e edição | `Patients.View` e `Patients.Manage` |
| Agenda | Consultas, disponibilidade, confirmação, cancelamento e reagendamento | `ClinicStaff`; alterações administrativas conforme policy de profissional |
| Conversas | Listagem, detalhe, mensagens, fila humana e operações humanas | Leitura autenticada; operações administrativas com `ClinicAdmin` |
| WhatsApp | Status sanitizado, configuração Twilio, validação, ativação, teste e templates | `ClinicAdmin` |
| Auditoria e dashboard | Consulta de auditoria e indicadores do tenant | `ClinicAdmin` e `ClinicStaff`, respectivamente |
| Plataforma | Tenants, usuários, clínicas e onboarding idempotente | `PlatformAdmin` |

As mutations concorrentes de agenda e conversa enviam `expectedVersion`. Criação de consulta, mensagem manual, onboarding e teste WhatsApp usam `Idempotency-Key` quando o contrato o exige.

## Tempo real

O Hub autenticado está em `/hubs/operations`. A conexão é vinculada ao `tenant_id` das claims e o frontend invalida queries HTTP específicas ao receber eventos sanitizados. Consulte [tempo real operacional](realtime.md).

## Cenários E2E

O perfil `e2e` fornece dados determinísticos e cobre login, isolamento de tenant, cadastros, agenda, conversas, Fake WhatsApp, administração de plataforma e atualizações por SignalR. O roteiro e os comandos estão no [guia Playwright](../testing/playwright.md) e no [guia E2E](../testing/e2e-execution-guide.md).

## Limites intencionais

- O frontend nunca recebe Auth Token Twilio, Account SID completo, destinatário de teste ou payloads sensíveis de webhook.
- Eventos SignalR não são fonte de dados; as queries HTTP autenticadas continuam sendo a fonte de verdade.
- A integração Twilio real requer o checklist de [prontidão operacional](../operations/twilio-production-readiness.md).
