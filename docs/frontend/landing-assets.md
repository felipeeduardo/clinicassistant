# Assets da Landing — Etapa 9.8.1

## Estado atual

Os PDFs fornecidos foram usados como referência visual de composição, mas não são publicados na Landing. O Product Showcase é construído exclusivamente com HTML/CSS, sem imagens externas e sem peso adicional de assets.

## Assets necessários

Para substituir os frames por capturas reais, fornecer três imagens aprovadas:

| Arquivo | Superfície | Conteúdo mínimo |
|---|---|---|
| Referência Dashboard | Dashboard | KPIs, status, fila, alertas e próximas consultas com dados fictícios |
| Referência Agenda | Agenda | Dia/Semana/Mês/Lista, filtros e disponibilidade |
| Referência Conversas | Conversas | Histórico, fila humana, handoff e contexto |

Requisitos:

- remover nomes, telefones, tokens e identificadores reais;
- preferir WebP ou AVIF em uma etapa posterior de compressão;
- largura recomendada entre 1280px e 1600px;
- preservar contraste e legibilidade em mobile;
- incluir texto alternativo específico por superfície;
- validar autorização para uso comercial.

Se futuramente forem aprovados assets comerciais, o componente poderá usar `next/image` com `sizes` responsivo e `priority` somente para a primeira superfície visível. Por decisão atual, as capturas permanecem fora do bundle.
