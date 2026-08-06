# Execução Playwright

Este é o guia canônico dos testes E2E. Os cenários vivem em `frontend/e2e/` e usam o manifesto determinístico em `database/seeds/e2e/manifest.json`.

Por necessidade operacional, todos os cenários podem ser temporariamente pulados com `E2E_SKIP=true`. A flag é opt-in e não altera a execução padrão; deve ser removida assim que a infraestrutura E2E estiver disponível novamente.

```bash
E2E_SKIP=true npm run e2e
```

1. Prepare o banco de teste conforme o [guia E2E](e2e-execution-guide.md).
2. Inicie API, Worker, dependências e frontend.
3. Execute no diretório `frontend`:

```bash
export E2E_DEFAULT_PASSWORD='senha-local-e2e'
export NEXT_PUBLIC_API_URL=http://localhost:8080
npm run test:e2e
```

Sem `E2E_DEFAULT_PASSWORD`, apenas o smoke não autenticado é executado. Os cenários autenticados cobrem catálogo, pacientes, agenda, conversas, Fake WhatsApp, multi-tenancy e SignalR. Traces e screenshots são preservados somente em falhas.

## Cobertura atual

- onboarding, unidades, especialidades e agenda de profissionais;
- paciente, consulta, conflito, reagendamento, confirmação e cancelamento;
- operações humanas, mensagem manual e leitura inbound fake;
- integração e templates Fake WhatsApp;
- isolamento multi-tenant;
- atualização SignalR de catálogo, fila, dashboard e auditoria entre sessões.

O CI usa o provider Fake e banco isolado; mensagens Twilio reais pertencem exclusivamente ao workflow manual protegido descrito em [prontidão Twilio](../operations/twilio-production-readiness.md).
