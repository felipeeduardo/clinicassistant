# Etapa 9.7 — Modernização Visual da Agenda e Calendário Operacional

## Contexto

O Clinic Assistant já possui agenda funcional com criação, confirmação, reagendamento, cancelamento, disponibilidade, profissionais, unidades e atualização em tempo real.

A próxima evolução será visual e operacional: transformar a agenda em um calendário moderno, limpo, responsivo e adequado ao uso diário de clínicas e consultórios.

Referência visual:

```text
https://www.untitledui.com/react/components/calendars
```

A referência deve ser utilizada somente como inspiração de UX, hierarquia, navegação, densidade, acessibilidade e comportamento.

Não copiar código proprietário.
Não depender da implementação da Untitled UI.
Não substituir o design system atual sem necessidade.

---

# 1. Objetivo

Modernizar a agenda para permitir que recepcionistas, gestores e profissionais consigam:

- visualizar rapidamente a ocupação do dia;
- alternar entre dia, semana, mês e lista;
- filtrar profissionais, especialidades e unidades;
- encontrar horários livres;
- criar consultas rapidamente;
- abrir detalhes sem perder o contexto;
- reagendar de forma segura;
- identificar conflitos;
- visualizar bloqueios, férias e indisponibilidades;
- operar em desktop, notebook, tablet e mobile.

---

# 2. Princípios visuais

A agenda deverá ser:

```text
clean
modern
dense but readable
accessible
responsive
fast
predictable
```

Evitar:

- excesso de bordas;
- excesso de sombras;
- cores muito saturadas;
- muitos botões simultâneos;
- textos longos dentro dos eventos;
- scroll horizontal desnecessário;
- componentes desconectados do restante do produto.

---

# 3. Referência visual

Usar como inspiração os padrões de calendários modernos apresentados pela Untitled UI:

```text
Month view
Week view
Day view
```

Adotar conceitualmente:

- cabeçalho simples;
- toolbar compacta;
- navegação clara entre períodos;
- células bem delimitadas;
- eventos com boa hierarquia;
- estados de hover e focus;
- forte acessibilidade;
- visual integrado ao Tailwind/design system atual.

Não replicar pixel a pixel.

---

# 4. Auditoria obrigatória

Antes de alterar código:

1. analise a rota atual da agenda;
2. identifique componentes atuais;
3. identifique biblioteca de calendário existente;
4. identifique dependências instaladas;
5. identifique design tokens;
6. identifique componentes compartilhados;
7. analise filtros atuais;
8. analise query hooks;
9. analise mutations;
10. analise SignalR;
11. analise timezone;
12. analise mobile;
13. analise Playwright;
14. identifique problemas de acessibilidade;
15. identifique overflow;
16. identifique problemas de performance;
17. identifique código duplicado.

Produza:

| Área | Estado atual | Problema | Solução proposta |
|---|---|---|---|

Não modificar código antes de concluir essa análise.

---

# 5. Estrutura principal

A página deve conter:

```text
PageHeader
CalendarToolbar
CalendarFilters
CalendarViewport
AppointmentDrawer
```

Estrutura conceitual:

```text
Agenda

[Hoje] [<] [>] Agosto 2026          [Dia] [Semana] [Mês] [Lista]

[Profissional] [Especialidade] [Unidade] [Status] [Buscar]

---------------------------------------------------------------

                     CALENDÁRIO

---------------------------------------------------------------
```

---

# 6. Toolbar

Criar:

```text
CalendarToolbar
```

Responsabilidades:

- período atual;
- anterior;
- próximo;
- hoje;
- seletor de visualização;
- timezone;
- refresh;
- realtime status.

Layout desktop:

```text
[Hoje] [←] [→]     Agosto 2026        [Dia | Semana | Mês | Lista]
```

---

# 7. Visualizações obrigatórias

Implementar:

```text
Dia
Semana
Mês
Lista
```

Persistir visualização via URL ou preferência adequada.

Exemplo:

```text
/appointments?view=week
```

---

# 8. Visão Dia

A visão Dia será a principal tela operacional.

Exibir:

- linha do tempo;
- horário atual;
- slots;
- consultas;
- bloqueios;
- disponibilidade;
- conflitos;
- agrupamento por profissional/unidade quando útil.

---

# 9. Linha de horário atual

Adicionar indicador discreto:

```text
──────── 14:32
```

Atualizar sem causar renderizações excessivas.

---

# 10. Visão Semana

Mostrar dias da semana com:

- horas laterais;
- scroll vertical;
- cabeçalho sticky;
- destaque para hoje;
- altura proporcional à duração;
- sobreposição tratada;
- eventos clicáveis.

---

# 11. Visão Mês

Cada célula deve conter:

```text
data
até N eventos
+ X mais
```

Ao clicar em `+ X mais`, abrir popover, drawer ou lista diária.

Não renderizar dezenas de eventos dentro da célula.

---

# 12. Visão Lista

Especialmente importante no mobile.

Agrupar por data:

```text
Hoje
  09:00 Ana Silva — Cardiologia
  10:30 Bruno Lima — Ortopedia

Amanhã
  08:30 Carla Souza — Pediatria
```

Suportar paginação server-side.

---

# 13. AppointmentCalendarEvent

Criar componente reutilizável.

Prioridade visual:

```text
Horário
Paciente
Profissional ou especialidade
Status
```

Detalhes completos somente no drawer.

---

# 14. Status visuais

Suportar:

```text
Pending
Confirmed
Rescheduled
Cancelled
Completed
NoShow
Blocked
Unavailable
Conflict
```

Não depender só de cor.

Usar:

- ícone;
- badge;
- texto acessível;
- tokens semânticos.

---

# 15. Appointment Drawer

Ao clicar no evento, abrir drawer preservando o contexto do calendário.

Exibir:

```text
Paciente
Profissional
Especialidade
Unidade
Data
Horário
Status
Origem
Telefone mascarado
Conversa vinculada
Observação administrativa
Version
```

Ações:

```text
Confirmar
Reagendar
Cancelar
Abrir paciente
Abrir conversa
```

No mobile, usar full-screen sheet/página.

---

# 16. Criação rápida

Ao clicar em slot livre:

```text
Nova consulta
```

Pré-preencher apenas:

```text
data
horário
profissional
unidade
```

quando conhecidos.

---

# 17. Novo agendamento

Fluxo progressivo:

```text
Paciente
Especialidade
Profissional
Unidade
Data
Horário
Resumo
```

Evitar formulário longo.

---

# 18. Disponibilidade

Após escolher profissional e data, mostrar slots em chips/botões:

```text
08:00
08:30
09:00
09:30
```

Não usar select gigante.

---

# 19. Reagendamento

Permitir iniciar pelo drawer e, opcionalmente, por drag and drop seguro.

Exibir comparação:

```text
Atual
12/08 09:00

Novo
14/08 10:30
```

Exigir confirmação.

Usar:

```text
expectedVersion
Idempotency-Key
```

---

# 20. Drag and drop seguro

Implementar somente se backend suportar:

```text
expectedVersion
Idempotency-Key
conflict handling
rollback
```

Nunca tornar drag and drop a única forma de reagendar.

---

# 21. Conflitos

Em conflito:

```text
⚠ Conflito de horário
```

Oferecer:

```text
Ver horários disponíveis
```

Não mostrar apenas HTTP 409.

---

# 22. Bloqueios

Representar visualmente:

```text
Bloqueado
Férias
Intervalo
Indisponível
Unidade fechada
```

Diferenciar de consultas.

---

# 23. Filtros

Criar:

```text
CalendarFilterBar
```

Filtros:

```text
Profissional
Especialidade
Unidade
Status
Origem
Paciente
```

No mobile:

```text
Filtros (3)
```

em drawer.

---

# 24. Busca

Campo:

```text
Buscar paciente
```

com debounce e cancelamento da request anterior.

---

# 25. Estado na URL

Sincronizar estado relevante:

```text
?view=week&professionalId=...&unitId=...
```

Não incluir dados sensíveis.

---

# 26. Timezone

Exibir de forma discreta:

```text
Horários em America/Recife
```

ou label amigável.

---

# 27. Realtime

Consumir eventos:

```text
appointment.created
appointment.updated
appointment.confirmed
appointment.rescheduled
appointment.cancelled
availability.updated
professional.block.created
professional.block.deleted
professional.vacation.created
professional.vacation.deleted
```

Atualizar somente evento/período afetado.

---

# 28. Realtime indicator

Exibir:

```text
Connected
Reconnecting
Offline
```

de forma discreta na toolbar.

---

# 29. Loading

Utilizar skeleton do calendário.

Evitar spinner central gigante.

---

# 30. Empty state

Mensagem:

```text
Nenhuma consulta neste período.

Você pode criar uma nova consulta ou ajustar os filtros.
```

Ação:

```text
Nova consulta
```

---

# 31. Error state

Exibir:

```text
Não foi possível carregar a agenda.
```

Ação:

```text
Tentar novamente
```

---

# 32. Performance

Queries devem carregar somente intervalo necessário usando:

```text
from
to
professionalId
unitId
specialtyId
status
```

Não carregar toda agenda.

---

# 33. Month view performance

Preferir endpoint agregado quando disponível.

Exemplo:

```text
date
appointmentCount
confirmedCount
availableSlots
```

Detalhes somente ao abrir o dia.

---

# 34. Responsividade

Validar:

```text
375
430
768
1024
1280
1440
```

---

# 35. Mobile

Padrão recomendado:

```text
Lista
```

ou:

```text
Dia
```

Toolbar compacta:

```text
[←] Hoje [→]
[Dia ▼]
[Filtros]
```

Evitar semana comprimida.

---

# 36. Tablet

Permitir semana simplificada e filtros recolhíveis.

---

# 37. Desktop

Usar largura disponível.

A agenda pode utilizar container mais amplo que formulários comuns.

---

# 38. Sticky headers

Dia/Semana:

- cabeçalho de datas sticky;
- coluna de horários sticky quando útil.

---

# 39. Acessibilidade

Meta:

```text
WCAG 2.2 AA
```

Aplicar:

- keyboard navigation;
- focus states;
- aria labels;
- status textual;
- contraste;
- touch target;
- alternativa ao drag.

---

# 40. React Aria

Se o projeto já utiliza ou se houver justificativa técnica, avaliar React Aria para:

- focus;
- keyboard;
- accessibility primitives.

Não adicionar apenas porque a referência utiliza.

---

# 41. Biblioteca de calendário

Antes de adicionar dependência, comparar:

```text
solução atual
FullCalendar
React Big Calendar
custom implementation
ou biblioteca já instalada
```

Avaliar:

- licença;
- bundle;
- acessibilidade;
- compatibilidade com Next/React;
- timezone;
- drag and drop;
- manutenção;
- SSR.

Documentar decisão.

---

# 42. Não copiar Untitled UI

A referência é somente visual/conceitual.

Não:

- copiar código fechado;
- replicar implementação;
- instalar pacote apenas para obter aparência;
- copiar arquivos proprietários.

---

# 43. Componentes propostos

Criar/evoluir apenas os necessários:

```text
CalendarShell
CalendarToolbar
CalendarViewSwitcher
CalendarFilterBar
CalendarDayView
CalendarWeekView
CalendarMonthView
CalendarListView
CalendarTimeColumn
CalendarDayHeader
AppointmentCalendarEvent
CalendarBlockEvent
CalendarEmptyState
CalendarSkeleton
AppointmentDrawer
AppointmentQuickCreate
RescheduleDialog
RealtimeIndicator
```

---

# 44. Separação de responsabilidades

Componentes visuais não devem chamar API diretamente.

Usar:

```text
hooks
services
queries
mutations
```

---

# 45. Query keys

Organizar:

```text
appointments.calendar
appointments.detail
appointments.availability
appointments.summary
```

Considerar:

```text
tenant
range
filters
```

---

# 46. E2E e locators

Não depender de classes CSS.

Quando necessário:

```text
data-testid="appointment-{id}"
data-testid="calendar-day-{yyyy-mm-dd}"
```

Usar test IDs somente onde semântica acessível não for suficiente.

---

# 47. Corrigir drift E2E

Atualizar testes antigos que ainda procuram elementos obsoletos como:

```text
getByLabel("Data")
```

Criar helper:

```text
navigateCalendarToDate()
```

quando fizer sentido.

---

# 48. Playwright

Cobrir:

```text
abre agenda
troca Dia/Semana/Mês/Lista
navega para data
filtra profissional
filtra unidade
abre consulta
cria consulta
confirma
reagenda
trata conflito
cancela
recebe realtime
mobile
```

---

# 49. Testes unitários

Cobrir:

- date calculations;
- grouping;
- event positioning;
- overlap calculation;
- status mapping;
- filter parsing;
- URL state;
- timezone helpers.

---

# 50. Overlapping events

Exibir eventos simultâneos lado a lado quando possível.

Não sobrepor conteúdo ilegível.

---

# 51. Horário operacional

Quando disponível, usar horário da unidade/profissional.

Exemplo:

```text
07:00–20:00
```

Não renderizar sempre 24h.

---

# 52. Hoje

Destacar dia atual de forma sutil.

---

# 53. Ações rápidas

No evento, não adicionar vários botões pequenos.

Concentrar ações no drawer ou context menu acessível.

---

# 54. Segurança

Frontend não decide regras de negócio.

Backend continua validando:

```text
authorization
tenant
status
expectedVersion
slot availability
```

---

# 55. Error mapping

Mapear:

```text
appointment_conflict
appointment_not_found
appointment_invalid_status
slot_unavailable
professional_unavailable
unit_closed
version_conflict
```

para mensagens amigáveis.

---

# 56. Documentação

Criar ou atualizar:

```text
docs/frontend/calendar-design.md
docs/frontend/scheduling.md
docs/frontend/calendar-accessibility.md
docs/frontend/calendar-realtime.md
docs/testing/calendar-e2e.md
docs/architecture/decisions/adr-calendar-component.md
```

---

# 57. Critérios de aceite

A etapa estará concluída quando:

1. agenda estiver visualmente modernizada;
2. design estiver coerente com o produto;
3. Month View funcionar;
4. Week View funcionar;
5. Day View funcionar;
6. List View funcionar;
7. toolbar estiver moderna;
8. filtros funcionarem;
9. URL preservar estado relevante;
10. cards de consulta estiverem legíveis;
11. drawer funcionar;
12. quick create funcionar;
13. reagendamento funcionar;
14. conflito funcionar;
15. cancelamento funcionar;
16. realtime atualizar eventos;
17. bloqueios aparecerem;
18. férias aparecerem;
19. timezone estiver correto;
20. loading usar skeleton;
21. empty state existir;
22. mobile funcionar;
23. tablet funcionar;
24. desktop usar largura disponível;
25. acessibilidade estiver validada;
26. drag possuir alternativa acessível;
27. testes unitários passarem;
28. Playwright passar;
29. E2E antigo for atualizado para a nova UX;
30. nenhuma regressão funcional ocorrer;
31. documentação estiver atualizada;
32. decisão da biblioteca estiver documentada.

---

# 58. Ordem de implementação

```text
9.7.1 Auditoria
9.7.2 Fundação visual
9.7.3 Day View
9.7.4 Week View
9.7.5 Month View
9.7.6 List View
9.7.7 Appointment Events
9.7.8 Drawer e Quick Create
9.7.9 Mutations e conflitos
9.7.10 Realtime
9.7.11 Responsividade e acessibilidade
9.7.12 Testes
9.7.13 Documentação
```

---

# 59. Primeira entrega

Implemente inicialmente:

```text
Auditoria
CalendarShell
CalendarToolbar
CalendarViewSwitcher
CalendarFilterBar
Day View
Week View
Month View
List View
AppointmentCalendarEvent
```

Não alterar mutations antes da base visual estar estável.

---

# 60. Segunda entrega

Depois:

```text
AppointmentDrawer
QuickCreate
Create
Confirm
Reschedule
Cancel
Conflicts
Realtime
```

---

# 61. Validação

Executar:

```bash
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1
```

Se houver alteração backend:

```bash
dotnet build
dotnet test
```

---

# 62. Relatório final

Apresentar:

1. arquitetura antiga;
2. problemas visuais;
3. biblioteca escolhida;
4. justificativa;
5. componentes criados;
6. componentes reutilizados;
7. views implementadas;
8. filtros;
9. drawer;
10. quick create;
11. realtime;
12. responsividade;
13. acessibilidade;
14. performance;
15. testes;
16. E2E atualizado;
17. documentação;
18. riscos restantes.

Não copiar código da referência Untitled UI.
Não avançar para funcionalidades fora desta etapa.
