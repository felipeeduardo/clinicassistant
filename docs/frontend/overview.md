# Etapa 9 — Fundação do frontend

O frontend Next.js está em `frontend/` e usa App Router, TypeScript, Tailwind, TanStack Query, React Hook Form e Zod. O token de acesso fica somente em memória; tokens não são gravados em `localStorage`. Uma recarga exige novo login até que o backend ofereça sessão por cookie HttpOnly/BFF.

As páginas iniciais são login, dashboard e Inbox estrutural. A Inbox e os indicadores serão conectados quando a API administrativa de conversas e métricas estiver disponível.

## Validação

```bash
cd frontend
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```
