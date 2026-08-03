# Clinic AI Assistant

Fundação do SaaS multiempresa para atendimento administrativo de clínicas via WhatsApp. Esta entrega corresponde à **Etapa 1**: estrutura .NET 10, infraestrutura local, observabilidade e endpoints operacionais.

## Stack

.NET 10, ASP.NET Core, EF Core, PostgreSQL, RabbitMQ, Redis, Serilog, OpenTelemetry, Docker Compose e Swagger.

## Estrutura

```text
src/
  ClinicAssistant.Api            # Host HTTP
  ClinicAssistant.Application    # Casos de uso e portas
  ClinicAssistant.Contracts      # Contratos públicos
  ClinicAssistant.Domain         # Núcleo de domínio
  ClinicAssistant.Infrastructure # Persistência e adaptadores
  ClinicAssistant.Worker         # Processamento em segundo plano
tests/
  ClinicAssistant.UnitTests
infra/docker/
docs/
```

## Executar

```bash
cp .env.example .env
docker compose up --build
```

- API/Swagger: `http://localhost:8080/swagger`
- Liveness: `http://localhost:8080/health/live`
- Readiness: `http://localhost:8080/health/ready`
- RabbitMQ: `http://localhost:15672`

O `.env.example` contém apenas credenciais de desenvolvimento. Não versione `.env` nem segredos reais.

O RabbitMQ pode levar cerca de um minuto na primeira inicialização, enquanto cria os dados persistentes. O Docker Compose aguarda esse período antes de considerar o health check do broker.

## Próximas etapas

A Etapa 2 introduz `Tenant`, `User`, JWT, refresh token rotativo, contexto do tenant, filtros globais EF e a primeira migration. A modelagem de cadastros e agenda permanece deliberadamente fora desta fundação.

Consulte [arquitetura](docs/architecture.md) e [desenvolvimento](docs/development.md) para mais detalhes.

Consulte também a documentação de [mensageria](docs/messaging.md), incluindo os fluxos de outbox, inbox, retry e dead-letter queue.

## Postman

Importe a collection [clinic-assistant.postman_collection.json](docs/postman/clinic-assistant.postman_collection.json) no Postman. Ela usa `http://localhost:8080` por padrão e salva o access token devolvido por registro, login e refresh. O refresh token fica somente no cookie `HttpOnly` do Postman; não o copie para variáveis ou arquivos.

## Política de dependências

Os avisos do compilador são tratados como erros. As versões centralizadas devem permanecer em linhas estáveis e sem vulnerabilidades conhecidas pelo NuGet; não reduza essa proteção para contornar falhas de restore.
