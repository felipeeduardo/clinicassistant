# Checklist de smoke do ambiente Pilot/Production

- [ ] Deploy identificado por commit e horário.
- [ ] `GET /health/live` retorna 200.
- [ ] `GET /health/ready` retorna 200 com PostgreSQL, Redis e RabbitMQ.
- [ ] Landing e login carregam pelo domínio HTTPS.
- [ ] CORS aceita somente as origens aprovadas.
- [ ] Refresh cookie está `HttpOnly`, `Secure` e com domínio/política esperados.
- [ ] Uma consulta de leitura funciona com usuário de QA.
- [ ] SignalR conecta e reconecta.
- [ ] Worker está ativo e sem dead-letter inesperado.
- [ ] Logs não expõem secrets, tokens ou payloads sensíveis.
- [ ] Rollback e backup estão disponíveis antes de qualquer promoção.

Este checklist não substitui o Gate E nem autoriza mensagem Twilio real.
