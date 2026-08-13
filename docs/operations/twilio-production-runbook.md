# Runbook Twilio Production — IA Recepção

## Estado

Este runbook é preparatório. Configurar sender, webhook ou enviar mensagem real
exige os Gates D e E da Etapa 9.8.4.

## Configuração segura

- `Twilio__AccountSid` e `Twilio__AuthToken` somente no secret manager do backend.
- `Twilio__WhatsAppFrom` deve ser o sender aprovado para a conta de produção.
- `Twilio__IncomingWebhookBaseUrl` e `Twilio__StatusCallbackBaseUrl` devem usar
  `https://api.iarecepcao.com.br` após o TLS ser validado.
- `Twilio__TrustedProxyAddresses` deve conter apenas proxies conhecidos.
- `WhatsApp__TestRecipient` deve ser um único número de QA allowlisted.

## Validação

1. Confirmar integração Twilio habilitada no tenant correto.
2. Validar assinatura `X-Twilio-Signature` com um request controlado.
3. Confirmar inbound, Outbox, envio outbound e StatusCallback.
4. Verificar estados e auditoria sem expor payload sensível nos logs.
5. Em qualquer falha, desabilitar a integração e seguir o rollback.

Sandbox, Fake e ngrok continuam restritos a Development/Test/Pilot.
