# Etapa 9.8 --- Refinamento de Produto, Dashboard Executivo e Landing Page Comercial

## Contexto e objetivo

A Etapa 9.7 modernizou a Agenda. A Etapa 9.8 deve consolidar o Clinic
Assistant como um MVP mais limpo para operar, mais útil para gerir e
mais fácil de demonstrar e vender.

Frentes:

1.  Agenda: reduzir ruído visual e padronizar controles.
2.  Dashboard: transformar números em informações operacionais
    acionáveis.
3.  Landing page: apresentar o produto publicamente de forma moderna,
    simples e convincente.
4.  Hardening: responsividade, acessibilidade, performance, testes e
    documentação.

Não criar funcionalidades ou métricas fictícias. Não expor dados reais.
Não quebrar fluxos da Etapa 9.7.

------------------------------------------------------------------------

## 1. Auditoria obrigatória

Antes de alterar código, analisar Agenda, Dashboard e área pública.

### Agenda

-   CalendarShell, toolbar, filtros, views Dia/Semana/Mês/Lista.
-   formulário atual de criação;
-   AppointmentDrawer/QuickCreate;
-   inputs, selects, date pickers e botões;
-   espaçamentos, alturas, larguras e overflow;
-   responsividade e Playwright.

### Dashboard

-   endpoint e métricas atuais;
-   cards, queries e SignalR;
-   consultas, pacientes, conversas, WhatsApp, fila humana, SLA,
    cancelamentos, no-show e profissionais.

### Landing

-   rota pública;
-   layout, logo e identidade visual;
-   autenticação;
-   assets;
-   SEO/metadata;
-   mobile e performance.

Produzir antes da implementação:

  --------------------------------------------------------------------------
  Área           Estado atual   Problema       Mudança        Backend
                                               proposta       necessário?
  -------------- -------------- -------------- -------------- --------------

  --------------------------------------------------------------------------

------------------------------------------------------------------------

# PARTE A --- AGENDA

## 2. Hierarquia da Agenda

A página deve priorizar:

``` text
Agenda
↓
Toolbar
↓
Filtros
↓
Calendário
```

O formulário de nova consulta não deve permanecer aberto ocupando
espaço.

## 3. Nova consulta sob demanda

Ocultar o formulário permanente e manter um botão discreto:

``` text
+ Nova consulta
```

Posicionar no PageHeader ou CalendarToolbar.

Ao clicar:

``` text
Desktop → side drawer
Mobile → full-screen sheet
```

Preservar o Quick Create pelo clique em slot livre.

Quick Create deve pré-preencher data, horário, profissional e unidade
apenas quando esses valores forem conhecidos.

## 4. Formulário

Organizar em grupos:

``` text
Paciente
Atendimento
Data e horário
Resumo
```

Ordem sugerida:

``` text
Paciente
Especialidade
Profissional
Unidade
Data
Horário
Observação administrativa opcional
```

Usar progressive disclosure; evitar formulário longo e visualmente
pesado.

## 5. Uniformização de controles

Padronizar no design system:

``` text
Button
Input
Select
DatePicker
Combobox
```

Criar/reutilizar tamanhos `sm`, `md`, `lg`.

Como referência, controles normais podem usar aproximadamente 40px e
compactos 36px, desde que coerente com tokens existentes.

Na mesma linha, input/select/date picker/button devem possuir:

-   mesma altura visual;
-   radius consistente;
-   baseline alinhada;
-   padding consistente;
-   focus ring consistente.

Não hardcode estilos diferentes em cada componente.

## 6. Toolbar e filtros

Desktop:

``` text
[Hoje] [←] [→] Agosto 2026    [Dia | Semana | Mês | Lista]    [+ Nova consulta]
```

Mobile:

``` text
Agosto 2026
[←] [Hoje] [→]
[Dia ▼] [Filtros] [+]
```

O botão `+` deve possuir `aria-label="Nova consulta"`.

Filtros:

``` text
Profissional
Especialidade
Unidade
Status
Paciente
```

Desktop: layout compacto e alinhado. Mobile: botão `Filtros (N)` abrindo
drawer.

## 7. Espaçamento

Revisar padding, margens, largura das colunas, altura das linhas, sticky
headers e overflow.

O calendário deve ocupar a maior área útil da página.

## 8. Aceite da Agenda

-   formulário oculto por padrão;
-   Nova Consulta acessível;
-   drawer/sheet funcional;
-   Quick Create preservado;
-   controles uniformes;
-   filtros alinhados;
-   calendário com mais espaço;
-   mobile sem overflow;
-   Dia/Semana/Mês/Lista sem regressão;
-   E2E verde.

------------------------------------------------------------------------

# PARTE B --- DASHBOARD

## 9. Objetivo

O Dashboard deve responder rapidamente:

``` text
Como está a operação?
Quantas consultas temos?
Quantas estão confirmadas/canceladas?
Existem pacientes aguardando?
Como está o WhatsApp?
Quais horários/profissionais estão mais ocupados?
Existe algo que exige atenção?
```

Não criar gráficos decorativos.

## 10. Estrutura

``` text
Dashboard
├── Filtro de período
├── KPIs
├── Evolução de atendimentos
├── Status das consultas
├── Ocupação por profissional
├── WhatsApp
├── Fila humana / SLA
├── Próximas consultas
└── Atenção necessária
```

## 11. Período

Adicionar:

``` text
Hoje
7 dias
30 dias
Este mês
Personalizado
```

Sincronizar com URL quando apropriado:

``` text
/dashboard?period=30d
```

## 12. KPIs

Cards principais:

``` text
Consultas
Confirmadas
Canceladas
Pacientes atendidos
```

Secundários quando houver dados:

``` text
Taxa de confirmação
Taxa de cancelamento
No-show
Tempo médio de resposta
```

Cada card pode mostrar valor, comparação com período anterior e
tendência somente se houver dados reais.

## 13. Gráfico de evolução

Gráfico temporal de consultas com séries úteis:

``` text
Agendadas
Confirmadas
Concluídas
Canceladas
```

Adequar granularidade ao período.

## 14. Distribuição por status

Mostrar:

``` text
Pending
Confirmed
Completed
Cancelled
NoShow
```

Escolher donut, barras horizontais ou stacked bar conforme melhor
legibilidade.

## 15. Ocupação por profissional

Mostrar Top N com:

``` text
Profissional
Consultas
Horas ocupadas
Taxa de ocupação
```

Objetivo: planejamento operacional, não competição entre profissionais.

## 16. Horários mais procurados

Quando suportado pelos dados, mostrar volume por faixa horária para
identificar picos.

## 17. Funil operacional

Se os eventos existirem:

``` text
Conversa iniciada
→ Disponibilidade consultada
→ Agendamento iniciado
→ Consulta agendada
→ Consulta confirmada
```

Se não existirem, não inventar números. Documentar lacuna e preparar
contrato/métricas futuras.

## 18. WhatsApp

Widget:

``` text
Mensagens recebidas
Mensagens enviadas
Falhas
Conversas abertas
Automação pausada
```

Ação: abrir área operacional de WhatsApp/Conversas.

## 19. Fila humana

Widget:

``` text
Aguardando
Em atendimento
Tempo médio de espera
Mais antiga na fila
```

Ação: `Ver fila`.

## 20. Atenção necessária

Exibir apenas ocorrências reais:

``` text
falhas de mensagens
conversas acima do SLA
conflitos de agenda
problemas de integração
```

Cada item deve levar ao local onde pode ser tratado.

## 21. Próximas consultas

Lista curta:

``` text
Horário
Paciente
Profissional
Status
```

Ação: `Ver agenda`.

## 22. Dashboard acionável

Widgets devem permitir drill-down:

``` text
Canceladas → Agenda filtrada
Fila → Conversas
Falhas → WhatsApp Operations
Profissional → Agenda filtrada
Consulta → detalhe/Agenda
```

## 23. API agregada

Avaliar endpoint existente. Se necessário, criar/evoluir algo
equivalente a:

``` http
GET /api/v1/dashboard/overview?from=...&to=...
```

Contrato conceitual:

``` json
{
  "summary": {},
  "appointmentsTrend": [],
  "appointmentStatus": [],
  "professionalOccupancy": [],
  "whatsapp": {},
  "humanQueue": {},
  "alerts": [],
  "upcomingAppointments": []
}
```

Não criar endpoint duplicado se já houver contrato adequado.

Agregações devem ocorrer no backend/banco, não carregando dados brutos
para o browser.

## 24. Realtime

Atualizar widgets por eventos relevantes, invalidando apenas queries
afetadas.

## 25. Gráficos

Antes de instalar biblioteca, verificar dependências atuais. Avaliar
bundle, manutenção, acessibilidade e compatibilidade React/Next.

Regras:

-   sem 3D;
-   poucas cores;
-   labels legíveis;
-   tooltips;
-   responsividade;
-   skeleton;
-   empty/error states;
-   acessibilidade.

------------------------------------------------------------------------

# PARTE C --- LANDING PAGE

## 26. Objetivo comercial

A landing deve responder em segundos:

``` text
O que é?
Para quem é?
Que problema resolve?
Como funciona?
Que recursos possui?
Como solicitar uma demonstração?
```

## 27. Posicionamento

Direção de copy:

``` text
Transforme o WhatsApp da sua clínica em uma recepção inteligente.
```

Subheadline:

``` text
Automatize consultas, disponibilidade, agendamentos e atendimento pelo WhatsApp sem perder o controle da operação.
```

Ajustar a copy conforme o produto real.

Não prometer IA se essa capacidade ainda não estiver implementada.

## 28. Hero

Compor:

``` text
Headline
Subheadline
CTA principal
CTA secundário
Preview do produto
```

CTAs:

``` text
Solicitar demonstração
Ver como funciona
```

Incluir `Entrar` para login.

## 29. Demo visual do WhatsApp

Criar mock leve:

``` text
Paciente:
Tem cardiologista amanhã?

Clinic Assistant:
Sim. Encontrei:
09:00 — Dra. Ana
10:30 — Dr. Bruno

Paciente:
10:30

Clinic Assistant:
Posso confirmar seu agendamento?
```

É uma demonstração visual, sem depender do backend real.

## 30. Seção Problema

Cards:

``` text
Mensagens acumuladas
Demora nas respostas
Agenda desorganizada
Confirmações manuais
Tarefas repetitivas
Pouca visibilidade da operação
```

## 31. Como funciona

Diagrama próprio HTML/CSS/SVG:

``` text
1. Paciente chama no WhatsApp
          ↓
2. Clinic Assistant conduz a solicitação
          ↓
3. Consulta agenda e disponibilidade
          ↓
4. Clínica acompanha no painel
```

Não usar imagem pesada quando um componente responsivo resolver.

## 32. Três pilares

``` text
WhatsApp
Agenda
Gestão
```

WhatsApp: - disponibilidade; - agendamento; - reagendamento; -
cancelamento; - confirmação; - handoff humano.

Agenda: - dia; - semana; - mês; - profissionais; - bloqueios; -
disponibilidade.

Gestão: - dashboard; - indicadores; - fila humana; - conversas; -
auditoria; - operação WhatsApp.

## 33. Screenshots

Criar seção com telas reais:

``` text
Dashboard
Agenda
Conversas
```

Preferir screenshots reais ou mockups derivados das telas reais.

Nunca usar dados reais de pacientes.

Se assets ainda não existirem, criar placeholders claramente
identificados para substituição.

## 34. Diagrama operacional

Criar:

``` text
Paciente
   ↓
WhatsApp
   ↓
Clinic Assistant
   ├── Agenda
   ├── Profissionais
   ├── Disponibilidade
   └── Atendimento humano
           ↓
        Recepção
```

Desktop: horizontal quando adequado. Mobile: vertical.

## 35. Benefícios

Usar claims qualitativos reais:

``` text
Menos tarefas repetitivas
Respostas mais rápidas
Agenda centralizada
Mais controle da recepção
Histórico de conversas
Visibilidade operacional
```

Não inventar percentuais de economia/conversão.

## 36. Público-alvo

Manter foco:

``` text
Clínicas médicas
Consultórios
Clínicas multidisciplinares
Pequenas redes
```

## 37. Segurança e controle

Comunicar apenas capacidades reais:

``` text
Controle de acesso
Isolamento por clínica
Auditoria
Atendimento humano
Histórico operacional
```

Não declarar certificações inexistentes.

## 38. FAQ

Perguntas iniciais:

``` text
O paciente precisa instalar aplicativo?
O sistema funciona pelo WhatsApp?
Posso assumir uma conversa?
Posso usar vários profissionais?
É possível controlar horários e bloqueios?
Como funciona a implantação?
```

Responder conforme capacidades reais.

## 39. CTA final

``` text
Quer ver o Clinic Assistant funcionando na sua clínica?

Solicite uma demonstração.
```

## 40. Lead

Se houver formulário:

``` text
Nome
Clínica
WhatsApp ou e-mail
```

Exigir backend seguro, validação, rate limiting e proteção anti-spam.

Se ainda não houver infraestrutura, manter CTA configurável e documentar
a pendência.

## 41. Navegação pública

``` text
Logo
Como funciona
Recursos
Produto
FAQ
Entrar
Solicitar demonstração
```

Sticky discreto e menu mobile acessível.

## 42. Interatividade

Permitir apenas animações leves:

-   reveal;
-   hover;
-   transições;
-   tabs/screenshots;
-   demo WhatsApp controlada.

Respeitar `prefers-reduced-motion`.

## 43. SEO

Configurar:

``` text
title
description
Open Graph
Twitter card
canonical quando aplicável
robots
sitemap quando aplicável
```

Páginas privadas não devem ser indexadas.

## 44. Performance

-   imagens otimizadas;
-   lazy loading;
-   evitar JS desnecessário;
-   server components quando adequados;
-   evitar vídeo pesado;
-   minimizar CLS;
-   avaliar Lighthouse.

## 45. Estrutura sugerida

``` text
LandingHeader
HeroSection
WhatsAppDemo
ProblemSection
HowItWorksSection
ProductFeaturesSection
ProductScreenshotsSection
OperationalDiagram
BenefitsSection
SecuritySection
FaqSection
FinalCta
LandingFooter
```

------------------------------------------------------------------------

# PARTE D --- DESIGN SYSTEM E QUALIDADE

## 46. Componentes a padronizar

``` text
Button
Input
Select
DatePicker
Badge
Card
Drawer
Tabs
Tooltip
Skeleton
EmptyState
```

Não criar CSS diferente em cada tela para componentes equivalentes.

## 47. Acessibilidade

Meta:

``` text
WCAG 2.2 AA
```

Aplicar em Agenda, Dashboard e Landing:

-   headings/landmarks;
-   teclado;
-   focus;
-   contraste;
-   labels;
-   alt text;
-   touch targets;
-   reduced motion.

## 48. Responsividade

Validar:

``` text
375
430
768
1024
1280
1440
```

Sem overflow horizontal indevido.

------------------------------------------------------------------------

# PARTE E --- TESTES

## 49. Agenda E2E

Cobrir:

``` text
formulário não aparece por padrão
Nova consulta abre drawer
fechar drawer preserva calendário
quick create funciona
filtros funcionam
Dia/Semana/Mês/Lista continuam funcionando
mobile sem overflow
```

## 50. Dashboard E2E

Cobrir:

``` text
carrega
troca período
KPIs atualizam
gráficos renderizam
fila abre destino
WhatsApp abre destino
próxima consulta abre agenda
empty state
error state
```

## 51. Landing E2E

Cobrir:

``` text
landing carrega
CTA demo
CTA login
anchors
FAQ
demo WhatsApp
screenshots/diagramas
menu mobile
sem overflow
```

## 52. Unit tests

Adicionar somente onde houver lógica:

``` text
dashboard period parser
chart data mapping
dashboard transformations
calendar UI state
landing demo state
```

------------------------------------------------------------------------

# PARTE F --- DOCUMENTAÇÃO

## 53. Criar/atualizar

``` text
docs/frontend/agenda-refinement.md
docs/frontend/dashboard-analytics.md
docs/frontend/landing-page.md
docs/frontend/design-system-guidelines.md
docs/testing/dashboard-e2e.md
docs/testing/landing-e2e.md
docs/product/value-proposition.md
docs/product/demo-flow.md
```

`value-proposition.md` deve registrar público, problema, proposta de
valor, funcionalidades reais, diferenciais atuais e claims proibidos por
falta de evidência.

`demo-flow.md` deve propor uma demonstração de 5--10 minutos:

``` text
Landing
→ WhatsApp
→ Agendamento
→ Agenda
→ Handoff humano
→ Dashboard
→ Auditoria/operação
```

------------------------------------------------------------------------

# PARTE G --- ORDEM DE IMPLEMENTAÇÃO

## 54. Fase 1 --- Auditoria

Concluir diagnóstico antes de mudanças amplas.

## 55. Fase 2 --- Agenda

Implementar ocultação do formulário, botão discreto, drawer/sheet,
padronização de controles, alinhamento, spacing, responsive e regressão.

## 56. Fase 3 --- Dashboard

Implementar filtro temporal, KPIs, tendências, status, ocupação,
WhatsApp, fila, alertas, próximas consultas e drill-down. Evoluir
backend somente quando necessário.

## 57. Fase 4 --- Landing

Implementar layout público, hero, demo WhatsApp, problema, como
funciona, produto, screenshots, diagrama, benefícios, segurança, FAQ,
CTA, footer, SEO e mobile.

## 58. Fase 5 --- Hardening

Acessibilidade, performance, testes, build e documentação.

------------------------------------------------------------------------

# 59. Restrições

Não:

-   reescrever frontend inteiro;
-   trocar framework/design system;
-   alterar regras de negócio sem necessidade;
-   criar métricas fictícias;
-   prometer funcionalidades inexistentes;
-   usar dados reais na landing;
-   copiar layout proprietário pixel a pixel;
-   quebrar multi-tenancy/autorização;
-   remover testes;
-   usar `test.skip`;
-   aumentar timeout para esconder falhas;
-   adicionar dependência sem justificar.

------------------------------------------------------------------------

# 60. Validação técnica

Frontend:

``` bash
npm run lint
npm run typecheck
npm run test
npm run build
```

E2E:

``` bash
npm run test:e2e -- --workers=1
```

Backend quando alterado:

``` bash
dotnet restore
dotnet build
dotnet test
```

------------------------------------------------------------------------

# 61. Critérios absolutos de conclusão

## Agenda

-   formulário oculto;
-   criação sob demanda;
-   Quick Create;
-   controles uniformes;
-   alinhamento;
-   responsividade;
-   regressão zero.

## Dashboard

-   KPIs reais;
-   filtro temporal;
-   gráficos úteis;
-   WhatsApp;
-   fila humana;
-   alertas;
-   próximas consultas;
-   drill-down;
-   loading/error/empty.

## Landing

-   hero;
-   demo WhatsApp;
-   problema;
-   como funciona;
-   recursos;
-   screenshots;
-   diagrama;
-   benefícios;
-   segurança;
-   FAQ;
-   CTA;
-   login;
-   SEO;
-   mobile;
-   acessibilidade.

## Qualidade

-   lint PASS;
-   typecheck PASS;
-   unit PASS;
-   build PASS;
-   backend PASS quando aplicável;
-   E2E PASS;
-   documentação atualizada.

------------------------------------------------------------------------

# 62. Relatório final obrigatório

Apresentar:

1.  diagnóstico inicial;
2.  mudanças da Agenda;
3.  componentes padronizados;
4.  mudanças do Dashboard;
5.  métricas implementadas;
6.  endpoints criados/alterados;
7.  gráficos implementados;
8.  realtime;
9.  landing criada;
10. seções;
11. assets/diagramas;
12. responsividade;
13. acessibilidade;
14. performance;
15. testes;
16. E2E;
17. documentação;
18. dependências e justificativas;
19. pendências;
20. riscos;
21. próximos passos.

Não considerar concluída apenas por melhoria estética.

Resultado esperado:

``` text
mais limpo para operar
+
mais útil para gerir
+
mais fácil de demonstrar e vender
```
