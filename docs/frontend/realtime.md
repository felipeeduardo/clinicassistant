# Tempo real operacional

O Hub SignalR está em `GET /hubs/operations` e exige JWT. O cliente usa o token de acesso somente durante a conexão e o servidor obtém o `tenant_id` exclusivamente das claims, adicionando a conexão ao grupo `tenant:{tenantId}`. O cliente não envia tenant como parâmetro.

## Eventos publicados

| Evento | Origem | Queries invalidadas |
| --- | --- | --- |
| `appointment.created` | Criação de consulta | `appointments`, `availability` |
| `appointment.updated` | Confirmação | `appointments`, `availability` |
| `appointment.cancelled` | Cancelamento | `appointments`, `availability` |
| `conversation.updated` | Leitura, assumir, liberar, pausar ou retomar | `conversations`, `conversation`, `conversation-messages` |

Cada envelope contém `eventId`, deduplicado no cliente em memória. A reconexão automática do SignalR mostra o estado no cabeçalho da aplicação. Uma falha de conexão não substitui as queries HTTP, que continuam sendo a fonte de verdade.

## Eventos aguardando backend

`whatsapp.integration.updated` e `dashboard.invalidated` já possuem mapeamento de cache no frontend, mas ainda não são publicados, pois não há operações administrativas que alterem esses recursos. Templates, auditoria, fila humana, transferência, envio manual, encerramento e reabertura continuam sem eventos e endpoints.
