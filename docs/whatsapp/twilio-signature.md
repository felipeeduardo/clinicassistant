# Assinatura Twilio

A URL assinada é reconstruída a partir da origem HTTPS configurada e dos parâmetros de formulário. A assinatura é validada pelo validador oficial Twilio. Quando há proxy, configure apenas endereços confiáveis em `TWILIO__TRUSTED_PROXY_ADDRESSES__0` e índices seguintes.

Requisições com assinatura inválida retornam 401 e não criam Inbox ou Outbox.
