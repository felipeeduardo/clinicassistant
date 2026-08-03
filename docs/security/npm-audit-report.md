# Relatório de `npm audit`

Data da análise: 2026-08-03. Comando executado: `npm audit --omit=dev --json`, no diretório `frontend`.

O relatório apontou 3 vulnerabilidades de severidade alta e nenhuma crítica para as dependências de produção. Todas chegam pela dependência direta `next@15.5.22`, que traz `postcss@8.4.31` e `sharp@0.34.5` de forma transitiva.

| Pacote | Avisos reportados | Origem | Situação |
| --- | --- | --- | --- |
| `postcss` | GHSA-qx2v-qp2m-jg93, GHSA-6g55-p6wh-862q, GHSA-r28c-9q8g-f849 e GHSA-fxqj-rqcc-2cmp | Transitiva por `next` | Mitigated / Blocked |
| `sharp` | Aviso de severidade alta reportado pelo audit | Transitiva por `next` | Blocked |
| `next` | Agrupa os avisos transitivos acima | Direta | Blocked |

## Decisão

Não foi executado `npm audit fix --force`. No momento da análise, a sugestão automática do npm apontava para `next@9.3.3`, uma redução de versão incompatível com a aplicação Next 15 e, portanto, com alto risco de quebra.

Como mitigação, a aplicação não aceita CSS de usuários e entrega uma CSP restritiva: `connect-src` é limitado ao frontend, API e SignalR configurados; não há `unsafe-eval`, curingas ou recursos remotos indiscriminados. Isso reduz, mas não elimina, o risco de vulnerabilidades em ferramentas de transformação.

## Ação necessária

Manter `next`, `postcss` e `sharp` sob revisão. Atualize para uma versão compatível do Next que o advisory do npm reconheça como corrigida, rode novamente `npm audit --omit=dev --json` e altere a classificação para **Resolved** somente quando não restarem avisos de produção. Nenhum segredo, token ou número de WhatsApp é registrado neste relatório.
