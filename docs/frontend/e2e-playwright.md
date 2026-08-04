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

## Cobertura atual

- formulário de login;
- login do administrador E2E;
- onboarding de tenant pelo administrador de plataforma;
- busca e abertura do detalhe de paciente fixture;
- acesso autenticado à agenda;
- detalhe e confirmação de consulta pendente do seed E2E;
- criação, bloqueio de conflito, reagendamento e cancelamento de consulta;
- disponibilidade, bloqueio de agenda e férias de profissional;
- criação e edição de especialidade administrativa;
- acesso à Inbox sem referência ao tenant isolado.
- assumir, transferir, liberar, priorizar, pausar, retomar, encerrar e reabrir atendimento humano;
- criação, edição, ativação e desativação de template WhatsApp;
- visualização da fila humana e da integração WhatsApp simulada.
- validação e mensagem de teste pelo `FakeWhatsAppGateway`.
- isolamento multi-tenant de pacientes e permissões de visualização.
- atualização de catálogo entre duas sessões por SignalR.

Os artefatos Playwright (trace e screenshot) são preservados somente em falhas. Cada execução pressupõe que `reset`, `seed` e `validate` já foram executados; os testes não alteram o banco nem chamam scripts de reset.
