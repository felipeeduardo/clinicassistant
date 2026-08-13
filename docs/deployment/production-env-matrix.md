# Matriz de variáveis por ambiente

Documento sem valores reais. Segredos devem ser cadastrados somente no secret
manager do respectivo provedor.

| Variável/grupo | Serviço | Dev/Test | Preview | Pilot | Production | Tipo |
|---|---|---|---|---|---|---|
| `NEXT_PUBLIC_SITE_URL` | Vercel | localhost | URL do Preview | landing do piloto | `https://iarecepcao.com.br` | Pública/build-time |
| `NEXT_PUBLIC_APP_URL` | Vercel | localhost | URL do Preview | app do piloto | `https://app.iarecepcao.com.br` | Pública/build-time |
| `NEXT_PUBLIC_API_URL` | Vercel | API local | API de Preview | API do piloto | `https://api.iarecepcao.com.br` | Pública/build-time |
| `NEXT_PUBLIC_BRAND_DOMAIN` | Vercel | `iarecepcao.com.br` | `iarecepcao.com.br` | domínio aprovado | domínio aprovado | Pública/build-time |
| `ConnectionStrings__Primary` | Railway API/Worker | Compose local | banco isolado | banco piloto | banco produção | Secret/runtime |
| `ConnectionStrings__Test` | Railway API/Worker | banco de testes | banco de testes | não configurar | não configurar | Secret/runtime |
| `Database__Target` | Railway API/Worker | `primary` ou `test` | `primary` isolado | `primary` | `primary` | Runtime |
| `Jwt__Issuer`, `Jwt__Audience` | Railway API/Worker | valores locais | valores Preview | valores Pilot | valores Production | Runtime |
| `Jwt__Secret` | Railway API/Worker | segredo local | secret separado | secret separado | secret forte/rotacionável | Secret/runtime |
| `Frontend__AllowedOrigins` | Railway API | localhost | URL Preview aprovada | landing/app Pilot | landing/app Production | Runtime |
| `RabbitMq__*` | Railway API/Worker | Compose local | broker isolado | broker gerenciado | broker gerenciado | Runtime/secret |
| `Redis__*` | Railway API/Worker | Compose local | Redis isolado | Redis gerenciado | Redis gerenciado | Runtime/secret |
| `WhatsApp__Provider` | Railway API/Worker | `Fake` ou Sandbox | `Fake` | Sandbox controlado | `Twilio` após Gate D | Runtime |
| `Twilio__AccountSid` | Railway API/Worker | somente Sandbox local | Sandbox isolado | Sandbox/conta aprovada | conta Production | Secret |
| `Twilio__AuthToken` | Railway API/Worker | secret local | secret Preview | secret Pilot | secret manager | Secret |
| `Twilio__WhatsAppFrom` | Railway API/Worker | sender Sandbox | sender Sandbox | sender aprovado | sender aprovado | Runtime |
| `Twilio__IncomingWebhookBaseUrl` | Railway API | ngrok | URL Preview HTTPS | API Pilot | `https://api.iarecepcao.com.br` | Runtime |
| `Twilio__StatusCallbackBaseUrl` | Railway API | ngrok | URL Preview HTTPS | API Pilot | `https://api.iarecepcao.com.br` | Runtime |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Railway API/Worker | opcional | observabilidade Preview | collector Pilot | collector Production | Runtime |
| `RabbitMq__Host` | Railway API/Worker | `rabbitmq` | host Preview | host Pilot | host CloudAMQP | Runtime |
| `RabbitMq__Port` | Railway API/Worker | `5672` | porta do broker | porta do broker | porta TLS fornecida | Runtime |
| `RabbitMq__Username` | Railway API/Worker | usuário local | usuário Preview | usuário Pilot | usuário CloudAMQP | Secret |
| `RabbitMq__Password` | Railway API/Worker | senha local | secret Preview | secret Pilot | secret manager | Secret |
| `RabbitMq__VirtualHost` | Railway API/Worker | `/` | vhost Preview | vhost Pilot | vhost CloudAMQP | Runtime |
| `RabbitMq__UseTls` | Railway API/Worker | `false` | conforme broker | conforme broker | `true` | Runtime |
| `RabbitMq__ServerName` | Railway API/Worker | vazio | hostname TLS, se usado | hostname TLS | hostname/SNI CloudAMQP | Runtime |

## Regras

- Nunca usar secrets de Production em Preview ou Test.
- Nenhuma variável `NEXT_PUBLIC_*` pode conter credenciais.
- `DATABASE__TARGET=test` é proibido em Production.
- A API e o Worker devem receber configurações compatíveis de banco, RabbitMQ,
  Redis, JWT e Twilio.
- Toda mudança de Production deve registrar responsável, data, motivo e rollback.
