# Métricas operacionais

As métricas são exportadas via OpenTelemetry/OTLP pela API e pelo Worker. Não possuem labels com telefone, nome de paciente, conteúdo de mensagens ou credenciais.

## Fila humana

- `human_queue_assigned_total`: atendimentos atribuídos a um operador.
- `human_queue_released_total`: atendimentos devolvidos à fila.
- `human_queue_transferred_total`: atendimentos transferidos entre operadores.
- `human_queue_completed_total`: atendimentos concluídos ao encerrar a conversa.

## Conversação

- `conversation_intent_total`: intents detectados, com tags não sensíveis `intent` e `flow`.
- `conversation_unknown_intent_total`: mensagens sem intent reconhecido.
- `conversation_invalid_input_total`: entradas inválidas após a normalização.
- `conversation_flow_started_total`, `conversation_flow_completed_total` e `conversation_flow_abandoned_total`: ciclo do fluxo.
- `conversation_flow_timeout_total`: fluxos reiniciados após expiração.
- `conversation_handoff_total`: encaminhamentos para atendimento humano.
- `conversation_messages_per_flow`: histograma de mensagens processadas por fluxo; pode ser agregado para obter médias operacionais.

## Outbox

- `outbox_published_total`: mensagens persistidas na Outbox e publicadas com sucesso no RabbitMQ.
- `outbox_failures_total`: tentativas de publicação que falharam.
- `outbox_dead_letters_total`: mensagens que atingiram o limite de tentativas e foram encaminhadas à dead-letter queue.

## WhatsApp e templates

- `whatsapp_template_synchronizations_total`: sincronizações de templates concluídas.
- `whatsapp_templates_synchronized_total`: quantidade de templates processados nas sincronizações.
- Os contadores de webhook, mensagens recebidas/enviadas, falhas e alterações de status permanecem no meter `ClinicAssistant.WhatsApp`.

## Operações administrativas

Estas métricas pertencem ao meter `ClinicAssistant.Operations`:

- `platform_onboarding_total`: onboardings de tenant persistidos e confirmados.
- `appointments_rescheduled_total`: reagendamentos concluídos.
- `appointment_conflicts_total`: tentativas de criar ou reagendar que encontraram conflito de agenda.
- `manual_messages_total`: mensagens manuais persistidas para envio pela Outbox.
- `audit_entries_total`: eventos de auditoria publicados pelo Hub SignalR.
- `signalr_connections_active`: conexões SignalR ativas; pode diminuir em reinicializações da API.
- `signalr_events_published_total`: eventos entregues pelo publicador SignalR.
- `signalr_publish_failures_total`: falhas no envio de eventos SignalR.
- `dashboard_requests_total` e `dashboard_request_duration`: quantidade e duração de consultas ao dashboard.
- `refresh_token_rotations_total`: rotações de refresh token concluídas.
- `refresh_token_reuse_detected_total`: uso de refresh token já revogado; não contabiliza expiração normal.
- `twilio_configuration_validations_total`: validações locais concluídas da integração Twilio.
- `twilio_configuration_failures_total`: validações rejeitadas por configuração incompleta ou provider incompatível.
- `authorization_denied_total`: desafios ou proibições retornados pela pipeline de autorização; não possui label de usuário ou tenant.
- `platform_onboarding_failures_total`: falhas durante o onboarding transacional de tenant.
- `whatsapp_template_sync_failures_total`: mensagens de sincronização inválidas ou processamentos rejeitados pelo Worker.

Alertas recomendados: aumento de `appointment_conflicts_total` acima do padrão da clínica, `signalr_publish_failures_total` diferente de zero e crescimento sustentado de falhas da Outbox.

Alertas recomendados: crescimento contínuo de `outbox_failures_total`, qualquer aumento de `outbox_dead_letters_total` e ausência prolongada de `outbox_published_total` quando houver tráfego esperado.
