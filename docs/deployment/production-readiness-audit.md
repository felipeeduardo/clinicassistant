# Auditoria de prontidão para produção — Etapa 9.8.4

**Data da auditoria:** 2026-08-12  
**Escopo:** somente auditoria. Nenhum deploy, alteração de DNS, configuração de Twilio real ou criação de segredo foi executado.  
**Marca pública:** IA Recepção. Os nomes técnicos `ClinicAssistant` permanecem inalterados.

## Resultado executivo

O repositório possui uma base funcional consistente para ser promovida a um ambiente Pilot/Staging, mas **não está aprovado para produção**. O código local compila e os fluxos de WhatsApp, migrations e worker estão presentes; porém Vercel/Railway, domínios, TLS, banco gerenciado, observabilidade, backup e segredos de produção ainda não foram provisionados nem verificados.

### Classificação

| Nível | Situação |
|---|---|
| **Bloqueio crítico** | Não há implantação cloud verificada, nem store de segredos de produção, nem backup/restore comprovado. |
| **Risco alto** | `docker-compose.yml` contém defaults locais inseguros; CORS, cookies atrás de proxy, migrations e worker ainda precisam de validação no provedor escolhido. |
| **Pronto no código** | Build .NET Release sem erros; frontend já validado com typecheck, lint, testes unitários e build; health checks, migrations, Twilio webhook/status callback, assinatura, idempotência e Outbox existem. |
| **Aguardando aprovação** | Escolha de Vercel/Railway, domínios oficiais, plano de custos, sender Twilio real, política de retenção e janela de mudança. |

## Arquitetura encontrada

| Componente | Localização | Papel | Estado |
|---|---|---|---|
| Landing/app | `frontend/` | Next.js, landing pública e aplicação autenticada | Pronto para build; URL é embutida no build via `NEXT_PUBLIC_*`. |
| Backend HTTP | `backend/src/ClinicAssistant.Api/` | API, autenticação, webhooks e SignalR | Container em `8080`; migrations são executadas no startup. |
| Worker | `backend/src/ClinicAssistant.Worker/` | Outbox, RabbitMQ, envio e processamento de WhatsApp | Serviço separado obrigatório em produção. |
| Persistência | `backend/src/ClinicAssistant.Infrastructure/` | PostgreSQL, EF Core e migrations | 15 migrations encontradas até `202608030004_AppointmentVersion`; aplicação automática precisa de controle de concorrência no deploy. |
| Mensageria | RabbitMQ + Redis | Outbox/retry, locks, eventos e SignalR | Dependências obrigatórias; não devem ser substituídas por defaults locais. |
| WhatsApp | `backend/src/ClinicAssistant.Infrastructure/WhatsApp/` | Fake/Twilio, inbound, StatusCallback e templates | Código presente; sender, credenciais e URLs reais não estão configurados. |

## Auditoria por área

| Área | Evidência | Estado | Pendência de produção |
|---|---|---|---|
| Frontend | `frontend/Dockerfile`, `frontend/next.config.ts`, `frontend/.env.example` | **Parcialmente pronto** | Definir projeto Vercel, variáveis Preview/Production, domínio canônico e rebuild após cada mudança de `NEXT_PUBLIC_*`. |
| Backend HTTP | `infra/docker/api.Dockerfile`, `Program.cs` | **Parcialmente pronto** | Publicar API em HTTPS, configurar porta/health, limites, logs e política de restart no Railway. |
| Worker | `infra/docker/worker.Dockerfile`, `backend/src/ClinicAssistant.Worker/Services/` | **Parcialmente pronto** | Criar serviço Railway independente com as mesmas conexões do API e alertas para crash/retry/dead-letter. |
| Health | `Program.cs` (`/health/live`, `/health/ready`) | **Pronto no código** | Validar externamente live/readiness; readiness exige PostgreSQL, RabbitMQ e Redis disponíveis. |
| Banco/migrations | `backend/src/ClinicAssistant.Infrastructure/Persistence/Migrations/` | **Pronto no código; risco operacional** | Provisionar PostgreSQL, executar migrations com uma única estratégia, testar rollback e impedir corrida entre réplicas. |
| Redis | `Program.cs`, `Redis` options | **Dependência obrigatória** | Usar Redis gerenciado/privado, TLS e credenciais; validar latência e persistência conforme necessidade. |
| RabbitMQ | `MessagingWorker`, `RabbitMq` options | **Dependência obrigatória** | Definir broker gerenciado ou serviço Railway compatível, vhost/usuário, durabilidade, retenção e DLQ. |
| CORS | `Program.cs` (`Frontend:AllowedOrigins`) | **Pronto no código; não configurado para produção** | Allowlist somente para `https://iarecepcao.com.br` e `https://app.iarecepcao.com.br` (e Preview aprovado, se necessário). |
| Proxy/TLS | `UseForwardedHeaders`, `Twilio:TrustedProxyAddresses` | **Parcial** | Confirmar headers encaminhados pelo provedor, proxies confiáveis e `Secure` nos cookies; nunca liberar proxy arbitrário. |
| Autenticação | JWT + cookie `clinic_assistant_refresh` em `Program.cs` | **Parcial** | Secret forte e rotacionável, issuer/audience de produção, domínio SameSite/Secure e política de expiração validados no domínio final. |
| Twilio inbound | `Program.cs`, `TwilioWebhookUrlResolver`, `TwilioWebhookSignatureValidator` | **Pronto no código** | Configurar sender real, URL HTTPS estável, `X-Twilio-Signature`, allowlist de integração e teste controlado. |
| Twilio outbound/status | `TwilioWhatsAppGateway`, `WhatsAppStatusCallbackService` | **Pronto no código; não homologado em produção** | Confirmar `StatusCallback`, templates aprovados, opt-in, limites e observação de estados `queued/sent/delivered/failed`. |
| Idempotência/Outbox | `IdempotencyRecord`, `OutboxMessage`, consumers do Worker | **Pronto no código** | Testar redelivery, restart, dead-letter e replay sem duplicar mensagem. |
| Observabilidade | Serilog + OpenTelemetry | **Instrumentação presente** | Definir exporter OTLP, dashboards, alertas, retenção e sanitização no provedor. |
| Backup/DR | Não há evidência de serviço configurado no repositório | **Bloqueio crítico** | Política e teste de restauração do PostgreSQL, RPO/RTO, retenção e responsável. |
| CI/CD | `.github/workflows/ci.yml` e `manual-twilio-smoke.yml` | **Parcial** | Adicionar deploy protegido após aprovação; manter smoke real exclusivamente manual e com environment protegido. |
| DNS/TLS | `docs/operations/domain-iarecepcao-runbook.md` | **Não executado** | Criar/validar `iarecepcao.com.br`, `www`, `app` e `api`, certificados e redirect canônico. |

## Evidências técnicas verificadas

- `dotnet build backend/ClinicAssistant.sln --no-restore --configuration Release` concluído com **0 erros** (o solution emite apenas avisos de configuração `Release|Any CPU`).
- O frontend possui scripts de `typecheck`, `lint`, testes e `build`; as validações anteriores registradas nesta etapa passaram.
- A API expõe `/health/live` sem dependências e `/health/ready` com PostgreSQL, RabbitMQ e Redis.
- A API aplica `Database.MigrateAsync()` no startup. Isso é conveniente no local, mas deve ser substituído ou protegido por uma execução controlada no deploy com mais de uma réplica.
- O código possui endpoints inbound e StatusCallback Twilio, validação oficial da assinatura, limite de corpo e processamento idempotente.
- O Worker é um processo separado; publicar somente a API deixaria Outbox e entrega assíncrona sem consumidor.
- A pipeline atual executa documentação, backend e frontend. O smoke Twilio está em workflow manual protegido; os E2E não fazem parte do CI atual por decisão anterior.

## Auditoria de configuração e segredos

O arquivo `.env.example` contém apenas exemplos, mas seus valores default são explicitamente locais (`clinicassistant`, RabbitMQ local e segredo JWT de desenvolvimento). Esses defaults **não podem chegar a Vercel, Railway ou Twilio**.

### Variáveis públicas do frontend

| Variável | Production esperada | Observação |
|---|---|---|
| `NEXT_PUBLIC_SITE_URL` | `https://iarecepcao.com.br` | Vai para o bundle público. |
| `NEXT_PUBLIC_APP_URL` | `https://app.iarecepcao.com.br` | Vai para o bundle público. |
| `NEXT_PUBLIC_API_URL` | `https://api.iarecepcao.com.br` | Não deve conter token. |
| `NEXT_PUBLIC_BRAND_DOMAIN` | `iarecepcao.com.br` | Identidade pública IA Recepção. |

### Variáveis privadas do backend/worker

`ConnectionStrings__*`, `JWT_SECRET`, `RabbitMq__Password`, `Twilio__AuthToken`, `Twilio__AccountSid`, destinatário de smoke e credenciais Redis/RabbitMQ devem ser armazenados exclusivamente no secret manager do ambiente. Não devem ser colocados em `NEXT_PUBLIC_*`, no GitHub log, em Postman versionado ou em documentação.

## Gates externos ainda abertos

1. **Hosting:** escolher e aprovar Vercel para frontend e Railway (ou equivalente) para API, Worker, PostgreSQL, Redis e RabbitMQ.
2. **Domínio:** ter acesso DNS a `iarecepcao.com.br`, aprovar subdomínios e registrar TTL, responsável, evidência TLS e rollback.
3. **Produção:** gerar secrets únicos, configurar ambientes separados e remover todos os defaults locais.
4. **Dados:** criar banco vazio, aplicar migrations, configurar backup e executar restauração de prova sem dados de pacientes reais.
5. **Twilio:** confirmar conta, sender WhatsApp aprovado, templates, opt-in, webhook inbound e StatusCallback HTTPS; o Sandbox não é evidência de produção.
6. **Observabilidade:** escolher destino OTLP, alertas de readiness, crash do Worker, falhas Twilio, dead-letter, latência e erro 5xx.
7. **Go-live:** definir janela, responsável, checklist de smoke, destinatário allowlisted, plano de rollback e aprovação explícita antes de enviar qualquer mensagem real.

## Documentos que devem ser produzidos depois desta auditoria

Esta etapa inicial cria somente este relatório. Depois da aprovação dos gates, devem ser completados, sem incluir segredos:

- `docs/deployment/vercel-deployment.md`;
- `docs/deployment/railway-deployment.md`;
- `docs/deployment/production-env-matrix.md`;
- `docs/deployment/dns-and-domains.md`;
- `docs/deployment/rollback-runbook.md`;
- `docs/deployment/observability-production.md`;
- `docs/operations/twilio-production-runbook.md`;
- `docs/operations/database-backup-restore.md`;
- atualização de `docs/operations/domain-iarecepcao-runbook.md` e `docs/operations/twilio-production-readiness.md` com evidências reais.

## Decisão solicitada antes da próxima subetapa

Para sair da auditoria, é necessário aprovar explicitamente:

- provedor e plano de hospedagem;
- domínios e estratégia de DNS;
- banco/Redis/RabbitMQ gerenciados;
- política de segredos e backup;
- sender Twilio real e destinatário de smoke;
- janela de mudança e responsável pelo rollback.

Até essa aprovação, o ambiente suportado continua sendo local/Fake ou Sandbox, e a Etapa 9.8.4 permanece **em auditoria, não liberada para produção**.
