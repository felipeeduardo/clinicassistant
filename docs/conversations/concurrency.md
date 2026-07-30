# Concorrência e idempotência

```mermaid
sequenceDiagram
  participant C as Consumer
  participant R as Redis
  participant P as PostgreSQL
  C->>R: SET lock tenant/conversation token NX EX
  R-->>C: lock
  C->>P: consulta ConversationProcessedMessage
  C->>P: atualiza State e Version
  C->>P: insere resposta, Outbox e idempotência
  P-->>C: commit único
  C->>R: DEL somente se token corresponder
```

Redis coordena o processamento temporariamente, mas PostgreSQL é a fonte de verdade. `Conversation` e `ConversationState` usam `Version` como concurrency token. A chave única `TenantId + ConversationMessageId` impede uma segunda resposta quando o evento é reenviado.
