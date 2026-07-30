# Adição de provedores

Um futuro `MetaWhatsAppGateway` deve implementar `IWhatsAppGateway` em Infrastructure e ser selecionado por configuração. Contratos internos, entidades de domínio, Inbox, Outbox, consumidores e regras de tenant não devem depender de DTOs do provedor.

Webhooks de outro provedor devem ser traduzidos para `WhatsAppIncomingMessageReceived` antes da publicação no RabbitMQ.
