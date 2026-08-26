# Runbook de e-mail

1. Autentique o domínio no SendGrid e confirme DNS.
2. Crie uma API Key com apenas `Mail Send`.
3. Cadastre as variáveis `Email__*` e `SendGrid__*` no serviço **API** do Railway.
4. Mantenha `Email__Enabled=false` durante o deploy inicial; valide readiness e
   logs.
5. Ative `Email__Enabled=true` e solicite recuperação para uma conta de QA.
6. Confirme recebimento, validade do link, reset e login; tente reutilizar o
   token e confirme que ele foi invalidado.

Falhas do provedor retornam erro interno controlado e são registradas apenas
com provedor, tipo, domínio do destinatário e status HTTP. Não repita a chave em
logs. Para rollback, desative o envio e preserve os tokens/auditoria.
