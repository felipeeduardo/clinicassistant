# Dashboard operacional

`GET /api/dashboard` exige `ClinicStaff` e retorna agregados apenas do tenant atual: consultas do dia, pendentes, fila humana, conversas ativas, mensagens em dead-letter na Outbox e estado da integração WhatsApp.
