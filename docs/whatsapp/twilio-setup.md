# Configuração do Twilio

1. Configure um WhatsApp Sender no Console Twilio, sandbox ou sender aprovado.
2. Defina `TWILIO__ACCOUNT_SID`, `TWILIO__AUTH_TOKEN` e `TWILIO__WHATSAPP_FROM` somente por variáveis de ambiente seguras.
3. Informe uma URL HTTPS pública em `TWILIO__INCOMING_WEBHOOK_BASE_URL` e `TWILIO__STATUS_CALLBACK_BASE_URL`.
4. No sender, configure POST para os endpoints de mensagem e StatusCallback.

Use `whatsapp:` apenas no limite com o Twilio; telefones internos permanecem em E.164. Templates aprovados exigem `ContentSid` e são necessários fora da janela de 24 horas.
