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

### Production

No projeto Vercel, em **Settings → Environment Variables → Production**, use:

```text
NEXT_PUBLIC_API_URL=https://api.iarecepcao.com.br
NEXT_PUBLIC_SITE_URL=https://iarecepcao.com.br
NEXT_PUBLIC_APP_URL=https://app.iarecepcao.com.br
NEXT_PUBLIC_BRAND_DOMAIN=iarecepcao.com.br
```

Depois de salvar, faça um novo deploy. `NEXT_PUBLIC_API_URL` é usada tanto pelo
cliente HTTP quanto pelo SignalR e também define a origem permitida na CSP; mudar
a variável sem reconstruir o frontend não altera o bundle já publicado.

No Railway, na API, configure a allowlist CORS para os domínios que realmente
serão usados pelo frontend, por exemplo:

```text
Frontend__AllowedOrigins__0=https://iarecepcao.com.br
Frontend__AllowedOrigins__1=https://app.iarecepcao.com.br
```

Não inclua `http://localhost` na configuração de produção. O valor local
continua sendo fornecido pelo Compose e pelo `frontend/.env.example`.

## Checklist Preview

- [ ] Build Linux concluído.
- [ ] Landing, login e assets carregam.
- [ ] API de Preview está configurada e responde por HTTPS.
- [ ] CSP, redirects e mobile foram verificados.
- [ ] Nenhum erro JavaScript crítico aparece no navegador.

Domínios e promoção Production permanecem bloqueados pelo Gate A.
