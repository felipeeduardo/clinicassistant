# Etapa 9.4 — Modernização Visual, Administração WhatsApp e Fechamento Operacional

Implemente a Etapa 9.4 do projeto Clinic Assistant.

Esta etapa deverá modernizar visualmente o frontend, melhorar a experiência de uso em desktop, tablet e dispositivos móveis e concluir as pendências operacionais remanescentes da Etapa 9.3.

Utilize como fontes de verdade:

```text
docs/prompts/etapa-9-frontend-operacional-e2e-parte-2.md
docs/prompts/etapa-9-3-hardening-operacional-fechamento-e2e.md
```

Não implemente Inteligência Artificial, RAG ou Tool Calling nesta etapa.

---

## 1. Objetivos

Ao final desta etapa, o sistema deverá possuir:

* layout visual moderno e consistente;
* navegação responsiva;
* menu com ícones;
* formulários melhor organizados;
* botões e ações padronizados;
* tabelas operacionais mais legíveis;
* experiência adequada em dispositivos móveis;
* administração segura da integração Twilio;
* APIs e telas administrativas de templates WhatsApp;
* SignalR cobrindo todos os fluxos operacionais;
* métricas operacionais completas;
* testes E2E completos com ambiente fake;
* CI capaz de subir o ambiente E2E;
* documentação técnica consolidada;
* Postman atualizado;
* roteiro controlado de prontidão para Twilio em produção.

---

# 2. Ordem obrigatória de prioridade

Executar na seguinte ordem:

```text
1. Auditoria visual e de usabilidade
2. Fundação do design system
3. Layout responsivo e navegação
4. Formulários e componentes operacionais
5. Administração segura do Twilio
6. Templates WhatsApp administrativos
7. Ampliação do SignalR
8. Métricas operacionais
9. E2E completo e CI
10. Documentação e Postman
11. Prontidão operacional para produção
12. Revisão das vulnerabilidades npm
```

Não começar pelo refinamento estético isolado sem validar os componentes compartilhados.

---

# 3. Análise inicial obrigatória

Antes de alterar qualquer código:

1. analise todas as rotas do frontend;
2. identifique layouts existentes;
3. identifique componentes duplicados;
4. identifique estilos inconsistentes;
5. identifique formulários com baixa organização visual;
6. identifique telas sem responsividade;
7. identifique tabelas inadequadas para mobile;
8. identifique botões sem padrão;
9. identifique ações sem ícones;
10. identifique estados de loading, erro e vazio ausentes;
11. identifique componentes que não utilizam o design system;
12. analise acessibilidade;
13. analise navegação por teclado;
14. analise contraste;
15. analise o comportamento do menu em telas pequenas;
16. analise a tela atual da integração WhatsApp;
17. analise entidades e configurações Twilio existentes;
18. analise como secrets são armazenados;
19. analise os templates WhatsApp existentes no domínio;
20. analise o SignalR atual;
21. analise métricas existentes;
22. analise testes Playwright;
23. analise o workflow de CI;
24. analise a documentação;
25. analise o Postman;
26. analise o relatório de vulnerabilidades npm;
27. apresente riscos e dependências.

Não altere código antes de apresentar essa análise.

Criar as matrizes:

## Tela x situação visual

| Tela | Problema | Componente afetado | Prioridade | Solução |
| ---- | -------- | ------------------ | ---------- | ------- |

## Componente x padronização

| Componente atual | Duplicações | Componente proposto | Ação |
| ---------------- | ----------- | ------------------- | ---- |

## Pendência operacional

| Pendência | Backend | Frontend | Testes | Documentação | Status |
| --------- | ------- | -------- | ------ | ------------ | ------ |

## SignalR

| Evento | Origem | Destino | Existe? | Alteração necessária |
| ------ | ------ | ------- | ------- | -------------------- |

---

# 4. Direção visual

Criar uma interface:

* moderna;
* limpa;
* profissional;
* adequada ao contexto de clínicas;
* com boa densidade de informação;
* sem excesso de elementos decorativos;
* consistente entre módulos;
* acessível;
* responsiva.

Não copiar interfaces de terceiros.

Não utilizar gradientes, sombras ou animações em excesso.

---

# 5. Design system

Criar ou evoluir componentes compartilhados:

```text
AppShell
Sidebar
MobileNavigation
Header
PageHeader
Breadcrumbs
PageContainer
ContentSection
Card
StatCard
ActionCard
Button
IconButton
ButtonGroup
Input
Textarea
Select
Combobox
MultiSelect
DatePicker
TimePicker
DateRangePicker
Checkbox
RadioGroup
Switch
FormField
FormSection
FormActions
FormErrorSummary
Dialog
ConfirmDialog
Drawer
Sheet
Tabs
Badge
StatusBadge
Avatar
Tooltip
DropdownMenu
CommandMenu
Table
DataTable
Pagination
FilterBar
SearchInput
EmptyState
ErrorState
LoadingState
Skeleton
Toast
Alert
Timeline
ActivityLog
```

Cada componente deverá possuir:

* estado padrão;
* hover;
* focus;
* disabled;
* loading;
* success;
* warning;
* error;
* suporte a teclado;
* atributos de acessibilidade.

Evitar componentes criados apenas para uma única página quando um componente genérico for suficiente.

---

# 6. Tokens visuais

Centralizar:

```text
spacing
border radius
font sizes
font weights
shadows
z-index
breakpoints
container widths
icon sizes
transition durations
```

Utilizar os mecanismos já adotados pelo Tailwind ou biblioteca existente.

Não espalhar valores arbitrários pelas páginas.

---

# 7. Botões

Padronizar variantes:

```text
Primary
Secondary
Outline
Ghost
Danger
Success
Link
Icon
```

Tamanhos:

```text
Small
Medium
Large
IconOnly
```

Regras:

* ações principais com destaque;
* somente uma ação primária por seção;
* ações destrutivas com confirmação;
* ícone acompanhado de texto quando a ação não for óbvia;
* tooltip em botão somente com ícone;
* loading sem alteração brusca de largura;
* impedir duplo clique em mutations.

Exemplos:

```text
Novo paciente
Salvar alterações
Criar consulta
Assumir conversa
Enviar mensagem
Reagendar
Cancelar consulta
Sincronizar templates
Validar integração
```

---

# 8. Ícones

Adicionar ícones ao menu e às principais ações.

Utilizar uma única biblioteca de ícones já existente ou uma biblioteca leve compatível.

Sugestão de associação:

```text
Dashboard → gráfico ou painel
Conversas → mensagens
Fila → usuários ou inbox
Pacientes → usuário
Profissionais → identificação ou estetoscópio administrativo
Especialidades → categorias
Agenda → calendário
Integrações → conexão
Templates → arquivo ou mensagem
Auditoria → histórico
Configurações → engrenagem
Usuários → grupo
Tenants → edifício
Unidades → localização
```

Não misturar bibliotecas de ícones sem necessidade.

---

# 9. Layout líquido e responsivo

Implementar layout fluido com:

* largura adaptável;
* containers com limites coerentes;
* sidebar recolhível;
* menu mobile em drawer;
* header responsivo;
* tabelas adaptáveis;
* cards reorganizados;
* formulários em uma ou múltiplas colunas conforme largura;
* composer de conversa fixo quando necessário;
* modais substituídos por drawers em mobile quando apropriado.

Breakpoints mínimos:

```text
Mobile
Tablet
Notebook
Desktop
Wide desktop
```

Prioridade operacional:

```text
Desktop
Notebook
Tablet
Mobile
```

Todas as telas devem ser utilizáveis em mobile, ainda que operações complexas possam utilizar fluxos simplificados.

---

# 10. Navegação

Evoluir o menu lateral para possuir:

* ícones;
* agrupamento por domínio;
* item ativo;
* suporte a colapso;
* tooltips no estado recolhido;
* itens filtrados por permissão;
* indicador do tenant;
* indicador da integração;
* botão de sair;
* navegação mobile.

Agrupamento sugerido:

```text
Visão geral
- Dashboard

Atendimento
- Conversas
- Fila humana
- Pacientes

Agenda
- Agenda
- Profissionais
- Especialidades
- Unidades

Administração
- Tenants
- Clínicas
- Usuários

Integrações
- WhatsApp
- Templates

Governança
- Auditoria
- Configurações
```

Não exibir itens sem permissão.

---

# 11. Cabeçalho de página

Cada tela deverá possuir:

* título;
* descrição curta;
* breadcrumbs;
* ação principal;
* ações secundárias;
* contexto atual;
* status quando aplicável.

Exemplo:

```text
Pacientes
Gerencie os dados administrativos e o histórico de atendimento dos pacientes.

[Novo paciente]
```

---

# 12. Formulários

Reorganizar os formulários utilizando:

* seções;
* títulos;
* descrições;
* agrupamento semântico;
* grid responsivo;
* labels persistentes;
* textos de ajuda;
* mensagens por campo;
* resumo de erros;
* ações fixas ou claramente posicionadas;
* confirmação ao sair com mudanças não salvas.

Estrutura sugerida:

```text
PageHeader

Form
├── Informações principais
├── Contato
├── Configurações
├── Relacionamentos
└── Ações
```

Não criar formulários longos em uma única coluna sem seções.

---

# 13. Formulários prioritários

Refinar visualmente:

* onboarding de tenant;
* clínica;
* unidade;
* usuário;
* paciente;
* profissional;
* especialidade;
* disponibilidade;
* bloqueio;
* férias;
* consulta;
* reagendamento;
* cancelamento;
* integração WhatsApp;
* template WhatsApp;
* configurações.

Cada formulário deverá suportar:

* criação;
* edição;
* loading;
* validação;
* erro 409;
* erro 422;
* sucesso;
* cancelamento;
* confirmação destrutiva;
* responsividade.

---

# 14. Tabelas operacionais

Evoluir tabelas com:

* cabeçalho fixo quando necessário;
* paginação;
* filtros;
* busca;
* ordenação;
* seleção;
* ações por linha;
* estado vazio;
* skeleton;
* erro;
* densidade adequada;
* colunas responsivas;
* menu de ações no mobile.

Não renderizar centenas de registros sem paginação ou virtualização.

---

# 15. Cards e dashboard

Modernizar cards de métricas:

* título;
* valor;
* contexto;
* variação quando disponível;
* ícone;
* estado;
* link para detalhe.

Não inventar tendências ou percentuais que o backend não fornece.

---

# 16. Conversas

Modernizar a tela para se aproximar de uma ferramenta de atendimento profissional:

```text
Lista de conversas
Painel de mensagens
Painel de contexto
```

Melhorias:

* avatars;
* badges;
* status visual;
* mensagens não lidas;
* prioridade;
* atendente;
* automação;
* busca;
* filtros;
* composer responsivo;
* ações operacionais agrupadas;
* status de envio;
* timestamps;
* separadores de data;
* loading de histórico;
* botão de carregar mensagens anteriores.

No mobile:

* lista e conversa em rotas ou painéis separados;
* composer fixo;
* ações em drawer ou menu.

---

# 17. Agenda

Modernizar:

* toolbar;
* filtros;
* navegação de datas;
* visualização dia, semana e lista;
* cards de consulta;
* status;
* conflitos;
* ações rápidas;
* detalhe em drawer;
* formulário responsivo.

Drag and drop somente se o backend e os testes suportarem corretamente concorrência e rollback.

---

# 18. Formulário administrativo do Twilio

Criar tela segura em:

```text
/settings/integrations/twilio
```

ou rota equivalente já adotada.

A tela deverá permitir configurar:

```text
Account SID
Auth Token
WhatsApp From
Incoming Webhook Base URL
Status Callback Base URL
Environment
Signature Validation
Enabled
```

## Regras de segurança obrigatórias

O frontend:

* não pode recuperar o Auth Token salvo;
* não pode exibir Auth Token existente;
* não pode armazenar Auth Token em localStorage;
* não pode enviar Auth Token para telemetry;
* não pode registrar Auth Token no console;
* deve limpar o campo após envio;
* deve exibir somente indicador de credencial configurada;
* deve exigir permissão administrativa;
* deve utilizar HTTPS em produção;
* deve enviar a configuração somente ao backend.

O backend:

* deve receber o token por DTO específico;
* deve criptografar o token em repouso ou enviar ao secret manager;
* nunca deve retornar o token;
* deve mascarar o Account SID e o sender nas respostas;
* deve auditar alteração;
* deve registrar apenas que a credencial foi alterada;
* deve suportar rotação;
* deve permitir remover ou substituir a credencial;
* deve validar configuração sem expor detalhes;
* deve impedir operação cross-tenant.

Resposta administrativa sugerida:

```json
{
  "provider": "Twilio",
  "accountSidMasked": "AC********************3e0e",
  "whatsAppFromMasked": "whatsapp:+55******5348",
  "authTokenConfigured": true,
  "environment": "Production",
  "signatureValidationEnabled": true,
  "enabled": false,
  "lastValidatedAt": null
}
```

Nunca retornar:

```text
authToken
authTokenEncrypted
secretReference
```

---

# 19. Endpoints de configuração Twilio

Criar ou evoluir endpoints equivalentes a:

```text
GET  /api/v1/admin/integrations/twilio/configuration
PUT  /api/v1/admin/integrations/twilio/configuration
POST /api/v1/admin/integrations/twilio/validate
POST /api/v1/admin/integrations/twilio/rotate-credentials
POST /api/v1/admin/integrations/twilio/enable
POST /api/v1/admin/integrations/twilio/disable
```

Considerar que secrets de infraestrutura podem não ser apropriados para armazenamento no banco.

Antes de implementar, decidir e documentar:

```text
Secret manager
Encrypted database field
Platform-level environment variables
Tenant-level credentials
Twilio subaccounts
```

Não criar armazenamento inseguro apenas para facilitar a interface.

---

# 20. Validação Twilio

A tela deve apresentar checks sanitizados:

```text
Account SID configurado
Auth Token configurado
Sender configurado
Webhook HTTPS
Status callback HTTPS
Assinatura habilitada
Conexão com o provider
Sender autorizado
Templates sincronizados
```

Não mostrar resposta bruta da Twilio.

---

# 21. Templates WhatsApp administrativos

## Problema atual

O domínio e o envio de templates existem, mas faltam:

* APIs;
* tela;
* sincronização;
* ativação;
* desativação;
* criação;
* edição.

## Backend

Implementar:

```text
GET    /api/v1/admin/whatsapp/templates
GET    /api/v1/admin/whatsapp/templates/{templateId}
POST   /api/v1/admin/whatsapp/templates
PUT    /api/v1/admin/whatsapp/templates/{templateId}
POST   /api/v1/admin/whatsapp/templates/{templateId}/activate
POST   /api/v1/admin/whatsapp/templates/{templateId}/deactivate
POST   /api/v1/admin/whatsapp/templates/sync
```

Filtros:

```text
page
pageSize
search
status
languageCode
category
provider
integrationId
```

Requisitos:

* TenantId;
* autorização;
* validação de variáveis;
* ContentSid;
* status do provider;
* concorrência;
* auditoria;
* sync assíncrono;
* Outbox quando apropriado;
* nenhum SDK no controller.

## Frontend

Criar:

```text
/integrations/whatsapp/templates
/integrations/whatsapp/templates/new
/integrations/whatsapp/templates/{id}
/integrations/whatsapp/templates/{id}/edit
```

Exibir:

* nome;
* provider;
* categoria;
* idioma;
* status local;
* status remoto;
* ContentSid mascarado quando apropriado;
* variáveis;
* última sincronização;
* falha sanitizada.

Criar preview do conteúdo com dados fictícios.

---

# 22. SignalR completo

## Problema atual

O Hub e eventos básicos existem, mas faltam eventos operacionais.

## Envelope obrigatório

Evoluir para:

```csharp
public sealed record RealtimeEvent<T>(
    string EventId,
    string EventType,
    Guid TenantId,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    long? ResourceVersion,
    T Data);
```

Todos os eventos devem possuir:

* EventId;
* EventType;
* TenantId;
* OccurredAt;
* CorrelationId;
* ResourceVersion quando aplicável;
* payload sanitizado.

## Eventos pendentes

Implementar:

```text
whatsapp.inbound.received
whatsapp.message.status.changed
whatsapp.template.created
whatsapp.template.updated
whatsapp.template.activated
whatsapp.template.deactivated
whatsapp.template.synced

audit.created

queue.item.created
queue.item.updated
queue.item.assigned
queue.item.released
queue.item.transferred
queue.item.completed

dashboard.invalidated
```

Publicar somente após commit.

Não publicar payload bruto do webhook.

---

# 23. SignalR no frontend

Criar mapeamento central:

```text
evento
    ↓
query keys afetadas
    ↓
estratégia de atualização
```

Regras:

* deduplicar por EventId;
* validar tenant;
* atualizar cache específico;
* invalidar apenas quando necessário;
* reconectar com backoff;
* sincronizar após reconexão;
* exibir indicador de conexão;
* encerrar conexão ao trocar tenant;
* não duplicar mensagens.

---

# 24. Métricas operacionais

Implementar métricas pendentes:

```text
authorization_denied_total
platform_onboarding_total
platform_onboarding_failures_total
appointments_rescheduled_total
appointment_conflicts_total
manual_messages_total
audit_entries_total
dashboard_requests_total
dashboard_request_duration
signalr_connections_active
signalr_events_published_total
signalr_publish_failures_total
refresh_token_rotations_total
refresh_token_reuse_detected_total
whatsapp_template_sync_total
whatsapp_template_sync_failures_total
twilio_configuration_validations_total
twilio_configuration_failures_total
```

Evitar labels de alta cardinalidade.

Não usar IDs, telefone ou conteúdo como labels.

---

# 25. E2E completo com ambiente fake

Criar cenários Playwright para:

## Administração

* criar tenant;
* editar tenant;
* onboarding;
* criar unidade;
* criar usuário.

## Pacientes

* criar;
* buscar;
* editar;
* visualizar detalhe;
* isolamento multi-tenant.

## Profissionais

* criar;
* disponibilidade;
* bloqueio;
* férias.

## Especialidades

* criar;
* editar;
* dependência;
* desativação.

## Agenda

* criar consulta;
* confirmar;
* reagendar;
* conflito;
* cancelar;
* detalhe;
* filtros.

## Conversas

* inbound fake;
* fila;
* assumir;
* transferir;
* liberar;
* enviar mensagem;
* pausar;
* retomar;
* prioridade;
* encerrar;
* reabrir.

## WhatsApp

* status;
* configuração sem secret exposto;
* validar integração;
* templates;
* sincronização;
* FakeWhatsAppGateway;
* status de mensagem.

## Realtime

* nova conversa;
* nova mensagem;
* status;
* fila;
* auditoria;
* dashboard.

---

# 26. CI com ambiente fake

Evoluir `.github/workflows/ci.yml`.

Pipeline:

```text
Checkout
Setup .NET
Setup Node
Restore backend
Build backend
Test backend
Install frontend
Lint
Typecheck
Test frontend
Build frontend
Start PostgreSQL
Start Redis
Start RabbitMQ
Apply migrations
Reset E2E
Seed E2E
Validate E2E
Start API
Start Worker
Start frontend
Wait for health checks
Run Playwright
Collect traces
Collect screenshots
Collect logs
Shutdown
```

Requisitos:

* FakeWhatsAppGateway;
* nenhum acesso à Twilio;
* banco isolado;
* timeout;
* health checks;
* artifacts em falha;
* secrets não expostos.

---

# 27. Smoke real Twilio

Criar workflow manual separado:

```text
manual_twilio_smoke
```

Requisitos:

* execução manual;
* ambiente protegido;
* secrets do CI;
* número em allowlist;
* limite de mensagens;
* confirmação explícita;
* timeout;
* cleanup;
* relatório;
* nenhum dado real de paciente;
* nenhuma execução em pull request.

---

# 28. Documentação de APIs

Criar ou consolidar:

```text
docs/api/authorization.md
docs/api/platform-administration.md
docs/api/patients.md
docs/api/scheduling.md
docs/api/conversations.md
docs/api/whatsapp.md
docs/api/realtime.md
docs/api/audit.md
docs/api/dashboard.md
```

Criar guia:

```text
docs/testing/playwright.md
```

Não manter documentos duplicados com nomes diferentes.

---

# 29. Postman

Atualizar a collection para incluir:

* pacientes completos;
* agenda completa;
* reagendamento;
* conversas;
* fila;
* mensagem manual;
* prioridade;
* templates;
* sincronização;
* configuração Twilio sanitizada;
* auditoria;
* dashboard;
* refresh;
* fluxos E2E.

Não armazenar Auth Token da Twilio no arquivo versionado.

---

# 30. OpenAPI

Caso a governança exija OpenAPI estático, gerar automaticamente:

```text
openapi/clinic-assistant-v1.json
```

ou YAML equivalente.

Não editar manualmente.

Validar drift no CI.

Caso não seja requisito, registrar a decisão arquitetural.

---

# 31. Vulnerabilidades npm

Executar:

```bash
npm audit
```

As três vulnerabilidades altas transitivas restantes deverão ser:

* identificadas;
* relacionadas ao pacote de origem;
* comparadas com versões corrigidas;
* avaliadas quanto a impacto real;
* acompanhadas.

Não executar:

```bash
npm audit fix --force
```

sem análise.

Atualizar:

```text
docs/security/npm-audit-report.md
```

Classificar como:

```text
Resolved
Mitigated
Accepted Temporarily
Blocked by Upstream
```

---

# 32. CSP

Validar a Content Security Policy com:

* URL real do frontend;
* URL real da API;
* URL do SignalR;
* recursos necessários;
* ambiente de build.

Revisar:

```text
connect-src
script-src
style-src
img-src
font-src
frame-src
```

Não utilizar wildcard amplo sem justificativa.

Criar documentação:

```text
docs/security/csp.md
```

---

# 33. Prontidão Twilio para produção

Criar checklist operacional:

```text
Credenciais rotacionadas
Secrets fora do repositório
HTTPS público
Sender autorizado
Inbound webhook configurado
Status callback configurado
X-Twilio-Signature validada
Templates aprovados
ContentSid sincronizado
Número de teste permitido
Logs sanitizados
Métricas ativas
Alertas configurados
Smoke real executado
Rollback documentado
```

Criar:

```text
docs/operations/twilio-production-readiness.md
```

---

# 34. Acessibilidade

Validar:

* teclado;
* foco;
* contraste;
* labels;
* ARIA;
* dialogs;
* drawers;
* menus;
* tabelas;
* formulários;
* mensagens de erro.

Meta:

```text
WCAG 2.2 AA
```

nas telas principais.

---

# 35. Performance

Avaliar:

* bundle;
* carregamento inicial;
* tabelas;
* listas de mensagens;
* calendário;
* formulários;
* SignalR;
* invalidações.

Aplicar:

* lazy loading;
* code splitting;
* virtualização;
* paginação;
* cache;
* debounce;
* cancelamento de requests;
* memoização apenas quando necessária.

---

# 36. Critérios de aceite

A Etapa 9.4 somente estará concluída quando:

```text
1. Layout estiver modernizado
2. Menu possuir ícones
3. Menu mobile funcionar
4. Layout líquido funcionar
5. Formulários estiverem reorganizados
6. Botões estiverem padronizados
7. Tabelas estiverem responsivas
8. Dashboard estiver modernizado
9. Conversas estiverem responsivas
10. Agenda estiver modernizada
11. Design system estiver consolidado
12. Tela segura de configuração Twilio existir
13. Auth Token não for retornado
14. Auth Token não for persistido no navegador
15. Backend armazenar secret com segurança
16. Templates WhatsApp possuírem APIs
17. Templates WhatsApp possuírem telas
18. Sincronização funcionar
19. SignalR possuir envelope completo
20. Inbound WhatsApp publicar evento
21. Status callback publicar evento
22. Templates publicarem eventos
23. Auditoria publicar evento
24. Fila publicar eventos
25. Dashboard publicar invalidação
26. Métricas operacionais existirem
27. Playwright cobrir CRUD
28. Playwright cobrir agenda
29. Playwright cobrir conversas
30. Playwright cobrir realtime
31. Playwright cobrir multi-tenancy
32. Playwright cobrir FakeWhatsApp
33. CI subir ambiente fake
34. CI aplicar seed
35. CI executar Playwright
36. Smoke Twilio estiver separado
37. Documentação de APIs estiver completa
38. Postman estiver completo
39. Vulnerabilidades npm estiverem classificadas
40. CSP estiver validada
41. Prontidão Twilio estiver documentada
42. Nenhum secret estiver exposto
43. Build e testes passarem
```

---

# 37. Ordem de implementação incremental

## 9.4.1 — Auditoria visual

* inventário;
* telas;
* componentes;
* acessibilidade;
* responsividade.

## 9.4.2 — Design system

* tokens;
* botões;
* inputs;
* formulários;
* tabelas;
* estados.

## 9.4.3 — Layout responsivo

* AppShell;
* sidebar;
* menu mobile;
* headers;
* containers.

## 9.4.4 — Modernização das telas

* dashboard;
* cadastros;
* agenda;
* conversas;
* integrações.

## 9.4.5 — Twilio e templates

* configuração segura;
* APIs;
* telas;
* sincronização.

## 9.4.6 — SignalR e métricas

* envelope;
* eventos;
* frontend;
* métricas.

## 9.4.7 — E2E e CI

* Playwright;
* ambiente fake;
* artifacts;
* smoke real.

## 9.4.8 — Segurança e documentação

* npm audit;
* CSP;
* Postman;
* OpenAPI;
* readiness.

---

# 38. Primeira entrega

Implemente inicialmente somente:

```text
9.4.1 — Auditoria visual
9.4.2 — Fundação do design system
9.4.3 — Layout responsivo
9.4.4 — Modernização das telas prioritárias
```

Telas prioritárias:

```text
Login
Dashboard
Pacientes
Profissionais
Especialidades
Unidades
Agenda
Conversas
Fila humana
WhatsApp
```

A primeira entrega deverá conter:

1. relatório visual;
2. matriz de componentes;
3. tokens visuais;
4. botões;
5. campos;
6. seções de formulário;
7. tabela;
8. estados;
9. sidebar com ícones;
10. menu mobile;
11. PageHeader;
12. layout líquido;
13. dashboard modernizado;
14. pacientes modernizado;
15. profissionais modernizado;
16. especialidades modernizado;
17. unidades modernizado;
18. agenda modernizada;
19. conversas modernizadas;
20. testes de componentes;
21. testes responsivos;
22. acessibilidade básica;
23. documentação visual.

Não implementar ainda:

* armazenamento de credenciais Twilio;
* APIs de templates;
* ampliação SignalR;
* métricas;
* CI E2E;
* smoke real;

até que o design system e os layouts prioritários estejam estáveis.

---

# 39. Validação da primeira entrega

Executar:

```bash
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```

Executar Playwright somente para smoke visual e navegação quando já configurado.

Validar em larguras equivalentes a:

```text
375px
768px
1024px
1440px
```

Corrigir:

* overflow horizontal;
* botões inacessíveis;
* formulários quebrados;
* tabelas ilegíveis;
* menu sobreposto;
* foco invisível;
* contraste inadequado.

---

# 40. Relatório final

Ao finalizar cada incremento, apresentar:

1. telas alteradas;
2. componentes criados;
3. componentes removidos;
4. duplicações eliminadas;
5. melhorias responsivas;
6. melhorias de acessibilidade;
7. endpoints criados;
8. eventos SignalR criados;
9. métricas adicionadas;
10. testes executados;
11. resultado do npm audit;
12. riscos restantes;
13. pendências externas;
14. próximos passos.

Não avance automaticamente para o próximo incremento.
