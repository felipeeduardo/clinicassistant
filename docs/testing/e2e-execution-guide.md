# Guia de Execução E2E

## 1. Pré-requisitos

- Docker Desktop e Docker Compose.
- .NET SDK 10.
- `dotnet-ef` disponível para aplicar migrations manualmente.
- Node.js apenas para executar o frontend fora do Docker.
- Cliente `psql` para usar os scripts locais.
- PostgreSQL acessível em `localhost:5432` ou pelas variáveis `DATABASE_*`.
- Migrations disponíveis em `src/ClinicAssistant.Infrastructure/Persistence/Migrations`.
- Scripts em `scripts/test-data` e seeds em `database/seeds`.

## 2. Variáveis obrigatórias

Use um arquivo local não versionado, como `.env`, ou exporte as variáveis no terminal.

```env
ASPNETCORE_ENVIRONMENT=Test
ALLOW_TEST_DATA_RESET=true
E2E_BASE_DATE=2026-08-03
E2E_DEFAULT_PASSWORD=
POSTGRES_DB=clinicassistant_test
POSTGRES_USER=clinicassistant
POSTGRES_PASSWORD=
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=clinicassistant_test
DATABASE_USER=clinicassistant
DATABASE_PASSWORD=
```

- `ALLOW_TEST_DATA_RESET=true` é obrigatório para reset.
- O nome do banco deve conter `test`, `e2e` ou `dev`. Alternativamente, defina `TEST_DATA_ALLOWED_DATABASES` para um banco local conhecido.
- Defina `E2E_DEFAULT_PASSWORD` no ambiente; ela nunca é salva em texto puro no banco. Se não houver uma senha personalizada, use `ClinicAssistant-E2E-Only-2026`, que é o padrão do script local. O mesmo valor deve ser usado no seed e no Playwright.
- Para o smoke Fake WhatsApp local, use o destinatário determinístico do seed: `export WHATSAPP_TEST_RECIPIENT=+550000000301`. Não use o número real do Twilio nesse cenário; ele deve ficar reservado para a configuração de produção.
- Não adicione segredos, credenciais reais ou telefones reais.
- Nunca execute reset em produção.

> Ao mudar `POSTGRES_DB` em uma instalação já criada, o volume do PostgreSQL pode continuar usando o banco antigo. Confirme com `docker compose ps` e recrie o volume apenas se for seguro fazê-lo.

## Banco de testes local

Em instalações novas, o Compose cria `clinicassistant` e também `clinicassistant_test` pelo script `database/init/01-create-test-database.sql`. Esse script só é executado na primeira inicialização do volume PostgreSQL.

Se o volume já existir, crie o banco de testes uma única vez sem apagar os dados existentes:

```bash
docker compose exec postgres createdb -U clinicassistant clinicassistant_test
```

> No Compose atual, `test-data-seeder` usa ambiente `Test`, mas `api` e `worker` estão fixados em `Development`. A variável exportada no terminal não altera esses dois containers.

## 3. Primeira execução

### 3.1 Subir infraestrutura

```bash
docker compose up -d postgres redis rabbitmq
```

### 3.2 Restaurar e validar backend

```bash
dotnet restore ClinicAssistant.sln
dotnet build ClinicAssistant.sln --no-restore
dotnet test ClinicAssistant.sln --no-restore
```

### 3.3 Aplicar migrations

A API aplica migrations automaticamente ao iniciar. Para aplicá-las manualmente:

```bash
ConnectionStrings__Default="Host=localhost;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" \
dotnet ef database update \
  --project src/ClinicAssistant.Infrastructure/ClinicAssistant.Infrastructure.csproj \
  --startup-project src/ClinicAssistant.Api/ClinicAssistant.Api.csproj
```

Ou suba a API e aguarde sua inicialização:

```bash
docker compose up -d api
docker compose logs -f api
```

### 3.4 Preparar dados E2E

```bash
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

### 3.5 Subir a aplicação

```bash
docker compose up -d api worker frontend
```

### 3.6 Verificar containers

```bash
docker compose ps
```

### 3.7 Verificar logs

```bash
docker compose logs -f api worker
```

## 4. Comando completo para copiar e executar

O `.env` padrão do projeto aponta para o banco principal `clinicassistant`, usado pelo smoke do Twilio. Para E2E, não altere esse arquivo permanentemente: sobrescreva o destino no próprio comando com `DATABASE_TARGET=test`, `POSTGRES_DB=clinicassistant_test` e `DATABASE_NAME=clinicassistant_test`. O bloco interrompe na primeira falha.

```bash
set -e
set -a
source .env
set +a

export ASPNETCORE_ENVIRONMENT=Test
export ALLOW_TEST_DATA_RESET=true
export TEST_DATA_CONFIRM=YES
export E2E_BASE_DATE=2026-08-03
export DATABASE_NAME="$POSTGRES_DB"
export DATABASE_HOST=localhost
export DATABASE_PORT="${POSTGRES_PORT:-5432}"
export DATABASE_USER="$POSTGRES_USER"
export DATABASE_PASSWORD="$POSTGRES_PASSWORD"
export TEST_DATA_ALLOWED_DATABASES="$POSTGRES_DB"

case "$POSTGRES_DB" in
  *test*|*Test*|*e2e*|*E2E*|*dev*|*Dev*) ;;
  *) echo "POSTGRES_DB must be a test/development database." >&2; exit 1 ;;
esac

docker compose up -d --build postgres redis rabbitmq api

attempt=0
until curl --fail --silent http://localhost:${API_PORT:-8080}/health/live >/dev/null; do
  attempt=$((attempt + 1))
  if [ "$attempt" -ge 30 ]; then
    docker compose logs --tail=100 api
    exit 1
  fi
  echo "Waiting for API migrations..."
  sleep 2
done

dotnet restore ClinicAssistant.sln
dotnet build ClinicAssistant.sln --no-restore
dotnet test ClinicAssistant.sln --no-restore

docker compose --profile e2e run --rm --build test-data-seeder e2e

docker compose up -d --build worker frontend
docker compose ps
```

Neste fluxo, a API aplica migrations por `Database.MigrateAsync()` e o `test-data-seeder` executa reset, seed e validação. Use `dotnet ef database update` apenas quando for necessário aplicar migrations manualmente.

## 5. Uso diário

Quando o schema e a infraestrutura já estiverem prontos:

```bash
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
docker compose up -d
```

Use este fluxo apenas quando migrations já tiverem sido aplicadas e o objetivo for reiniciar os dados E2E.

## 6. Ambiente mínimo

```bash
./scripts/test-data/reset.sh minimal
./scripts/test-data/seed.sh minimal
./scripts/test-data/validate.sh minimal
```

Use `minimal` para smoke tests, login, validações rápidas e testes locais básicos.

## 7. Execução via Docker Compose

O serviço `test-data-seeder` existe e aceita somente o perfil `minimal` ou `e2e`. Ele aguarda migrations, executa reset, seed e validação.

```bash
docker compose --profile e2e run --rm test-data-seeder e2e
```

Não use argumentos como `reset e2e` ou `seed e2e`: o entrypoint atual não aceita subcomandos.

## 8. Após mudanças no banco

Execute quando houver migration, tabela, foreign key ou enum persistido novo:

```bash
dotnet build ClinicAssistant.sln
dotnet test ClinicAssistant.sln
ConnectionStrings__Default="Host=localhost;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" \
dotnet ef database update --project src/ClinicAssistant.Infrastructure/ClinicAssistant.Infrastructure.csproj --startup-project src/ClinicAssistant.Api/ClinicAssistant.Api.csproj
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

## 9. Após mudanças somente no seed

```bash
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

## 10. Validação rápida

```text
[ ] PostgreSQL está saudável
[ ] Redis está saudável
[ ] RabbitMQ está saudável
[ ] migrations foram aplicadas
[ ] reset terminou sem erro
[ ] seed terminou sem erro
[ ] validação terminou sem erro
[ ] API está saudável
[ ] Worker está consumindo
[ ] frontend está acessível
[ ] usuários E2E conseguem autenticar
[ ] tenant principal está disponível
[ ] tenant isolado não pode ser acessado
```

## 11. Ordem das próximas etapas E2E

```text
E2E-00 — Plataforma de Dados de Teste
E2E-TW-01 — Segurança e configuração
E2E-TW-02 — FakeWhatsAppGateway
E2E-TW-03 — Webhooks e assinatura
E2E-TW-04 — Envio real e callbacks
E2E-TW-05 — Recebimento e orquestração
E2E-TW-06 — Frontend operacional
E2E-TW-07 — CI e smoke real
```

Testes reais com Twilio só devem começar após `reset`, `seed`, `validate`, FakeWhatsAppGateway e webhooks validados.

## 12. Solução de problemas

| Problema | Verificação | Solução |
| --- | --- | --- |
| Reset bloqueado | `ALLOW_TEST_DATA_RESET` | Defina `true` apenas em teste. |
| Banco rejeitado | Nome do banco | Use `test`, `e2e`, `dev` ou `TEST_DATA_ALLOWED_DATABASES`. |
| Migration pendente | Logs da API | Suba `api` ou execute `dotnet ef database update`. |
| EF diz “up to date”, mas seed diz migration ausente | Banco ou histórico EF divergentes | Confirme `POSTGRES_DB`/`DATABASE_NAME`; suba a API e consulte o histórico no banco de teste. |
| API não sobe | Logs | `docker compose logs api` |
| Worker não processa | RabbitMQ e logs | `docker compose logs worker` |
| Validação falha | Script isolado | Corrija dados inconsistentes e rode reset/seed novamente. |

## 13. Comandos úteis

```bash
docker compose ps
docker compose logs -f
docker compose logs -f api
docker compose logs -f worker
docker compose restart api
docker compose restart worker
docker compose down
docker compose down -v
```

> `docker compose down -v` remove volumes, incluindo o banco local.

## 14. Regras de segurança

- Nunca execute reset em produção.
- Nunca versione `.env` ou credenciais reais.
- Nunca use telefone real nos fixtures.
- Nunca ative Twilio pelo seed.
- Nunca use `DROP DATABASE` em ambiente compartilhado.
- Confirme o banco antes de qualquer operação destrutiva.
