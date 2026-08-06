# Cabeçalhos de segurança do frontend

O frontend configura os cabeçalhos em `frontend/next.config.ts`, aplicados a todas as rotas. A CSP é criada com `NEXT_PUBLIC_API_URL`; a origem WebSocket correspondente é incluída em `connect-src` para o SignalR.

| Cabeçalho | Valor / propósito |
| --- | --- |
| `Content-Security-Policy` | Permite apenas a própria aplicação, a API e SignalR configurados em `connect-src`. Não usa `unsafe-eval` nem `*`. |
| `X-Content-Type-Options` | `nosniff`. |
| `X-Frame-Options` | `DENY`; a CSP também usa `frame-ancestors 'none'`. |
| `Referrer-Policy` | `strict-origin-when-cross-origin`. |
| `Permissions-Policy` | Desabilita câmera, microfone e geolocalização. |

`style-src` e `script-src` usam `unsafe-inline` por compatibilidade com a renderização do Next.js. Não inclua domínios de terceiros ou `unsafe-eval` sem uma necessidade concreta e revisão de segurança.

## Validação local

Com o frontend em execução, confira os cabeçalhos e a origem efetiva da API:

```bash
curl -I http://localhost:3000
NEXT_PUBLIC_API_URL=http://localhost:8080 npm --prefix frontend run build
```

## Build e validação em produção

`NEXT_PUBLIC_API_URL` precisa estar definida **antes do build**. O Dockerfile recebe o valor como build argument, e o Compose o encaminha para esse estágio; alterar somente a variável do container já pronto não reconstrói a CSP.

Para uma implantação com API pública, construa com a origem HTTPS real:

```bash
NEXT_PUBLIC_API_URL=https://api.exemplo.com docker compose build frontend
```

Após publicar, confirme no navegador e por terminal que os cabeçalhos pertencem ao frontend real e que `connect-src` contém apenas as origens HTTPS/WSS esperadas:

```bash
curl -fsSI https://app.exemplo.com | rg -i 'content-security-policy|x-frame-options|x-content-type-options'
```

Verifique também login, chamadas HTTP e reconexão SignalR no DevTools: não deve haver violações de CSP. A origem de API deve gerar `https://...` e `wss://...`; não aceite `http://`, `ws://`, curingas ou `unsafe-eval` em produção.
