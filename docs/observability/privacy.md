# Privacidade e cardinalidade

Permitidos em traces/logs: IDs técnicos (tenant, clínica, conversa, consulta),
correlation/trace/span ID, operação, status e tipo. Nunca exportar nome,
telefone, e-mail completo, conteúdo de mensagem, dados clínicos, senha, hash,
tokens, AuthToken Twilio, API Key SendGrid, header OTLP, connection string ou
parâmetros SQL.

IDs técnicos e correlation não devem ser labels de métricas. Use apenas
dimensões de baixa cardinalidade como `provider`, `operation`, `status`,
`direction` e `reminder.type`.
