# Configuração do Twilio

1. Configure um WhatsApp Sender no Console Twilio, sandbox ou sender aprovado.
2. Defina `TWILIO__ACCOUNT_SID`, `TWILIO__AUTH_TOKEN` e `TWILIO__WHATSAPP_FROM` somente por variáveis de ambiente seguras.
3. Informe uma URL HTTPS pública em `TWILIO__INCOMING_WEBHOOK_BASE_URL` e `TWILIO__STATUS_CALLBACK_BASE_URL`.
4. No sender, configure POST para os endpoints de mensagem e StatusCallback.

## Administração segura

As credenciais Twilio são de nível de plataforma e devem permanecer somente no secret manager ou em variáveis de ambiente seguras. O Clinic Assistant não persiste `AuthToken` no banco e a interface em `/settings/integrations/twilio` nunca recebe, registra ou exibe esse valor. Ela mostra apenas indicadores mascarados e checks sanitizados para administradores da clínica.

Para rotação, atualize o secret do ambiente, reinicie ou recarregue a aplicação conforme a plataforma e execute **Validar configuração**. Não use o navegador, `localStorage` ou campos administrativos para transportar credenciais.

Use `whatsapp:` apenas no limite com o Twilio; telefones internos permanecem em E.164. Templates aprovados exigem `ContentSid` e são necessários fora da janela de 24 horas.

Para validar produção de forma controlada, siga o [checklist de prontidão e smoke real](../operations/twilio-production-readiness.md). Não dispare mensagens reais pelo CI comum ou por pull requests.
