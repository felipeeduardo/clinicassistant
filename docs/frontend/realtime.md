# Tempo real operacional

O Hub SignalR está em `GET /hubs/operations` e exige JWT. O cliente usa o token de acesso somente durante a conexão e o servidor obtém o `tenant_id` exclusivamente das claims, adicionando a conexão ao grupo `tenant:{tenantId}`. O cliente não envia tenant como parâmetro.

## Eventos publicados

| Evento | Origem | Queries invalidadas |
| --- | --- | --- |
| `appointment.created` | Criação de consulta | `appointments`, `availability` |
| `appointment.updated` | Confirmação | `appointments`, `availability` |
| `appointment.cancelled` | Cancelamento | `appointments`, `availability` |
| `conversation.updated` | Entrada de mensagem e operações de conversa | `conversations`, `conversation`, `conversation-messages` |
| `whatsapp.inbound.received` | Mensagem de entrada já persistida | `conversations`, `conversation`, `conversation-messages`, `dashboard` |
| `whatsapp.message.status.changed` | StatusCallback persistido | `conversations`, `conversation`, `conversation-messages` |
| `whatsapp.template.*` | Administração ou sincronização de templates | `whatsapp-templates`, `whatsapp-template` |
| `queue.item.*` | Criação e operações da fila humana | `conversation-queue`, `conversations`, `conversation` |
| `audit.created` | Auditoria de conversa, catálogo, pacientes, agenda, plataforma, templates e integração WhatsApp | `audit`, catálogos administrativos |
| `dashboard.invalidated` | Alteração operacional relevante | `dashboard` |

Cada envelope contém `eventId`, deduplicado no cliente em memória. A reconexão automática do SignalR mostra o estado no cabeçalho da aplicação. Uma falha de conexão não substitui as queries HTTP, que continuam sendo a fonte de verdade.

As mensagens são emitidas somente depois do commit que persistiu a alteração. O payload do evento não inclui conteúdo de conversa, telefone, perfil WhatsApp ou credenciais; a interface usa as queries HTTP autenticadas como fonte de verdade.

## Cobertura a ampliar

`audit.created` é emitido pelos fluxos administrativos de conversa, catálogo, pacientes, agenda, plataforma e WhatsApp. Novos módulos administrativos devem seguir o mesmo padrão: salvar a auditoria e só então publicar o evento sanitizado.
