# Clinic Assistant

Plataforma SaaS multiempresa para a operação administrativa de clínicas: cadastros, agenda, conversas, fila humana e integração WhatsApp. A solução aplica isolamento por tenant, processamento assíncrono com Inbox/Outbox, tempo real por SignalR e dados determinísticos para validação E2E.

## Stack

.NET 10, ASP.NET Core, EF Core, PostgreSQL, RabbitMQ, Redis, Serilog, OpenTelemetry, Next.js, React, TypeScript, Tailwind, Playwright e Docker Compose.

## Executar localmente

```bash
cp .env.example .env
docker compose up --build
```

- Frontend: `http://localhost:3000`
- API e Swagger: `http://localhost:8080/swagger`
- Liveness: `http://localhost:8080/health/live`
- Readiness: `http://localhost:8080/health/ready`
- RabbitMQ: `http://localhost:15672`

O `.env.example` contém somente valores de desenvolvimento. Não versione `.env`, tokens, senhas ou credenciais Twilio.

## Validar

```bash
dotnet restore backend/ClinicAssistant.sln
dotnet build backend/ClinicAssistant.sln --no-restore
dotnet test backend/ClinicAssistant.sln --no-build

cd frontend
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```

Para a validação E2E com dados determinísticos, siga o [guia de execução](docs/testing/e2e-execution-guide.md).

## Documentação

Consulte o [índice da documentação](docs/README.md). Os contratos HTTP são expostos pela [especificação OpenAPI em execução](docs/api/openapi.md); a [collection Postman](docs/postman/README.md) reúne os fluxos manuais mais usados.

## Estrutura

```text
backend/
  ClinicAssistant.sln
  Directory.Build.props
  Directory.Packages.props
  src/
    ClinicAssistant.Api            # Host HTTP e SignalR
    ClinicAssistant.Application    # Casos de uso e portas
    ClinicAssistant.Contracts      # DTOs e contratos públicos
    ClinicAssistant.Domain         # Núcleo de domínio
    ClinicAssistant.Infrastructure # EF Core, PostgreSQL e adaptadores
    ClinicAssistant.Worker         # Processamento assíncrono
  tests/                         # Testes unitários
  tools/                         # Ferramentas auxiliares
frontend/                         # Aplicação operacional Next.js
database/                         # Migrations, reset e seeds E2E
docs/                             # Documentação de produto e operação
```
