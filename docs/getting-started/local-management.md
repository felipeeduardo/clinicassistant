# Gerenciamento local

Scripts disponíveis em `scripts/local`:

- `start-local.sh`, `start-e2e.sh`, `start-twilio-smoke.sh`;
- `status.sh`, `logs.sh`, `validate.sh`, `open-app.sh`;
- `reset-e2e.sh` e `stop.sh`.

Todos validam Docker e bloqueiam execução com `ASPNETCORE_ENVIRONMENT=Production`.
