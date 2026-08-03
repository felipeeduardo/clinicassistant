# Collection Postman

Importe `clinic-assistant.postman_collection.json` e mantenha `baseUrl` em `http://localhost:8080` para desenvolvimento local. Execute **Login** primeiro: ele grava apenas `accessToken`; o refresh token é administrado pelo cookie `HttpOnly` do próprio Postman.

A pasta **Etapa 9.3 - Operações administrativas** reúne os fluxos de pacientes, agenda, conversas, fila humana, WhatsApp, auditoria, dashboard e administração de plataforma. Preencha os IDs de fixture ou os IDs retornados pelas operações de criação antes de executar requisições dependentes.

Não preencha, exporte ou versione cookies de sessão, senhas de produção, tokens, chaves do Twilio ou números de telefone reais.
