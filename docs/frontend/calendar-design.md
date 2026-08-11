# Agenda e calendário operacional

## Escopo da Etapa 9.7

A agenda administrativa passou a ter uma fundação visual responsiva composta por `CalendarShell`, toolbar, filtros e quatro visualizações: dia, semana, mês e lista. O componente está em `frontend/components/calendar/calendar.tsx` e a rota permanece `/appointments`.

## Comportamento

- O período consultado continua sendo calculado por `periodForView` e enviado à API existente.
- Filtros de profissional, unidade, especialidade e status são enviados à busca; o filtro de paciente é aplicado localmente sobre o nome retornado.
- Todos os eventos exibem paciente, profissional, especialidade e status em linguagem humana.
- Ao filtrar um profissional, bloqueios e férias do período são carregados pela API de schedule e aparecem como eventos neutros/indisponíveis, sem possibilidade de abertura do drawer de consulta.
- O timezone exibido é `America/Recife`; datas são manipuladas sem alterar o contrato UTC da API.
- View, data e filtros operacionais são sincronizados na query string sem incluir dados clínicos; em viewport móvel sem `view` explícita, a Lista é escolhida como padrão.
- O clique em um evento abre o drawer já existente, que mantém confirmação, cancelamento e reagendamento.

## Views

| View | Uso |
| --- | --- |
| Dia | linha do tempo vertical com horários e eventos |
| Semana | sete colunas, uma por dia |
| Mês | grade de 42 células, com limite visual e “mais” |
| Lista | agrupamento por data para operação rápida |

A criação rápida avançada, drag-and-drop e realtime visual detalhado ficam para a segunda entrega da etapa.
