# Estado conversacional

`ConversationState.ContextJson` persiste intenção, etapa anterior, contador de entradas inválidas, seleções de catálogo, confirmação pendente e timestamps. O estado continua protegido pelo lock distribuído, token de concorrência e Inbox/Outbox idempotentes.

Fluxos expiram conforme `Conversation:StateExpirationMinutes`; uma nova mensagem reinicia a navegação sem depender da memória do processo.
