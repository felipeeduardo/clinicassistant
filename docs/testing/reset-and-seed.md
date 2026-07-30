# Reset, seed e validação

Pré-requisitos: migrations aplicadas, `psql` no `PATH`, .NET SDK e uma conexão configurada por `DATABASE_*` (ou `POSTGRES_*`). Para o banco padrão de desenvolvimento, informe explicitamente `TEST_DATA_ALLOWED_DATABASES=clinicassistant`.

```bash
export TEST_DATA_ALLOWED_DATABASES=clinicassistant
export ALLOW_TEST_DATA_RESET=true
export TEST_DATA_CONFIRM=YES
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

Também é possível executar `./scripts/test-data/seed.sh minimal` e `./scripts/test-data/reset.sh tenant <tenant-id>`. O reset apaga somente tenants conhecidos de teste (ou o tenant informado), em ordem inversa das dependências, e nunca toca em `__EFMigrationsHistory`.

No Docker: `docker compose --profile e2e run --rm test-data-seeder e2e`. O serviço aguarda a migration `202607300009_HumanQueue`, executa reset, seed e validação.
