# Etapa 9 — Fundação do frontend

O frontend Next.js está em `frontend/` e usa App Router, TypeScript, Tailwind, TanStack Query, React Hook Form e Zod. O token de acesso fica somente em memória; tokens não são gravados em `localStorage`. Uma recarga exige novo login até que o backend ofereça sessão por cookie HttpOnly/BFF.

As páginas iniciais incluem login, dashboard, Inbox estrutural, cadastros operacionais, agenda diária e a [administração de plataforma](platform-administration.md). Consulte [o mapa operacional e suas dependências](operational-e2e.md) antes de validar os fluxos E2E: auditoria completa, realtime ampliado e reagendamento dependem de APIs posteriores.

## Validação

```bash
cd frontend
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```
