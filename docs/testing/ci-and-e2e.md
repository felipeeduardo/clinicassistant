# CI e E2E

O workflow [ci.yml](../../.github/workflows/ci.yml) executa em pull requests e na branch `main`:

- restore, build e testes .NET em Release;
- `typecheck`, lint, testes unitários e build do frontend.

Os cenários Playwright dependem do banco determinístico e da API em execução. Neste momento, a execução E2E está temporariamente fora do workflow de CI para permitir validação funcional manual por um usuário QA. O job pode ser reintroduzido quando a infraestrutura E2E for retomada.

Os cenários permanecem versionados em `frontend/e2e/`, mas não são executados automaticamente pela pipeline.

Não configure senha E2E nem destinatários de teste no repositório ou no workflow. Para o ciclo atual, use o [roteiro de QA manual](manual-qa-login.md) e os fluxos funcionais documentados em [execução local](e2e-execution-guide.md).
