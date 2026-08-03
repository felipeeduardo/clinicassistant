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

Para produção, defina `NEXT_PUBLIC_API_URL` para a URL HTTPS pública da API antes do build; ela fica incorporada no bundle de produção.
