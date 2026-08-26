# Deploy Railway — API e Worker

## Estado

`MANUAL ACTION REQUIRED`: nenhum projeto Railway, banco ou serviço cloud foi
criado nesta etapa.

## Serviços necessários

| Serviço | Imagem/comando | Porta/health |
|---|---|---|
| API | `infra/docker/api.Dockerfile` | `8080`, `/health/ready` |
| Worker | `infra/docker/worker.Dockerfile` | processo contínuo; monitorar restart |
| PostgreSQL | serviço gerenciado | conexão privada/TLS |
| Redis | serviço gerenciado | ping/health |
| RabbitMQ | broker compatível | conexão privada e DLQ |

API e Worker devem receber configurações equivalentes e secrets separados por
ambiente. O Worker não deve ser omitido: ele processa Outbox e entrega assíncrona.

## Ordem segura

1. Criar ambiente Preview/Pilot separado de Production.
2. Provisionar dependências e cadastrar variáveis pela matriz.
3. Aplicar migrations com o runbook específico.
4. Validar readiness, logs e reinício do Worker.
5. Executar smoke sem Twilio real.
6. Solicitar Gate B antes de domínio/API Production.

## Recuperação de senha

Antes de liberar a recuperação de senha em Production, configure no serviço
**API** `Email__Enabled=true`, `Email__Provider=SendGrid`,
`Email__FromAddress`, `Email__FromName`, `SendGrid__ApiKey`,
`SendGrid__RequestTimeoutSeconds` e `PasswordRecovery__FrontendBaseUrl`.

As credenciais devem ser adicionadas pelo painel **Variables/Secrets** do
Railway, nunca no Dockerfile, no repositório ou em variáveis `NEXT_PUBLIC_*`.

Para SendGrid, configure somente na **API**: `Email__Enabled`,
`Email__Provider=SendGrid`, `Email__FromAddress`, `Email__FromName`,
`SendGrid__ApiKey` e `SendGrid__RequestTimeoutSeconds`. O Worker não precisa
dessas variáveis nesta versão.
