# CI e E2E

O workflow [ci.yml](../../.github/workflows/ci.yml) executa em pull requests e na branch `main`:

- restore, build e testes .NET em Release;
- `typecheck`, lint, testes unitários e build do frontend.

Os cenários Playwright dependem do banco determinístico e da API em execução; por isso são executados separadamente no ambiente de integração:

```bash
./scripts/test-data/reset.sh e2e
docker compose up -d --build
cd frontend
E2E_DEFAULT_PASSWORD='<senha-do-ambiente>' npm run test:e2e
```

Não configure a senha E2E nem destinatários de teste no repositório ou no workflow.
