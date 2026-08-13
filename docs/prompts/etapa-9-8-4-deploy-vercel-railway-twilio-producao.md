# Etapa 9.8.4 — Deploy Cloud, Domínio de Produção e Twilio Real

## Contexto

O projeto está entrando na fase de validação real do MVP **IA Recepção**.

Decisões já tomadas:

- Marca pública: **IA Recepção**
- Domínio registrado: `iarecepcao.com.br`
- Nome técnico interno: `ClinicAssistant`
- Frontend: Next.js/React
- Backend: ASP.NET Core/.NET
- Frontend deverá ser hospedado na **Vercel**
- Backend deverá ser hospedado na **Railway**
- WhatsApp em produção será configurado com **Twilio**
- O desenvolvimento local deve continuar suportando `localhost`, `ngrok` e `FakeWhatsAppGateway`
- O nome técnico `ClinicAssistant` NÃO deve ser renomeado nesta etapa

Esta etapa é crítica e deve priorizar segurança, configuração por ambiente, observabilidade, rollback e validação gradual.

## 1. Objetivo

Preparar e validar uma arquitetura de produção equivalente a:

```text
https://iarecepcao.com.br
        ↓
Landing pública — Vercel

https://app.iarecepcao.com.br
        ↓
Aplicação autenticada — Vercel

https://api.iarecepcao.com.br
        ↓
ASP.NET Core API — Railway
        ↓
PostgreSQL / Redis / demais dependências
        ↓
Twilio WhatsApp
```

A conclusão desta etapa significa **ambiente pronto para piloto controlado**, e não autorização automática para tráfego amplo em produção.

## 2. Regras de segurança

Nunca:

- versionar secrets;
- inserir tokens reais em `.env.example`;
- inserir secrets no frontend;
- exibir secrets em logs;
- copiar secrets para documentação;
- commitar `.env.production`;
- executar migration destrutiva sem análise;
- alterar DNS real silenciosamente;
- alterar configuração Twilio real sem gate de aprovação;
- desabilitar validação de assinatura de webhook;
- liberar CORS com `*` quando há credenciais;
- utilizar banco local em produção.

Use os marcadores:

`MANUAL ACTION REQUIRED` para ações externas manuais.

`APPROVAL REQUIRED` para decisões que dependem de autorização explícita.

## 3. Ordem da etapa

Executar:

1. Auditoria de deploy
2. Production readiness do código
3. Vercel Preview
4. Railway Backend
5. PostgreSQL / Redis / workers / migrations
6. Domínios
7. Vercel Production
8. Integração Frontend ↔ Backend
9. Twilio Production
10. Smoke/E2E
11. Observabilidade e backup
12. Rollback
13. Pilot readiness
14. Documentação final

## 4. Auditoria obrigatória

Antes de qualquer deploy, analisar o repositório.

### Frontend
Verificar:

- diretório raiz real do Next.js;
- versão Node;
- package manager;
- `package.json`;
- `next.config.*`;
- build command;
- variáveis `NEXT_PUBLIC_*`;
- API base URL;
- SignalR URL;
- auth/session;
- callbacks;
- metadata;
- assets;
- domains;
- CSP;
- cookies;
- redirects/rewrites;
- Playwright;
- scripts de build/test.

### Backend
Verificar:

- versão .NET;
- projeto startup;
- `Program.cs`;
- `appsettings.*`;
- health checks;
- Swagger/OpenAPI;
- PostgreSQL;
- migrations;
- Redis;
- workers/background jobs;
- SignalR;
- forwarded headers;
- CORS;
- auth;
- rate limiting;
- Twilio;
- FakeWhatsAppGateway;
- webhook validation;
- status callback;
- idempotência;
- port binding;
- graceful shutdown.

### Infra
Verificar:

- `Dockerfile`;
- `docker-compose`;
- Railway config;
- Vercel config;
- GitHub Actions;
- `.env.example`;
- PostgreSQL;
- Redis;
- storage;
- e-mail;
- observability;
- jobs.

Produzir:

| Componente | Estado atual | Production ready? | Problema | Ação |
|---|---|---:|---|---|

Criar `docs/deployment/production-readiness-audit.md`.

## 5. Estratégia de ambientes

Garantir separação entre:

```text
Development
Test/E2E
Preview
Pilot/Staging
Production
```

Não reutilizar secrets de Production em Preview.

Development deve continuar funcionando com:

```text
localhost
ngrok
FakeWhatsAppGateway
```

## 6. Vercel

Antes de deploy:

- confirmar root directory do frontend;
- framework detection;
- build command;
- Node;
- install command;
- branch de produção;
- Preview Deployments;
- environment variables.

Inventariar variáveis e classificá-las como:

```text
PUBLIC
SERVER ONLY
SECRET
BUILD TIME
RUNTIME
```

Nenhum secret deve estar em `NEXT_PUBLIC_*`.

Auditar especialmente Twilio, JWT, DB, Redis, SMTP e private keys.

Se houver secret exposto, registrar:

`CRITICAL — SECRET ROTATION REQUIRED`.

## 7. Vercel Preview

Executar primeiro um Preview Deployment quando houver acesso.

Validar:

- build Linux;
- Landing;
- login;
- assets;
- redirects;
- envs;
- CSP;
- mobile;
- erros JS.

Se o Codex não tiver acesso à conta Vercel, documentar os passos exatos como `MANUAL ACTION REQUIRED`.

## 8. Domínios Vercel

Planejar:

```text
iarecepcao.com.br
app.iarecepcao.com.br
```

Antes de configurar, verificar se Landing e App pertencem ao mesmo projeto Next.js.

Não assumir.

Para `www.iarecepcao.com.br`, configurar redirect canônico para `iarecepcao.com.br` usando um único mecanismo oficial.

## 9. Railway Backend

Auditar se o deploy será via Dockerfile ou build nativo.

Preferir a estratégia já testada pelo projeto.

Se houver Dockerfile, validar:

- multi-stage build;
- runtime image;
- restore/build/publish;
- usuário não-root quando possível;
- startup;
- sem `.env` dentro da imagem.

ASP.NET Core deve ouvir a porta fornecida pelo ambiente Railway. Não hardcode `localhost:5000`.

## 10. Health checks

Garantir rota real de saúde.

Preferência:

```text
/health
```

e, se necessário:

```text
/health/live
/health/ready
```

Configurar Railway Healthcheck com a rota real.

Não expor detalhes internos ou secrets.

## 11. PostgreSQL

Utilizar banco separado de Development/Test.

Auditar:

- connection string;
- SSL;
- pool;
- timeout;
- migrations;
- índices;
- timezone;
- backup;
- health.

Nunca reutilizar banco local.

## 12. Migrations

Criar `docs/deployment/database-migrations-runbook.md`.

Antes do primeiro deploy:

- listar pending migrations;
- classificar breaking/non-breaking;
- analisar downtime;
- planejar backup;
- evitar múltiplas instâncias executando migration em paralelo.

Não substituir por `EnsureCreated()`.

## 13. Redis

Se utilizado:

- usar instância gerenciada/Railway;
- secret por environment;
- retry;
- timeout;
- expiry;
- health;
- queue/backplane quando aplicável.

## 14. Workers/background jobs

Auditar processos necessários, por exemplo:

- Outbox worker;
- dispatcher;
- scheduler;
- queue consumer.

Criar:

| Processo | Necessário? | Serviço Railway | Start command | Health |
|---|---:|---|---|---|

Não assumir que a API executa tudo.

## 15. Storage

Verificar escrita em filesystem local.

Se houver uploads, exports ou attachments, documentar solução persistente antes de produção.

## 16. Railway variables

Criar `docs/deployment/production-env-matrix.md`.

Formato:

| Variable | Service | Environment | Secret? | Required? | Source | Notes |
|---|---|---|---:|---:|---|---|

Não colocar valores reais.

Serviços podem incluir:

```text
Vercel
Railway API
Railway Worker
```

## 17. Railway temporary domain

Antes de associar domínio customizado, validar:

- `/health`;
- auth;
- DB;
- Redis;
- SignalR;
- worker;
- uma rota pública segura.

Somente depois associar `api.iarecepcao.com.br`.

## 18. DNS

Arquitetura desejada:

```text
@      → Vercel
www    → Vercel/redirect
app    → Vercel
api    → Railway
```

Os valores reais devem vir dos dashboards dos provedores.

Atualizar `docs/operations/domain-iarecepcao-runbook.md`.

Não inventar A/CNAME/TXT.

## 19. HTTPS

Validar certificado para:

```text
iarecepcao.com.br
www.iarecepcao.com.br
app.iarecepcao.com.br
api.iarecepcao.com.br
```

Não avançar para webhook Twilio real antes de `api` estar em HTTPS válido.

## 20. Frontend ↔ Backend

Depois que `api.iarecepcao.com.br` estiver funcional:

- configurar API URL real na Vercel;
- redeploy quando a variável for build-time/public;
- remover URL Railway temporária da build final, se custom domain estiver aprovado.

## 21. CORS

Permitir explicitamente apenas origins necessários, por exemplo:

```text
https://iarecepcao.com.br
https://app.iarecepcao.com.br
```

Preview deve ser tratado separadamente.

Nunca utilizar `AllowAnyOrigin` com credentials.

## 22. Auth/cookies

Se usar cookies, validar:

- `Secure`;
- `HttpOnly`;
- `SameSite`;
- Domain;
- HTTPS;
- cross-subdomain behavior.

Se usar bearer token, preservar arquitetura atual e revisar refresh/session.

## 23. SignalR

Validar:

- WSS;
- auth;
- CORS;
- reconnect;
- URLs;
- scaling implications.

Executar teste real no ambiente cloud.

## 24. CSP

Validar CSP para os domínios reais, SignalR, assets e analytics aprovado.

Não usar `*` para resolver problema de integração.

## 25. Gate Twilio

Antes de qualquer alteração Twilio real:

`APPROVAL REQUIRED`

Pré-condições:

- API saudável;
- HTTPS;
- migrations concluídas;
- worker saudável;
- logs disponíveis;
- validação de assinatura implementada;
- idempotência testada.

## 26. Twilio secrets

Configurar somente no backend/Railway secret store:

```text
TWILIO_ACCOUNT_SID
TWILIO_AUTH_TOKEN
TWILIO_WHATSAPP_FROM
```

Usar os nomes reais existentes no projeto.

Nunca colocar no Vercel se o frontend não precisar.

Auditar histórico/logs/documentação. Em caso de exposição:

`SECRET ROTATION REQUIRED`.

## 27. Twilio inbound webhook

Configurar a rota REAL encontrada no projeto:

```text
https://api.iarecepcao.com.br/<rota-real-inbound>
```

Não inventar endpoint.

Confirmar método HTTP e route mapping.

## 28. Twilio status callback

Configurar:

```text
https://api.iarecepcao.com.br/<rota-real-status>
```

Validar os status realmente tratados pela implementação atual.

## 29. Validação `X-Twilio-Signature`

Em Production, validar assinatura Twilio.

Nunca desativar para “fazer funcionar”.

Auditar reverse proxy, forwarded proto/host e custom domain para garantir que a URL usada na validação corresponda à pública.

## 30. Idempotência

Webhook duplicado não pode gerar:

- duas mensagens;
- duas conversas;
- duas operações de agenda;
- duas respostas.

Usar external message id real, como MessageSid, conforme arquitetura existente.

## 31. Outbound

Preservar o fluxo atual:

```text
Application
↓
Outbox
↓
Worker
↓
Twilio
```

Não enviar diretamente do webhook/controller se a arquitetura usa Outbox.

## 32. Status de mensagens

Validar os status suportados pelo projeto e provider, por exemplo:

```text
queued
sent
delivered
read
failed
```

Não assumir sem verificar.

Persistir e publicar SignalR quando aplicável.

## 33. Templates

Auditar templates necessários para mensagens fora da janela permitida.

Documentar quais precisam estar:

```text
approved
active
synced
```

antes do piloto.

## 34. Teste real Twilio controlado

Primeiro teste:

```text
1 tester
1 tenant piloto
1 cenário
```

Fluxo sugerido:

```text
inbound
→ IA Recepção
→ disponibilidade
→ outbound
→ callback
→ histórico
```

Não abrir tráfego amplo.

## 35. Cenários reais Twilio

Testar controladamente:

1. inbound;
2. intent/menu;
3. disponibilidade;
4. outbound;
5. delivered;
6. read quando suportado;
7. failed;
8. duplicidade;
9. assinatura inválida;
10. handoff;
11. pause automation;
12. resume;
13. mensagem manual via Outbox;
14. isolamento multi-tenant.

## 36. CI

CI/E2E padrão deve continuar usando `FakeWhatsAppGateway`.

Nunca disparar Twilio real automaticamente por push.

## 37. Logs

Logs devem conter contexto operacional seguro:

- trace/correlation ID;
- tenant ID quando apropriado;
- conversation ID;
- external message ID;
- operation;
- result.

Não logar secrets, password, JWT completo ou PII desnecessária.

## 38. Métricas/alertas

Validar:

- API requests/errors;
- DB;
- inbound/outbound;
- callback failures;
- Outbox backlog;
- worker health;
- scheduling conflicts.

Alertas mínimos para piloto:

```text
API unavailable
healthcheck failing
message failures
outbox backlog
DB failure
worker down
```

## 39. Backup

Criar `docs/operations/database-backup-restore.md`.

Documentar:

- frequência;
- retenção;
- restore;
- owner.

Não declarar backup configurado sem verificar.

## 40. Seed/admin inicial

Não executar seed fake completo em Production.

Criar apenas tenant piloto/admin/configuração mínima por mecanismo seguro.

Não colocar passwords no repositório.

## 41. Pre-deploy validation

Usar apenas scripts reais existentes.

Frontend:

```text
lint
typecheck
unit
build
E2E fake
```

Backend:

```text
restore
build
unit/integration
```

## 42. Smoke Vercel

Validar:

- `/`;
- login;
- assets;
- metadata;
- mobile;
- sem erros JS críticos.

## 43. Smoke Railway

Validar:

- health;
- auth;
- DB;
- Redis;
- migrations;
- SignalR;
- worker.

## 44. E2E Cloud sem Twilio real

Executar contra o ambiente cloud:

```text
Landing
→ Login
→ Dashboard
→ Agenda
→ Pacientes
→ Conversas
```

com provider fake/controlado quando aplicável.

## 45. E2E Twilio real

Separado do E2E comum.

Registrar sem PII no repositório:

- timestamp;
- tester identificável apenas de forma segura;
- tenant;
- scenario;
- MessageSid;
- outcome;
- traceId.

## 46. Rollback

Criar `docs/deployment/rollback-runbook.md`.

Cobrir:

- Vercel rollback;
- Railway rollback;
- migrations;
- workers;
- env vars;
- Twilio webhook rollback;
- pause automation/handoff.

Não assumir que rollback de código desfaz migration destrutiva.

## 47. Feature flags

Auditar flags para:

```text
RealWhatsApp
FakeWhatsApp
Automation
Pilot
Pricing
Analytics
```

Provider não deve ser escolhido via hardcode.

## 48. CI/CD

Se Vercel/Railway já usam integração Git, evitar pipeline duplicado publicando a mesma coisa.

Documentar production branch e checks.

## 49. Documentação obrigatória

Criar/atualizar:

```text
docs/deployment/production-readiness-audit.md
docs/deployment/production-env-matrix.md
docs/deployment/vercel-deployment.md
docs/deployment/railway-deployment.md
docs/deployment/database-migrations-runbook.md
docs/deployment/rollback-runbook.md
docs/operations/domain-iarecepcao-runbook.md
docs/operations/twilio-production-runbook.md
docs/operations/database-backup-restore.md
docs/pilot/production-smoke-checklist.md
docs/pilot/twilio-real-test-checklist.md
```

Nunca documentar secret values.

## 50. Approval Gates

### Gate A — Vercel Production
Antes de associar domínio/promover Production:

`APPROVAL REQUIRED`

Apresentar status do Preview.

### Gate B — Railway Production
Antes de migration real/custom API domain:

`APPROVAL REQUIRED`

Apresentar health, migration plan, env completeness e worker.

### Gate C — DNS
Antes de aplicar DNS real:

`APPROVAL REQUIRED`

Apresentar records exatos fornecidos pelos provedores.

### Gate D — Twilio Production
Antes de alterar webhook/sender:

`APPROVAL REQUIRED`

Apresentar inbound/status routes exatas.

### Gate E — Primeiro envio real
Antes de mensagem real:

`APPROVAL REQUIRED`

Apresentar tester, tenant, cenário e conteúdo/template.

## 51. Critérios de aceite

### Frontend
- Vercel build PASS;
- Preview validado;
- Production HTTPS;
- Landing e login funcionando;
- API real configurada;
- CSP válida;
- mobile válido.

### Backend
- Railway deploy PASS;
- health PASS;
- DB PASS;
- Redis PASS quando necessário;
- migrations concluídas;
- worker PASS;
- SignalR PASS;
- custom domain HTTPS.

### Twilio
- secrets somente no backend;
- sender correto;
- inbound correto;
- callback correto;
- signature validation PASS;
- inbound/outbound PASS;
- delivery callback PASS;
- idempotência PASS;
- handoff PASS.

### Segurança
- CORS restrito;
- HTTPS;
- secrets auditados;
- DB separado;
- logs seguros;
- authorization/rate limiting sem regressão.

### Operação
- health;
- logs;
- rollback;
- backup;
- smoke checklist;
- pilot checklist.

## 52. Ordem prática recomendada

```text
AUDITORIA
↓
PRODUCTION HARDENING
↓
VERCEL PREVIEW
↓
RAILWAY API + DB + REDIS + WORKERS
↓
HEALTH / MIGRATIONS / SMOKE
↓
API CUSTOM DOMAIN
↓
CORS / SIGNALR
↓
VERCEL PRODUCTION ENV
↓
VERCEL PRODUCTION + DOMÍNIOS
↓
FULL CLOUD E2E SEM TWILIO REAL
↓
TWILIO PRODUCTION GATE
↓
WEBHOOK + STATUS CALLBACK
↓
TESTE REAL CONTROLADO
↓
OBSERVABILIDADE
↓
PILOT READY
```

O frontend pode ser criado primeiro na Vercel, mas a promoção final deve acontecer somente depois que a API Railway estiver funcional.

## 53. Não fazer

Não:

- renomear `ClinicAssistant`;
- hardcode production URLs;
- hardcode Twilio credentials;
- commitar `.env.production`;
- usar CORS permissivo;
- desabilitar assinatura Twilio;
- executar Twilio real em CI;
- usar seed fake completo em Production;
- executar migration destrutiva sem análise;
- publicar antes de healthcheck;
- assumir que deploy verde significa funcional;
- ocultar falha E2E;
- usar `test.skip` para liberar produção.

## 54. Relatório final

Apresentar:

1. arquitetura encontrada;
2. frontend root;
3. backend startup project;
4. estratégia Vercel;
5. Preview status;
6. estratégia Railway;
7. Railway temporary URL/status;
8. health;
9. DB;
10. migrations;
11. Redis;
12. workers;
13. custom domains;
14. DNS/manual actions;
15. env matrix;
16. CORS;
17. auth/cookies;
18. SignalR;
19. CSP;
20. Twilio inbound route;
21. Twilio status route;
22. signature validation;
23. idempotência;
24. Outbox;
25. templates;
26. secrets audit;
27. observabilidade;
28. backup;
29. smoke tests;
30. E2E cloud;
31. Twilio controlled test;
32. rollback;
33. approval gates;
34. riscos;
35. recomendação para Pilot;
36. ações manuais restantes.
