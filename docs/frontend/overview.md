# Frontend operacional

O frontend Next.js está em `frontend/` e usa App Router, TypeScript, Tailwind, TanStack Query, React Hook Form e Zod. O token de acesso fica somente em memória; tokens não são gravados em `localStorage`. Uma recarga exige novo login até que o backend ofereça sessão por cookie HttpOnly/BFF.

As páginas incluem login, dashboard, pacientes, agenda, cadastros, conversas, auditoria, administração WhatsApp e [administração de plataforma](platform-administration.md). A matriz atual de telas, permissões e endpoints está em [operação e E2E](operational-e2e.md).

## Validação

```bash
cd frontend
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```
