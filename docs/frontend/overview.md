# Etapa 9 — Fundação do frontend

O frontend Next.js está em `frontend/` e usa App Router, TypeScript, Tailwind, TanStack Query, React Hook Form e Zod. O token de acesso fica somente em memória; tokens não são gravados em `localStorage`. Uma recarga exige novo login até que o backend ofereça sessão por cookie HttpOnly/BFF.

As páginas iniciais incluem login, dashboard, Inbox estrutural, os cadastros operacionais de clínica atual, unidades, pacientes, profissionais e especialidades, além da agenda diária. Consulte [o mapa operacional e suas dependências](operational-e2e.md) antes de validar os fluxos E2E: administração de tenants/usuários, auditoria, realtime e reagendamento dependem de APIs que ainda não existem.

## Validação

```bash
cd frontend
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```
