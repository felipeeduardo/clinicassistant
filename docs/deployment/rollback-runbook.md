# Runbook de rollback

## Gatilhos

- readiness instável;
- erro 5xx persistente;
- Worker em crash loop ou Outbox em dead-letter;
- falha de assinatura/webhook Twilio;
- regressão de login, CORS ou SignalR;
- indisponibilidade ou corrupção percebida no banco.

## Procedimento

1. Registrar incidente, horário, versão, responsável e impacto.
2. Desabilitar o envio Twilio ou a integração afetada; não apagar evidências.
3. Reverter o frontend para o último deployment Vercel saudável.
4. Reverter API/Worker para a última imagem Railway saudável, respeitando
   compatibilidade de schema.
5. Restaurar PostgreSQL somente com aprovação e evidência do backup escolhido.
6. Validar health, login, uma operação sem envio real e filas.
7. Comunicar encerramento e abrir análise de causa raiz.

DNS, migrations e primeiro envio real exigem seus respectivos approval gates;
este runbook não autoriza essas ações automaticamente.
