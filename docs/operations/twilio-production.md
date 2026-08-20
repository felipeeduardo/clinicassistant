# Twilio em produção

Em produção, configure `WhatsApp__Provider=Twilio` e os secrets `Twilio__AccountSid`, `Twilio__AuthToken` e `Twilio__WhatsAppFrom` no secret store do Railway. Configure também `Twilio__IncomingWebhookBaseUrl` e `Twilio__StatusCallbackBaseUrl` com HTTPS.

A API e o Worker falham rapidamente no startup quando o provider não é Twilio, quando faltam credenciais ou quando as URLs públicas não são HTTPS. A rotação de token ocorre exclusivamente no Railway; não há tela de credenciais no produto.

