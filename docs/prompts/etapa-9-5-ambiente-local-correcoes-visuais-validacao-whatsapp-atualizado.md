# Etapa 9.5 — Ambiente Local, Correções Visuais e Validação WhatsApp

## Contexto

O ngrok já está instalado e funcionando. A Etapa 9.5 deverá padronizar um ambiente local de fácil gerenciamento, corrigir as telas de clínicas e auditoria e diagnosticar o erro apresentado ao enviar uma mensagem de teste pela integração WhatsApp:

> Não foi possível concluir a operação. Tente novamente.

O ambiente deverá suportar desenvolvimento sem custos com `FakeWhatsAppGateway` e testes manuais controlados com Twilio Sandbox.

A Inteligência Artificial permanece fora do escopo.

---

## 1. Objetivos

Ao final desta etapa deverá ser possível:

- subir e encerrar o ambiente local com poucos comandos;
- escolher entre `Fake`, `E2E` e `Twilio Sandbox`;
- iniciar o ngrok e descobrir a URL pública HTTPS;
- exibir URLs de inbound webhook e status callback;
- validar todos os serviços por health checks;
- aplicar migrations, resetar e popular dados E2E;
- corrigir visualmente as telas de clínicas e auditoria;
- modernizar a agenda com um calendário operacional fluido, responsivo e seguro;
- diagnosticar a causa raiz do erro no envio de teste;
- criar mensagem de teste pela Outbox;
- acompanhar o processamento pelo Worker;
- apresentar erros sanitizados, específicos e com `traceId`;
- executar smoke tests e Playwright;
- manter Twilio real desabilitado no CI.

---

## 2. Escopo

```text
9.5.1 Auditoria do ambiente local
9.5.2 Perfis Docker Compose
9.5.3 Scripts de gerenciamento
9.5.4 Integração com ngrok
9.5.5 Health checks e diagnóstico
9.5.6 Correção visual de clínicas
9.5.7 Correção visual de auditoria
9.5.8 Calendário operacional moderno
9.5.9 Diagnóstico do envio WhatsApp
9.5.10 Correção do fluxo de teste
9.5.11 Observabilidade
9.5.12 Smoke tests e Playwright
9.5.13 Documentação
```

---

## 3. Princípios obrigatórios

- `FakeWhatsAppGateway` deve ser o padrão local e no CI;
- Twilio deve exigir ativação explícita;
- ngrok deve ser opcional;
- nenhuma credencial pode ser versionada;
- Auth Token nunca pode ser retornado ao frontend;
- nenhum controller pode chamar Twilio diretamente;
- toda mensagem de teste deve utilizar `ConversationMessage` e `OutboxMessage`;
- o Worker deve executar o envio;
- erros HTTP devem usar `ProblemDetails`;
- o frontend deve mapear códigos de erro conhecidos;
- mensagens genéricas devem ser usadas apenas como fallback;
- as telas devem respeitar o design system;
- scripts destrutivos devem bloquear produção;
- nenhuma execução automática do CI pode enviar mensagem real.

---

## 4. Análise inicial obrigatória

Antes de alterar qualquer arquivo:

1. analise todos os arquivos Docker Compose;
2. liste serviços, portas, volumes, profiles e health checks;
3. analise scripts de start, stop, seed, reset e validação;
4. identifique as portas reais de API e frontend;
5. identifique como API, Worker, Redis, RabbitMQ e PostgreSQL são iniciados;
6. identifique como o provider WhatsApp é resolvido;
7. analise o suporte atual a ngrok;
8. analise a tela de clínicas;
9. identifique overflow, grids quebrados, botões, filtros e problemas mobile;
10. analise a tela de auditoria;
11. identifique tabela, filtros, drawer, paginação e problemas mobile;
12. analise o formulário de envio de teste WhatsApp;
13. identifique endpoint, método, payload, headers e permission;
14. identifique o tratamento do erro no API Client e na tela;
15. identifique o handler de backend;
16. identifique a criação de mensagem e Outbox;
17. identifique o consumo pelo Worker;
18. identifique o gateway resolvido;
19. identifique logs, métricas e traces;
20. verifique provider desabilitado, configuração inválida, sender, telefone, allowlist, limites e idempotência;
21. verifique se o Worker ou RabbitMQ estão indisponíveis;
22. verifique se `ProblemDetails` está sendo descartado;
23. apresente riscos e dependências;
24. não altere código antes de concluir a análise.

Produza:

### Matriz de serviços

| Serviço | Porta | Health check | Dependências | Profile |
|---|---:|---|---|---|

### Matriz visual

| Tela | Problema | Componente | Causa provável | Correção |
|---|---|---|---|---|

### Matriz do envio WhatsApp

| Etapa | Componente | Resultado atual | Possível falha | Evidência |
|---|---|---|---|---|

### Matriz de erros

| Status/Código | Backend | Frontend atual | Mensagem correta |
|---|---|---|---|

---

## 5. Perfis de ambiente

Criar ou consolidar:

### `local`

```text
FakeWhatsAppGateway
Twilio desabilitado
ngrok desabilitado
PostgreSQL
Redis
RabbitMQ
API
Worker
Frontend
```

Objetivo: desenvolvimento diário, custo zero e sem dependência de internet.

### `e2e`

```text
FakeWhatsAppGateway
Twilio desabilitado
seed determinístico
Playwright
PostgreSQL isolado
Redis
RabbitMQ
API
Worker
Frontend
```

Objetivo: testes repetíveis sem chamadas externas.

### `twilio-smoke`

```text
Twilio Sandbox
ngrok
allowlist obrigatória
limite diário reduzido
API
Worker
PostgreSQL
Redis
RabbitMQ
Frontend
```

Objetivo: teste manual controlado. Não executar automaticamente no CI.

---

## 6. Docker Compose

Garantir serviços equivalentes aos nomes reais do projeto:

```text
postgres
redis
rabbitmq
api
worker
frontend
test-data-seeder
ngrok opcional
```

Requisitos:

- health checks;
- dependências baseadas em saúde;
- profiles claros;
- nenhum secret em arquivos versionados;
- logs acessíveis;
- nenhuma chamada real nos profiles `local` e `e2e`;
- `FakeWhatsAppGateway` explicitamente configurado no CI.

---

## 7. Scripts de gerenciamento

Criar ou evoluir:

```text
scripts/local/start-local.sh
scripts/local/start-e2e.sh
scripts/local/start-twilio-smoke.sh
scripts/local/stop.sh
scripts/local/status.sh
scripts/local/logs.sh
scripts/local/reset-e2e.sh
scripts/local/validate.sh
scripts/local/open-app.sh
```

### `start-local.sh`

Deve:

1. validar Docker;
2. validar variáveis obrigatórias;
3. validar conflitos de porta;
4. subir infraestrutura;
5. aplicar migrations;
6. subir API, Worker e frontend;
7. aguardar health checks;
8. mostrar URLs;
9. mostrar provider ativo.

### `start-e2e.sh`

Deve:

1. subir infraestrutura E2E;
2. aplicar migrations;
3. executar reset, seed e validate;
4. subir aplicação;
5. aguardar health checks;
6. mostrar comando para Playwright.

### `start-twilio-smoke.sh`

Deve:

1. exigir confirmação explícita;
2. validar credenciais sem exibi-las;
3. validar allowlist e limites;
4. subir infraestrutura;
5. iniciar ngrok;
6. obter URL pública;
7. exibir inbound webhook e status callback;
8. validar provider efetivo;
9. não enviar mensagem automaticamente;
10. informar o próximo passo manual.

---

## 8. Integração com ngrok

Criar configuração equivalente a:

```env
NGROK__ENABLED=false
NGROK__AUTHTOKEN=
NGROK__DOMAIN=
NGROK__API_PORT=4040
NGROK__TARGET=http://localhost:8080
```

Ajustar a porta ao projeto real.

O authtoken não pode ser versionado.

Criar script para consultar:

```text
http://127.0.0.1:4040/api/tunnels
```

e extrair a URL HTTPS.

Salvar a URL apenas em arquivo temporário ignorado pelo Git:

```text
.tmp/ngrok-url
```

Exibir:

```text
Inbound webhook:
https://<ngrok>/api/webhooks/twilio/whatsapp/inbound

Status callback:
https://<ngrok>/api/webhooks/twilio/whatsapp/status
```

Não modificar arquivos versionados com a URL temporária.

---

## 9. Health checks

Criar ou validar:

```text
/health
/health/live
/health/ready
```

Cobrir:

- PostgreSQL;
- Redis;
- RabbitMQ;
- Outbox;
- Worker;
- provider WhatsApp;
- configuração Twilio;
- SignalR quando aplicável.

Health checks não podem enviar mensagens reais.

---

## 10. Diagnóstico local no frontend

Criar somente para Development/Test e usuários autorizados:

```text
/settings/development
```

ou rota equivalente.

Exibir:

- ambiente;
- provider efetivo;
- API;
- Worker;
- PostgreSQL;
- Redis;
- RabbitMQ;
- ngrok;
- URL pública;
- inbound webhook;
- status callback;
- seed;
- tenant;
- integração;
- allowlist;
- limites;
- última mensagem de teste.

Nunca exibir secrets.

---

## 11. Correção visual — Clínicas

Corrigir a tela de clínicas e alinhar ao design system.

Validar:

- overflow horizontal;
- container;
- grid;
- cards;
- DataTable;
- filtros;
- botões;
- formulários;
- ações por linha;
- dialogs e drawers;
- loading;
- empty state;
- erro;
- responsividade.

Estrutura sugerida:

```text
PageHeader
FilterBar
ClinicCards ou DataTable
Pagination
```

No detalhe:

```text
Resumo
Dados cadastrais
Unidades
Usuários
Integrações
Status
Auditoria resumida
```

Não alterar regras de negócio.

---

## 12. Correção visual — Auditoria

Corrigir a tela `/audit`.

Implementar:

- filtros responsivos;
- tabela ou timeline;
- paginação;
- busca;
- badges;
- correlation ID copiável;
- detalhes em drawer;
- timestamp;
- usuário;
- ação;
- recurso;
- resultado;
- loading;
- empty state;
- erro;
- layout mobile.

Em mobile:

- usar cards ou lista;
- ocultar colunas secundárias;
- abrir detalhes em drawer;
- evitar scroll horizontal obrigatório.

Não exibir payload integral ou dados sensíveis.

---

## 13. Calendário operacional moderno

### Objetivo

Evoluir a agenda atual para um calendário operacional moderno, claro, fluido e responsivo, adequado ao uso diário de recepcionistas, gestores e profissionais.

Nesta etapa, “calendário editorial” significa uma experiência visual rica de calendário, com organização por período, profissionais, unidades, especialidades e estados. As regras de disponibilidade, conflito, concorrência e reagendamento deverão permanecer no backend.

### Rotas e navegação

Criar ou evoluir:

```text
/scheduling
```

As visualizações podem ser controladas por rota, query string ou estado, conforme o padrão atual:

```text
Dia
Semana
Mês
Lista
```

Não criar rotas redundantes quando a aplicação já possuir estratégia adequada.

### Visualização diária

Implementar:

- linha do tempo por horário;
- agrupamento por profissional, unidade ou recurso;
- indicação do horário atual;
- slots livres, ocupados e bloqueados;
- consultas sobrepostas destacadas;
- navegação para dia anterior, hoje e próximo dia;
- abertura do detalhe da consulta;
- carregamento apenas do intervalo necessário.

### Visualização semanal

Implementar:

- colunas por dia;
- linhas por horário;
- filtros por profissional, unidade e especialidade;
- duração visual proporcional quando tecnicamente viável;
- bloqueios, férias e indisponibilidades;
- navegação entre semanas;
- comportamento adequado em notebook e tablet.

### Visualização mensal

Implementar:

- resumo por dia;
- quantidade de consultas;
- indicadores de ocupação;
- dias sem atendimento;
- clique no dia para abrir a visualização diária;
- não tentar exibir todos os detalhes dentro de cada célula;
- carregamento agregado quando o backend fornecer resumo mensal.

### Visualização em lista

Implementar:

- paginação server-side;
- busca;
- filtros;
- ordenação;
- experiência principal para dispositivos móveis;
- ações por linha;
- abertura de detalhe;
- exportação somente quando endpoint existente.

### Toolbar

Criar toolbar com:

- período atual;
- botão Hoje;
- anterior;
- próximo;
- seletor de visualização;
- filtros;
- busca por paciente;
- botão Nova consulta;
- botão Atualizar;
- timezone;
- indicador realtime;
- indicador offline ou dados desatualizados.

### Filtros

Implementar filtros por:

```text
Profissional
Especialidade
Unidade
Status
Origem
Paciente
Período
```

Quando disponível:

```text
Fila
Canal de origem
Confirmação
```

Requisitos:

- filtros server-side;
- sincronização com URL quando útil;
- preservação ao abrir e voltar do detalhe;
- debounce na busca;
- cancelamento da requisição anterior;
- botão para limpar filtros;
- contador de filtros ativos;
- nenhuma filtragem de grandes volumes apenas no navegador.

### Representação das consultas

Cada evento deverá exibir, conforme o espaço:

- horário;
- paciente;
- profissional;
- especialidade;
- unidade;
- status;
- origem;
- confirmação;
- conflito;
- conversa vinculada.

Usar badges e ícones do design system.

Não depender somente de cor.

### Estados

Representar:

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

Ações indisponíveis deverão possuir motivo visível ou tooltip acessível.

### Detalhe da consulta

Abrir detalhe em drawer, sheet ou página responsiva contendo:

- identificador;
- paciente;
- profissional;
- especialidade;
- unidade;
- data e horário;
- timezone;
- status;
- origem;
- `Version`;
- conversa vinculada;
- histórico resumido;
- ações permitidas.

Ações:

```text
Confirmar
Reagendar
Cancelar
Abrir paciente
Abrir conversa
Copiar identificador
```

### Criação de consulta

Implementar fluxo:

```text
Paciente
    ↓
Especialidade
    ↓
Profissional
    ↓
Unidade
    ↓
Data
    ↓
Disponibilidade
    ↓
Slot
    ↓
Confirmação
```

Requisitos:

- React Hook Form;
- Zod;
- revalidação do slot;
- `Idempotency-Key`;
- tratamento de 409;
- preservação do formulário em erro;
- timezone explícito;
- loading e sucesso;
- atualização do calendário;
- atualização da conversa quando aplicável.

### Reagendamento

Implementar:

```text
Consulta atual
    ↓
Novo período
    ↓
Nova disponibilidade
    ↓
Novo slot
    ↓
Comparação
    ↓
Confirmação
```

Exibir:

- horário atual;
- novo horário;
- profissional atual e novo;
- unidade atual e nova;
- motivo opcional.

Enviar:

```text
expectedVersion
Idempotency-Key
```

Em conflito:

- restaurar o estado visual anterior;
- exibir mensagem específica;
- atualizar disponibilidade;
- permitir escolher outro slot;
- não persistir atualização otimista incorreta.

### Drag and drop seguro

Implementar somente quando o backend possuir reagendamento estável.

Regras:

- drag and drop deve abrir confirmação;
- deve representar solicitação, não atualização definitiva;
- enviar `expectedVersion`;
- enviar `Idempotency-Key`;
- aplicar mudança somente após sucesso;
- restaurar o item em falha;
- tratar 409;
- bloquear consultas não reagendáveis;
- disponibilizar alternativa acessível por formulário.

Drag and drop nunca poderá ser a única forma de reagendar.

### Bloqueios, férias e indisponibilidade

Exibir:

- bloqueio parcial;
- bloqueio integral;
- férias;
- intervalos;
- unidade fechada;
- profissional indisponível.

Permitir criar ou editar somente quando endpoints e permissões existirem.

Não gerar disponibilidade no frontend.

### SignalR

Consumir eventos equivalentes a:

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

Requisitos:

- validar tenant;
- deduplicar por `EventId`;
- atualizar apenas período ou recurso afetado;
- evitar invalidação global;
- reconciliar após reconexão;
- indicar dados desatualizados quando offline.

### Performance

Aplicar:

- carregamento por intervalo visível;
- cache por período e filtros;
- paginação na lista;
- virtualização quando necessária;
- cancelamento de requests;
- lazy loading do detalhe;
- atualização incremental;
- evitar múltiplas chamadas por evento;
- resumo mensal separado de detalhes quando suportado.

### Responsividade

#### Desktop

- calendário completo;
- filtros visíveis;
- drawer de detalhes;
- agrupamento por recursos.

#### Tablet

- filtros recolhíveis;
- toolbar compacta;
- semana simplificada;
- drawer adaptado.

#### Mobile

- visão padrão em Lista ou Dia;
- filtros em drawer;
- formulário em tela cheia;
- ações em menu;
- nenhuma dependência de grid semanal horizontal;
- suporte a toque;
- ausência de overflow obrigatório.

### Acessibilidade

- navegação por teclado;
- foco visível;
- descrição textual dos eventos;
- botões para anterior, próximo e hoje;
- alternativa ao drag and drop;
- labels e ARIA;
- conflitos anunciados;
- não depender somente de cor.

### Escolha da biblioteca

Antes de adicionar biblioteca de calendário:

1. verificar dependências já existentes;
2. avaliar licença;
3. avaliar bundle;
4. avaliar compatibilidade com Next.js;
5. avaliar timezone;
6. avaliar acessibilidade;
7. avaliar drag and drop;
8. registrar a decisão.

Não adicionar biblioteca pesada sem justificativa.

### Testes unitários

Criar testes para:

- transformação dos DTOs;
- timezone;
- filtros;
- estados;
- intervalo visível;
- permissões;
- conflito;
- rollback visual;
- eventos realtime.

### Testes de integração

Criar testes para:

- carregar dia;
- carregar semana;
- carregar mês;
- carregar lista;
- filtrar;
- abrir detalhe;
- criar;
- confirmar;
- reagendar;
- conflito 409;
- cancelar;
- receber atualização realtime;
- estado offline.

### Playwright

Criar cenários:

```text
Abrir agenda
Alternar visualizações
Filtrar por profissional
Filtrar por unidade
Criar consulta
Abrir detalhe
Confirmar
Reagendar com sucesso
Reagendar com conflito
Cancelar
Receber evento SignalR
Validar mobile
Validar ausência de overflow
```

### Critérios de aceite

A agenda moderna estará concluída quando:

1. Dia, Semana, Mês e Lista funcionarem;
2. filtros forem server-side;
3. timezone estiver correto;
4. criação funcionar;
5. detalhe funcionar;
6. confirmação funcionar;
7. reagendamento funcionar;
8. conflito exibir feedback específico;
9. cancelamento funcionar;
10. SignalR atualizar o calendário;
11. mobile estiver utilizável;
12. alternativa ao drag and drop existir;
13. não houver atualização irreversível antes da resposta do backend;
14. testes unitários passarem;
15. testes de integração passarem;
16. Playwright passar;
17. documentação estiver atualizada.

---

## 14. Diagnóstico do envio de teste WhatsApp

Sintoma atual:

```text
Não foi possível concluir a operação. Tente novamente.
```

Fluxo esperado:

```text
Frontend
  ↓
API administrativa
  ↓
Validação
  ↓
ConversationMessage Pending
  ↓
OutboxMessage
  ↓
Commit
  ↓
Worker
  ↓
GatewayResolver
  ↓
Fake ou Twilio
  ↓
Status
```

Verificar obrigatoriamente:

- endpoint e método;
- autenticação e permission;
- tenant e integrationId;
- provider efetivo;
- configuração e sender;
- formato do destinatário;
- prefixo `whatsapp:`;
- normalização de telefone brasileiro;
- allowlist;
- limites diário e mensal;
- `Idempotency-Key`;
- DTO e validação;
- Outbox;
- Worker;
- RabbitMQ;
- gateway;
- callbacks;
- logs;
- `traceId`.

---

## 15. Endpoint de mensagem de teste

Criar ou evoluir:

```text
POST /api/v1/admin/integrations/whatsapp/{integrationId}/test-message
```

Header:

```text
Idempotency-Key
```

Request:

```json
{
  "recipient": "+5581999999999",
  "message": "Mensagem de teste do Clinic Assistant"
}
```

O provider deve ser resolvido pela configuração efetiva, não escolhido livremente pelo navegador.

Resposta inicial:

```json
{
  "messageId": "guid",
  "status": "Pending",
  "provider": "Fake",
  "traceId": "..."
}
```

Não aguardar o envio externo no request.

---

## 16. Erros padronizados

Criar códigos equivalentes a:

```text
whatsapp_provider_disabled
whatsapp_configuration_invalid
whatsapp_credentials_missing
whatsapp_sender_missing
whatsapp_recipient_invalid
whatsapp_recipient_not_allowed
whatsapp_daily_limit_exceeded
whatsapp_monthly_limit_exceeded
whatsapp_integration_not_found
whatsapp_worker_unavailable
whatsapp_outbox_failure
whatsapp_provider_failure
whatsapp_signature_invalid
```

Retornar `ProblemDetails`.

Exemplo:

```json
{
  "title": "Destinatário não autorizado",
  "status": 422,
  "code": "whatsapp_recipient_not_allowed",
  "detail": "O número informado não está autorizado para o Sandbox.",
  "traceId": "..."
}
```

---

## 17. Tratamento de erros no frontend

Mapear:

| Código | Mensagem |
|---|---|
| `whatsapp_provider_disabled` | O envio real está desabilitado neste ambiente. |
| `whatsapp_configuration_invalid` | Revise a configuração da integração WhatsApp. |
| `whatsapp_credentials_missing` | Configure as credenciais antes de testar. |
| `whatsapp_recipient_invalid` | Informe um telefone válido com DDI e DDD. |
| `whatsapp_recipient_not_allowed` | O número não está autorizado para testes no Sandbox. |
| `whatsapp_daily_limit_exceeded` | O limite diário de mensagens foi atingido. |
| `whatsapp_monthly_limit_exceeded` | O limite mensal de mensagens foi atingido. |
| `whatsapp_worker_unavailable` | A mensagem foi registrada, mas o serviço de envio está indisponível. |
| `whatsapp_provider_failure` | O provider recusou a mensagem. Consulte o diagnóstico. |

Exibir `traceId` copiável.

Não exibir stack trace.

A mensagem genérica deve permanecer apenas como fallback para códigos desconhecidos.

---

## 18. Status da mensagem

Permitir acompanhar:

```text
Pending
Queued
Sent
Delivered
Read
Failed
```

Atualizar por SignalR e usar polling como fallback.

Exibir timeline do processamento.

---

## 19. Observabilidade

Adicionar logs estruturados:

```text
TestMessageRequested
TestMessageValidated
OutboxCreated
WorkerConsumed
GatewayResolved
ProviderCalled
ProviderAccepted
StatusCallbackReceived
MessageFailed
```

Campos:

- TenantId;
- IntegrationId;
- MessageId;
- Provider;
- Environment;
- CorrelationId;
- TraceId;
- Result;
- FailureCode.

Não registrar telefone completo, mensagem integral ou credenciais.

Métricas:

```text
whatsapp_test_messages_requested_total
whatsapp_test_messages_queued_total
whatsapp_test_messages_sent_total
whatsapp_test_messages_failed_total
whatsapp_test_message_duration
local_environment_start_failures_total
ngrok_tunnel_status
```

---

## 20. Smoke tests

### Fake

1. iniciar ambiente local;
2. confirmar gateway Fake;
3. criar mensagem de teste;
4. confirmar Outbox;
5. confirmar consumo pelo Worker;
6. confirmar sucesso fake;
7. confirmar atualização de status;
8. confirmar resultado no frontend.

### Twilio Sandbox

Execução exclusivamente manual:

1. iniciar `twilio-smoke`;
2. confirmar ngrok;
3. configurar URLs no Console Twilio;
4. confirmar allowlist;
5. validar integração;
6. criar mensagem;
7. confirmar Outbox;
8. confirmar Worker;
9. confirmar `MessageSid`;
10. confirmar callback;
11. confirmar status.

---

## 21. Playwright

Criar cenários:

### Clínicas

- desktop;
- mobile;
- filtros;
- detalhe;
- formulário;
- ausência de overflow.

### Auditoria

- filtros;
- paginação;
- drawer;
- correlation ID;
- empty state;
- mobile.

### WhatsApp Fake

- abrir integração;
- enviar teste;
- visualizar `Pending`;
- visualizar `Sent`;
- visualizar sucesso.

### Erros

- provider desabilitado;
- telefone inválido;
- fora da allowlist;
- limite atingido;
- Worker indisponível;
- erro desconhecido sanitizado.

---

## 22. CI

Configuração obrigatória:

```text
Provider = Fake
Twilio = Disabled
ngrok = Disabled
AllowRealMessages = false
```

O CI deve:

- aplicar migrations;
- executar seed E2E;
- subir API, Worker, frontend, PostgreSQL, Redis e RabbitMQ;
- executar Playwright;
- coletar traces e screenshots em falha;
- nunca iniciar ngrok;
- nunca chamar Twilio.

---

## 23. Documentação

Criar ou atualizar:

```text
docs/getting-started/quick-start.md
docs/getting-started/local-environment.md
docs/getting-started/local-management.md
docs/testing/local-smoke-tests.md
docs/testing/twilio-smoke.md
docs/integrations/ngrok.md
docs/integrations/whatsapp-test-message.md
docs/frontend/clinics.md
docs/frontend/audit.md
docs/troubleshooting/whatsapp-test-message.md
```

O quick start deve conter apenas os comandos essenciais.

---

## 24. Critérios de aceite

A etapa estará concluída quando:

1. profiles `local`, `e2e` e `twilio-smoke` funcionarem;
2. Fake for o padrão;
3. ngrok for opcional;
4. scripts de start, stop, status, logs e validação funcionarem;
5. health checks funcionarem;
6. seed E2E funcionar;
7. tela de clínicas estiver corrigida e responsiva;
8. tela de auditoria estiver corrigida e responsiva;
9. agenda moderna estiver funcional e responsiva;
10. envio de teste utilizar Outbox;
10. Worker processar o teste;
11. causa raiz do erro estiver identificada;
12. frontend exibir erros específicos;
13. `ProblemDetails` incluir `code` e `traceId`;
14. provider desabilitado for tratado;
15. configuração inválida for tratada;
16. destinatário inválido for tratado;
17. allowlist e limites forem tratados;
18. status da mensagem for exibido;
19. smoke Fake passar;
20. Playwright passar;
21. CI não chamar Twilio;
22. documentação estiver atualizada;
23. nenhum secret estiver exposto.

---

## 25. Ordem de implementação

```text
9.5.1 Análise
9.5.2 Ambiente local
9.5.3 ngrok
9.5.4 Correções visuais
9.5.5 Calendário operacional moderno
9.5.6 Diagnóstico e correção do envio
9.5.7 Observabilidade
9.5.8 Testes
9.5.9 Documentação
```

---

## 26. Primeira entrega

Implemente inicialmente:

```text
9.5.1 Auditoria
9.5.2 Ambiente local
9.5.3 Integração ngrok
9.5.4 Correções visuais
9.5.5 Calendário operacional moderno
9.5.6 Diagnóstico do envio de teste
```

Entregar:

1. relatório do ambiente;
2. matrizes solicitadas;
3. profiles Compose;
4. scripts locais;
5. health checks;
6. integração ngrok;
7. tela de clínicas corrigida;
8. tela de auditoria corrigida;
9. agenda moderna implementada;
10. causa raiz do erro;
11. tratamento frontend corrigido;
12. testes unitários;
13. testes de integração;
14. documentação inicial.

Não executar smoke real até que Fake, Outbox, Worker, tratamento de erro e ngrok estejam validados.

---

## 27. Validação

Backend:

```bash
dotnet restore
dotnet build
dotnet test
```

Frontend:

```bash
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```

Ambiente:

```bash
./scripts/local/start-local.sh
./scripts/local/status.sh
./scripts/local/validate.sh
```

E2E:

```bash
./scripts/local/start-e2e.sh
npm run test:e2e
```

Não enviar mensagem real automaticamente.

---

## 28. Relatório final

Apresentar:

1. serviços e profiles;
2. scripts criados;
3. health checks;
4. correções da tela de clínicas;
5. correções da auditoria;
6. causa raiz do erro WhatsApp;
7. endpoint e fluxo corrigidos;
8. códigos de erro;
9. mensagens do frontend;
10. testes executados;
11. resultados;
12. documentação;
13. riscos restantes;
14. roteiro do smoke Twilio.

Não avançar automaticamente para funcionalidades fora desta etapa.
