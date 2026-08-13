# Observabilidade em produção

## Sinais mínimos

| Sinal | Fonte | Alerta inicial |
|---|---|---|
| Disponibilidade | `/health/live` e `/health/ready` | qualquer indisponibilidade sustentada |
| Erros HTTP | Serilog/ASP.NET | aumento de 5xx ou 401/403 inesperado |
| Worker | logs e métricas Outbox | crash loop, retries e dead-letter |
| WhatsApp | métricas e StatusCallback | falhas de assinatura/envio e callback atrasado |
| Banco/filas | health e métricas do provedor | conexões esgotadas, latência ou fila crescente |
| SignalR | logs de conexão | falha de conexão/reconexão no app |

Configurar exporter OTLP somente com endpoint e credenciais no ambiente. Sanitizar
tokens, Auth Token, cookies, conteúdo de mensagens e dados pessoais antes da coleta.
Definir retenção e acesso mínimo no provedor de observabilidade.
