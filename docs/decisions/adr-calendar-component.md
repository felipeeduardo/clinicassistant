# ADR: componente de calendário operacional

## Decisão

Adotar um componente React próprio para a primeira entrega da agenda, com views dia/semana/mês/lista e filtros reutilizáveis.

## Contexto

O frontend já possuía a rota, os contratos da API e as mutações, mas a apresentação era uma tabela/cards sem calendário real. Não havia necessidade de transferir regras de disponibilidade, conflito ou autorização para o navegador.

## Consequências

- Menor dependência e bundle previsível.
- Acessibilidade e nomes humanos são controlados pelo produto.
- Drag-and-drop, realtime incremental e criação rápida continuam como evolução explícita.
- O backend permanece a fonte de verdade para disponibilidade, versionamento e transições de status.
