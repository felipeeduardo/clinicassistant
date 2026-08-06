# WhatsApp e templates

| Método | Endpoint | Permissão | Idempotência | Descrição |
| --- | --- | --- | --- | --- |
| `GET` | `/api/whatsapp/integration/status` | `ClinicStaff` | Não | estado operacional e telefone mascarado |
| `GET` | `/api/whatsapp/integration/twilio/configuration` | `ClinicAdmin` | Não | configuração sanitizada, sem credenciais |
| `POST` | `/integration/validate`, `/enable`, `/disable` | `ClinicAdmin` | Não | ações de estado da integração |
| `POST` | `/integration/test-message` | `ClinicAdmin` | `Idempotency-Key` | solicita envio apenas ao destinatário permitido pelo ambiente |
| `GET/POST/PUT` | `/api/whatsapp/templates...` | `ClinicAdmin` | Não | lista, detalha, cria e atualiza templates |
| `POST` | `/templates/{id}/activate`, `/deactivate`, `/sync` | `ClinicAdmin` | Não | ativa, desativa ou solicita sincronização |

Webhooks Twilio são públicos somente para recebimento e StatusCallback, com validação de assinatura.

Webhooks Twilio são públicos somente para recebimento e StatusCallback, com validação de assinatura. Consulte [configuração Twilio](../whatsapp/twilio-setup.md), [webhooks](../whatsapp/twilio-webhooks.md) e o [smoke protegido](../operations/twilio-production-readiness.md).
