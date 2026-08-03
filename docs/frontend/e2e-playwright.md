# Playwright E2E

Os testes estão em `frontend/e2e/` e usam o manifesto versionado em `database/seeds/e2e/manifest.json`. O manifesto fornece IDs e e-mails; a senha nunca é versionada e deve ser fornecida por `E2E_DEFAULT_PASSWORD`.

## Execução

Primeiro prepare a infraestrutura e os dados conforme o [guia E2E](../testing/e2e-execution-guide.md). Em seguida:

```bash
cd frontend
export E2E_DEFAULT_PASSWORD='sua-senha-e2e-local'
export NEXT_PUBLIC_API_URL=http://localhost:8080
E2E_DEFAULT_PASSWORD='<senha-do-ambiente>' npm run test:e2e
```

Sem `E2E_DEFAULT_PASSWORD`, somente o smoke não autenticado de login é executado; os testes que exigem API são ignorados intencionalmente.

## Cobertura inicial

- formulário de login;
- login do administrador E2E;
- acesso autenticado a pacientes e agenda;
- acesso à Inbox sem referência ao tenant isolado.

Os artefatos Playwright (trace e screenshot) são preservados somente em falhas. Cada execução pressupõe que `reset`, `seed` e `validate` já foram executados; os testes não alteram o banco nem chamam scripts de reset.
