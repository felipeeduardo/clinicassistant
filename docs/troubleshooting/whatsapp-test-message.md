# Diagnóstico da mensagem de teste

Verifique nesta ordem: integração Connected, `WhatsApp:TestRecipient`, `Idempotency-Key`, Outbox, RabbitMQ, Worker e logs do gateway. O endpoint apenas grava `ConversationMessage` e `OutboxMessage`; a entrega é responsabilidade do Worker.

O frontend preserva mensagens genéricas para detalhes internos, mas traduz códigos conhecidos (`invalid_operation`, `resource_not_found`, `unauthorized` e `scheduling_conflict`) e exibe o `traceId` para correlação nos logs. Se a operação retornar `invalid_operation`, valide a configuração e habilite a integração antes de tentar novamente. Nunca habilite Twilio ou execute smoke real automaticamente.
