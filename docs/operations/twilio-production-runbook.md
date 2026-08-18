# Runbook Twilio Production — IA Recepção

## Ordem de execução

1. No Twilio Console, conclua o cadastro do WhatsApp Sender de produção e a
   aprovação da conta Meta/WhatsApp. O Sandbox não é usado em produção.
2. Anote o Account SID, o Auth Token e o número aprovado no formato E.164.
   O valor de `Twilio__WhatsAppFrom` deve ser `whatsapp:+<número-aprovado>`.
3. No Railway, cadastre as variáveis abaixo **na API e no Worker**. Credenciais
   ficam como secrets e nunca no frontend.

```env
WhatsApp__Provider=Twilio
WhatsApp__TestRecipient=+<número-único-de-qa>
Twilio__AccountSid=ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
Twilio__AuthToken=<secret-do-twilio>
Twilio__WhatsAppFrom=whatsapp:+<sender-aprovado>
Twilio__MessagingServiceSid=
Twilio__IncomingWebhookBaseUrl=https://api.iarecepcao.com.br
Twilio__StatusCallbackBaseUrl=https://api.iarecepcao.com.br
Twilio__StatusCallbackUrl=https://api.iarecepcao.com.br/api/webhooks/whatsapp/twilio/status/<integration-key>
Twilio__RequestTimeoutSeconds=15
Twilio__SignatureValidationEnabled=true
```

4. No painel Twilio, em **Messaging → Senders → WhatsApp Senders**, configure
   o webhook de entrada como `POST` para
   `https://api.iarecepcao.com.br/api/webhooks/whatsapp/twilio/<integration-key>`.
   Configure o Status Callback como `POST` para a URL correspondente acima.
   A URL precisa conter a mesma `IntegrationKey` da integração persistida no
   tenant.
5. No frontend autenticado, entre como `ClinicAdmin`, abra **Integrações →
   WhatsApp**, clique em **Validar configuração** e depois em **Habilitar**.
   A integração persistida deve estar no tenant correto, com provider `Twilio`
   e `WhatsAppFrom` igual ao sender aprovado.
6. Faça um teste controlado para o único `WHATSAPP__TEST_RECIPIENT` permitido.
   Confirme inbound, Outbox, envio outbound e StatusCallback antes de liberar
   qualquer uso comercial.

O cadastro do sender e a associação Meta são etapas externas: a aplicação não
 cria nem aprova números no Twilio.

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
