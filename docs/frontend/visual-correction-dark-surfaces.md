# Correção visual — superfícies dark, scrollbar e header público

## Ajustes realizados

- O card **Impacto operacional estimado** agora usa `brand-dark`, com borda `brand-dark-border`.
- Os quatro indicadores internos usam `brand-dark-surface`, mantendo valores brancos e labels claros.
- O disclaimer permanece com texto claro (`text-slate-300`) para preservar legibilidade.
- O card informativo de WhatsApp no Product Showcase deixou de usar `bg-slate-950` e passou a usar `bg-brand-dark`.
- A sidebar desktop e o drawer mobile usam `brand-dark`.
- O scroll container da sidebar recebeu a utilidade `brand-scrollbar`: trilho transparente, thumb discreto e hover mais perceptível, com fallback Firefox e WebKit.
- O header público já permanece sticky sobre `brand-dark`, evitando perda de contraste durante o scroll; o menu mobile usa `brand-dark-surface`.

## Tokens reutilizados

`brand-dark`, `brand-dark-surface`, `brand-dark-border`, `foreground-on-dark` e `brand-primary`. Nenhuma cor nova foi criada e nenhuma cor de status foi alterada.

## Escopo preservado

Não foram alterados fórmulas, navegação, autenticação, backend, API, textos ou estrutura de menus. Cards neutros, estados semânticos e logo foram preservados.

## Validação

- `npm run lint` — aprovado.
- `npm run typecheck` — aprovado.
- `npm run test -- --run` — 19 arquivos, 41 testes aprovados.
- `npm run build` — aprovado.
- `git diff --check` — aprovado.

QA visual manual recomendado: Landing no topo/meio/final, calculadora, Showcase, sidebar desktop/mobile e Safari para confirmar o comportamento do scrollbar.
