# Relatório de `npm audit`

Data da análise: 2026-08-05. Comando executado: `npm audit --omit=dev --json`, no diretório `frontend`.

O relatório apontou 3 vulnerabilidades de severidade alta e nenhuma crítica para as dependências de produção. Todas chegam pela dependência direta `next@15.5.22`, que traz `postcss@8.4.31` e `sharp@0.34.5` de forma transitiva. A única correção automática indicada pelo npm é `next@16.3.0`, uma atualização major.

| Pacote | Avisos reportados | Origem | Situação |
| --- | --- | --- | --- |
| `postcss@8.4.31` | [GHSA-qx2v-qp2m-jg93](https://github.com/advisories/GHSA-qx2v-qp2m-jg93), [GHSA-6g55-p6wh-862q](https://github.com/advisories/GHSA-6g55-p6wh-862q), [GHSA-r28c-9q8g-f849](https://github.com/advisories/GHSA-r28c-9q8g-f849) e [GHSA-fxqj-rqcc-2cmp](https://github.com/advisories/GHSA-fxqj-rqcc-2cmp) | Transitiva por `next` | Accepted Temporarily / Blocked by major upgrade |
| `sharp@0.34.5` | [GHSA-f88m-g3jw-g9cj](https://github.com/advisories/GHSA-f88m-g3jw-g9cj) | Transitiva por `next` | Accepted Temporarily / Blocked by major upgrade |
| `next@15.5.22` | Agrupa os avisos transitivos acima | Direta | Accepted Temporarily |

## Decisão

Não foi executado `npm audit fix --force`. A sugestão automática atual aponta para `next@16.3.0`; embora seja uma correção disponível, ela é uma migração major e deve passar por avaliação de compatibilidade, atualização de lockfile, build e E2E completos antes de ser adotada.

Como mitigação, a aplicação não aceita CSS de usuários e entrega uma CSP restritiva: `connect-src` é limitado ao frontend, API e SignalR configurados; não há `unsafe-eval`, curingas ou recursos remotos indiscriminados. Isso reduz, mas não elimina, o risco de vulnerabilidades em ferramentas de transformação.

## Ação necessária

Manter `next`, `postcss` e `sharp` sob revisão. Planejar a migração para Next 16 em uma mudança dedicada; após ela, rode build, testes unitários, Playwright e `npm audit --omit=dev --json`. Altere a classificação para **Resolved** somente quando não restarem avisos de produção. Nenhum segredo, token ou número de WhatsApp é registrado neste relatório.
