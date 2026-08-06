# Formulários administrativos

Esta página descreve somente formulários sustentados por contratos existentes ou planejados de forma explícita. Schemas completos pertencem ao [OpenAPI](../api/openapi.md).

| Formulário | Campos obrigatórios | Endpoint e permissão | Validações e erros | Resultado |
| --- | --- | --- | --- | --- |
| Clínica | razão social, nome fantasia, documento, e-mail, telefone, fuso | `PUT /api/clinics/current` — `Clinics.Manage` | formato de e-mail/documento, fuso válido; `400` para payload inválido | atualiza a clínica do tenant da sessão |
| Unidade | nome, endereço, telefone | `POST/PUT /api/units` — `Units.Manage` | campos obrigatórios; `409`/`400` conforme regra de dependência | cria ou atualiza unidade; horários são geridos separadamente |
| Horários da unidade | dia, abertura, fechamento | `PUT /api/units/{id}/business-hours` — `Units.Manage` | abertura anterior ao fechamento; dias sem duplicidade | substitui os horários da unidade |
| Especialidade | nome; descrição opcional | `POST/PUT /api/specialties` — `Specialties.Manage` | nome único no tenant; `409` em duplicidade | cria ou atualiza especialidade |
| Profissional | unidade, nome, e-mail, telefone, registro, especialidades | `POST/PUT /api/professionals` — `Professionals.Manage` | unidade e especialidades do tenant; registro único no tenant | cria ou atualiza profissional |
| Disponibilidade | dia, início, fim, duração do slot, ativo | `POST/PUT /api/professionals/{id}/availability...` — `Professionals.Manage` | intervalo válido, duração positiva | adiciona ou substitui regras; bloqueios e férias são formulários separados |
| Bloqueio/férias | início, fim; motivo opcional | `POST /blocks` ou `POST /vacations` — `Professionals.Manage` | início anterior ao fim; profissional do tenant | bloqueia agenda ou registra férias |
| Paciente | nome, telefone, consentimento; e-mail e nascimento opcionais | `POST/PUT /api/patients` — `Patients.Manage` | telefone único no tenant; `400`/`409` para dados inválidos ou duplicados | cria ou atualiza dado administrativo do paciente |
| Agendamento | unidade, profissional, especialidade, paciente, início, fim, origem; observação opcional | `POST /api/appointments` — `ClinicStaff` | slot disponível; `Idempotency-Key`; `409` em conflito | cria consulta pendente |
| Operação de consulta | versão esperada; motivo para cancelamento; novo intervalo para reagendamento | `POST .../confirm`, `cancel`, `reschedule` — `ClinicStaff` | `Idempotency-Key`; `expectedVersion` atual; `409` em concorrência/conflito | confirma, cancela ou gera substituição |
| Conversa humana | versão esperada; destinatário para transferência; conteúdo para mensagem manual | `/api/conversations/{id}/...` — `ClinicAdmin` | versão atual; usuário do mesmo tenant; mensagem manual requer `Idempotency-Key` | altera fila/automação ou enfileira mensagem na Outbox |
| Integração WhatsApp | não recebe credenciais no navegador | validar, habilitar, desabilitar e testar em `/api/whatsapp/integration/...` — `ClinicAdmin` | configuração mantida no servidor; teste requer `Idempotency-Key` | altera o estado operacional, sem expor segredos |
| Template WhatsApp | Content SID, nome, idioma, categoria, variáveis | `POST/PUT /api/whatsapp/templates` — `ClinicAdmin` | Content SID e campos obrigatórios; `400`/`409` conforme contrato | cria ou atualiza template; ativação e sync são ações próprias |

## Formulário de onboarding de tenant

O único fluxo atual de criação de empresa é `POST /api/platform/onboarding`, para `PlatformAdmin`, com `Idempotency-Key`. O wizard deve separar os dados abaixo, mesmo que o endpoint seja transacional:

1. **Tenant:** nome e slug.
2. **Clínica:** razão social, nome fantasia, documento, e-mail, telefone e fuso.
3. **Unidade inicial:** nome, endereço e telefone.
4. **Administrador:** nome, e-mail e senha temporária.
5. **Integração:** criada como Fake desabilitada; configuração posterior é administrativa.
6. **Revisão:** resumo de dados, confirmação e chave de idempotência.

Os campos de plano, locale, limites operacionais e início de vigência não possuem contrato atual; consulte [endpoints administrativos ausentes](../api/missing-administrative-endpoints.md).

## Regras de interface

- Exibir `400`, `401`, `403`, `404` e `409` por meio de mensagens acionáveis, preservando os dados digitados quando possível.
- Reconsultar detalhe após operações com `expectedVersion` e nunca repetir automaticamente uma mutation sem manter a mesma chave de idempotência.
- Não coletar diagnóstico, sintomas, prontuário, prescrição ou conteúdo clínico no cadastro administrativo de paciente.
- Não exibir nem persistir Auth Token Twilio, refresh token, sender real ou destinatário de teste.
