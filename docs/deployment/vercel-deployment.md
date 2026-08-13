# Deploy Vercel — IA Recepção

## Estado

`MANUAL ACTION REQUIRED`: nenhuma conta Vercel ou Preview foi acessada nesta etapa.

## Configuração planejada

- Root Directory: `frontend`.
- Framework: Next.js.
- Node: 22, conforme `frontend/Dockerfile` e `package.json`.
- Build: `npm run build`.
- Install: `npm ci`.
- Production branch: `main`, após aprovação.
- Preview: obrigatório antes de associar domínio de produção.

Configure as variáveis `NEXT_PUBLIC_*` usando a matriz de ambientes. Elas são
embutidas no bundle durante o build; alteração exige novo deploy. Não cadastrar
JWT, banco, Redis, RabbitMQ ou Twilio na Vercel.

## Checklist Preview

- [ ] Build Linux concluído.
- [ ] Landing, login e assets carregam.
- [ ] API de Preview está configurada e responde por HTTPS.
- [ ] CSP, redirects e mobile foram verificados.
- [ ] Nenhum erro JavaScript crítico aparece no navegador.

Domínios e promoção Production permanecem bloqueados pelo Gate A.
