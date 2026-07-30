# Diagnóstico

Se o webhook retornar 401, confirme AuthToken, URL pública configurada e proxies confiáveis. Se o webhook não chegar, confirme POST e URLs do sender Twilio. Para mensagens pendentes, verifique Outbox, RabbitMQ, DLQ e logs do Worker.

Mídia não suportada ou acima do limite configura a conversa como `WaitingHuman`. Arquivos não são baixados nesta versão.
