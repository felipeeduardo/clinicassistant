# Etapa 9 — Frontend Operacional E2E — Parte 2

## Contexto

A primeira parte da Etapa 9 estabeleceu a fundação do frontend operacional, incluindo autenticação, autorização, multi-tenancy, layout, dashboard inicial, Inbox de conversas, agenda, pacientes, profissionais, especialidades e integração com as APIs administrativas.

Esta segunda parte deverá transformar o frontend em uma ferramenta operacional completa, permitindo validar os principais fluxos ponta a ponta sem depender exclusivamente do Postman.

O frontend deverá refletir os estados reais do backend, utilizar exclusivamente endpoints existentes, tratar concorrência, atualizar dados em tempo real e permitir a execução de cenários E2E com FakeWhatsAppGateway e Twilio real em smoke tests controlados.

A Etapa 8 de Inteligência Artificial permanece adiada.

---

# 1. Dependências obrigatórias

Antes de iniciar esta etapa, validar o estado das seguintes entregas:

```text
E2E-00 — Plataforma de Dados de Teste
E2E-TW-01 — Segurança e configuração
E2E-TW-02 — FakeWhatsAppGateway
E2E-TW-03 — Webhooks e assinatura
E2E-TW-04 — Envio real e callbacks
E2E-TW-05 — Recebimento e orquestração
E2E-TW-06 — Frontend operacional
E2E-TW-07 — CI e smoke real
```

Também deverão estar disponíveis:

- APIs administrativas;
- autenticação;
- autorização;
- multi-tenancy;
- paginação;
- Outbox;
- SignalR;
- dashboard agregado;
- fila humana;
- reagendamento;
- auditoria;
- operações WhatsApp;
- templates;
- plataforma de dados E2E;
- fixtures determinísticas.

Caso algum item esteja ausente:

- documentar a lacuna;
- bloquear apenas o fluxo dependente;
- não simular sucesso no frontend;
- não criar regra de negócio temporária na interface;
- registrar o endpoint ausente na documentação.

---

# 2. Objetivo

Ao final desta etapa, o frontend deverá permitir executar os fluxos administrativos e operacionais completos:

- login;
- seleção de tenant;
- dashboard;
- criação e atualização de tenants;
- criação e atualização de clínicas;
- criação e atualização de unidades;
- gestão de usuários;
- gestão de pacientes;
- gestão de profissionais;
- gestão de especialidades;
- disponibilidade;
- agenda;
- agendamento;
- reagendamento;
- cancelamento;
- conversas;
- fila humana;
- atendimento manual;
- pausa e retomada da automação;
- transferência;
- encerramento;
- reabertura;
- integração WhatsApp;
- templates;
- auditoria;
- atualização em tempo real;
- testes E2E com Playwright.

O Postman continuará como ferramenta de validação técnica, mas o frontend deverá permitir operar o produto.

---

# 3. Princípios obrigatórios

- o frontend não deve conter regras de negócio;
- o frontend não deve chamar Twilio;
- o frontend não deve publicar diretamente no RabbitMQ;
- o frontend não deve acessar o banco;
- toda mutation deve chamar endpoint administrativo;
- toda operação crítica deve tratar loading, erro, sucesso e conflito;
- toda operação mutável deve considerar `expectedVersion`;
- operações idempotentes devem usar `Idempotency-Key`;
- SignalR deve atualizar cache, não substituir queries;
- o backend continua sendo a fonte de verdade;
- o tenant deve vir da sessão ou contexto autorizado;
- nenhuma tela deve exibir dados de outro tenant;
- nenhuma tela deve exibir secrets;
- nenhuma ação destrutiva deve ocorrer sem confirmação;
- nenhuma resposta de erro deve exibir stack trace;
- conteúdo de mensagens deve ser tratado como entrada não confiável.

---

# 4. Stack

Manter a stack já adotada no projeto.

Preferencialmente:

```text
Next.js
TypeScript
App Router
Tailwind CSS
TanStack Query
React Hook Form
Zod
SignalR
Vitest
Testing Library
Playwright
```

Não substituir bibliotecas existentes sem necessidade.

---

# 5. Organização por features

Estrutura recomendada:

```text
src/
├── app/
├── components/
├── features/
│   ├── dashboard/
│   ├── tenants/
│   ├── clinics/
│   ├── units/
│   ├── users/
│   ├── patients/
│   ├── professionals/
│   ├── specialties/
│   ├── scheduling/
│   ├── appointments/
│   ├── conversations/
│   ├── human-queue/
│   ├── whatsapp/
│   ├── templates/
│   ├── audit/
│   └── settings/
├── providers/
├── services/
├── hooks/
├── schemas/
├── types/
└── lib/
```

Cada feature deverá possuir, quando aplicável:

```text
components
hooks
services
schemas
types
queries
mutations
tests
```

---

# 6. Dashboard operacional

Criar ou evoluir:

```text
/dashboard
```

Exibir:

- conversas abertas;
- aguardando paciente;
- aguardando humano;
- em atendimento humano;
- conversas encerradas;
- mensagens recebidas;
- mensagens enviadas;
- entregues;
- lidas;
- falhas;
- tamanho da fila;
- tempo médio de espera;
- maior tempo de espera;
- SLA excedido;
- consultas do dia;
- consultas confirmadas;
- reagendadas;
- canceladas;
- status WhatsApp;
- último webhook;
- último envio;
- falhas recentes.

Filtros:

```text
Hoje
7 dias
30 dias
Período personalizado
Unidade
Fila
Atendente
```

Atualizar por SignalR via evento:

```text
dashboard.invalidated
```

Ao receber o evento:

- invalidar somente queries necessárias;
- não refazer todas as chamadas;
- aplicar debounce;
- impedir tempestade de requests.

---

# 7. Gestão de tenants

Rotas:

```text
/tenants
/tenants/new
/tenants/{tenantId}
/tenants/{tenantId}/edit
```

Operações:

- listar;
- buscar;
- filtrar;
- criar;
- editar;
- ativar;
- suspender;
- desativar;
- consultar detalhes;
- consultar clínicas;
- consultar usuários;
- consultar integrações;
- consultar limites.

Somente PlatformAdmin.

---

# 8. Wizard de onboarding

Criar wizard:

```text
/tenants/new
```

Etapas:

## Empresa

- nome;
- slug;
- timezone;
- locale;
- plano;
- limites.

## Clínica

- razão social;
- nome fantasia;
- documento;
- email;
- telefone.

## Unidade inicial

- nome;
- endereço;
- telefone;
- horário.

## Administrador

- nome;
- email;
- perfil;
- convite ou senha temporária.

## Integração

- provider;
- sender placeholder;
- ativação posterior;
- status inicial.

## Revisão

- resumo;
- validação;
- confirmação.

Requisitos:

- React Hook Form;
- Zod;
- estado preservado entre etapas;
- validação por etapa;
- tratamento de rollback parcial;
- resultado detalhado;
- nunca criar entidades diretamente no frontend;
- preferir endpoint transacional ou workflow administrativo.

---

# 9. Clínicas

Rotas:

```text
/clinics
/clinics/{clinicId}
/clinics/{clinicId}/edit
```

Operações:

- listar;
- buscar;
- criar;
- editar;
- ativar;
- desativar;
- consultar unidades;
- consultar integração;
- consultar horários.

---

# 10. Unidades

Rotas:

```text
/units
/units/{unitId}
/units/{unitId}/edit
```

Operações:

- listar;
- criar;
- editar;
- ativar;
- desativar;
- configurar horário;
- vincular profissionais;
- consultar agenda.

Não implementar mapa externo obrigatório nesta etapa.

---

# 11. Usuários

Rotas:

```text
/users
/users/new
/users/{userId}
/users/{userId}/edit
```

Perfis:

```text
PlatformAdmin
ClinicAdmin
Manager
Receptionist
Operator
Viewer
```

Operações:

- listar;
- buscar;
- criar;
- editar;
- ativar;
- desativar;
- redefinir acesso;
- alterar perfil;
- consultar permissões;
- consultar auditoria.

O frontend apenas exibe permissões retornadas pelo backend.

---

# 12. Pacientes

Rotas:

```text
/patients
/patients/new
/patients/{patientId}
/patients/{patientId}/edit
```

Lista:

- nome;
- telefone mascarado;
- email mascarado;
- status;
- última interação;
- próxima consulta;
- conversas abertas;
- origem.

Detalhe:

- dados administrativos;
- consentimentos;
- consultas futuras;
- consultas anteriores resumidas;
- conversas;
- última interação;
- auditoria resumida.

Formulário:

- nome;
- telefone;
- email;
- data de nascimento;
- consentimento;
- origem;
- observação administrativa não clínica;
- status.

Não incluir dados clínicos.

---

# 13. Profissionais

Rotas:

```text
/professionals
/professionals/new
/professionals/{professionalId}
/professionals/{professionalId}/edit
```

Operações:

- listar;
- buscar;
- criar;
- editar;
- ativar;
- desativar;
- vincular especialidades;
- vincular unidades;
- configurar duração;
- configurar disponibilidade;
- consultar agenda.

---

# 14. Especialidades

Rotas:

```text
/specialties
/specialties/new
/specialties/{specialtyId}/edit
```

Operações:

- listar;
- criar;
- editar;
- ativar;
- desativar;
- vincular profissionais;
- vincular unidades.

Impedir exclusões destrutivas quando houver dependências.

---

# 15. Disponibilidade

Criar interface para:

- dias da semana;
- horário inicial;
- horário final;
- duração do slot;
- intervalos;
- exceções;
- bloqueios;
- férias;
- indisponibilidade.

Requisitos:

- timezone da clínica;
- validação de sobreposição;
- feedback de conflito;
- atualização da agenda;
- não gerar slots no frontend como fonte de verdade.

---

# 16. Agenda

Rota:

```text
/scheduling
```

Visualizações:

```text
Dia
Semana
Mês
Lista
```

Exibir:

- profissional;
- paciente;
- unidade;
- especialidade;
- origem;
- status;
- confirmação;
- conflito;
- observação administrativa.

Filtros:

```text
Profissional
Especialidade
Unidade
Status
Período
Origem
```

Funcionalidades:

- abrir detalhe;
- criar consulta;
- reagendar;
- cancelar;
- confirmar;
- bloquear horário;
- atualizar após SignalR.

Drag and drop somente se:

- backend possuir endpoint apropriado;
- houver confirmação explícita;
- conflito for tratado;
- rollback visual for implementado.

---

# 17. Agendamento

Fluxo:

```text
Paciente
Especialidade
Profissional
Unidade
Data
Disponibilidade
Slot
Confirmação
Criação
```

Requisitos:

- revalidar slot;
- `Idempotency-Key`;
- tratar 409;
- preservar formulário;
- mostrar timezone;
- atualizar agenda;
- atualizar conversa quando originado por atendimento;
- exibir confirmação.

---

# 18. Reagendamento

Fluxo:

```text
Consulta atual
Novo filtro
Novo slot
Comparação
Confirmação
Reagendamento
```

Requisitos:

- `expectedVersion`;
- `Idempotency-Key`;
- exibir horário anterior e novo;
- revalidar slot;
- tratar conflito;
- preservar estado anterior;
- atualizar agenda;
- atualizar conversa;
- atualizar auditoria.

---

# 19. Cancelamento

Requisitos:

- mostrar política;
- motivo opcional;
- confirmação explícita;
- destacar ação destrutiva;
- tratar consulta já cancelada;
- tratar versão desatualizada;
- atualizar agenda;
- atualizar conversa;
- registrar resultado.

---

# 20. Inbox de conversas

Rota:

```text
/conversations
```

Layout desktop:

```text
Lista | Conversa | Detalhes
```

Lista:

- paciente;
- telefone mascarado;
- última mensagem;
- horário;
- status;
- automação;
- intenção;
- prioridade;
- responsável;
- fila;
- não lidas;
- falha.

Filtros:

```text
Status
Automação
Intenção
Prioridade
Responsável
Fila
Unidade
Período
Busca
```

Paginação server-side.

---

# 21. Tela de conversa

Exibir:

- cabeçalho;
- paciente;
- status;
- automação;
- intenção;
- etapa;
- prioridade;
- atendente;
- fila;
- histórico;
- composer;
- ações;
- auditoria resumida;
- consultas do paciente.

Ações:

```text
Assumir
Liberar
Transferir
Pausar
Retomar
Reiniciar fluxo
Voltar ao menu
Encerrar
Reabrir
Alterar prioridade
Enviar mensagem
```

---

# 22. Atendimento humano

Regras:

- ao assumir, tratar 409;
- mostrar responsável;
- pausar automação;
- permitir envio manual;
- impedir envio duplicado;
- utilizar Outbox;
- exibir status;
- permitir transferir;
- permitir liberar;
- permitir retomar automação;
- auditar.

---

# 23. Envio manual

Composer:

- texto;
- limite;
- Enter;
- Shift+Enter;
- loading;
- retry seguro;
- idempotency key;
- status;
- falha sanitizada.

Fluxo:

```text
Frontend
    ↓
API administrativa
    ↓
ConversationMessage Pending
    ↓
Outbox
    ↓
Worker
    ↓
IWhatsAppGateway
```

Nunca chamar Twilio diretamente.

---

# 24. Mídia

Somente implementar anexos quando os endpoints existirem.

Se não existirem:

- ocultar ou desabilitar;
- documentar dependência;
- não simular upload local.

---

# 25. Fila humana

Rota:

```text
/queue
```

Exibir:

- paciente;
- motivo;
- prioridade;
- fila;
- tempo de espera;
- status;
- responsável;
- última mensagem.

Ações:

- assumir;
- transferir;
- liberar;
- concluir;
- abrir conversa.

Atualização em tempo real.

---

# 26. WhatsApp

Rota:

```text
/integrations/whatsapp
```

Exibir:

- provider;
- status;
- sender mascarado;
- ambiente;
- último webhook;
- último envio;
- última falha;
- templates;
- capabilities;
- callbacks.

Ações:

- validar;
- enviar mensagem de teste;
- sincronizar templates;
- ativar;
- desativar.

Nunca exibir secrets.

---

# 27. Templates

Rota:

```text
/integrations/whatsapp/templates
```

Operações:

- listar;
- filtrar;
- visualizar;
- criar;
- editar;
- ativar;
- desativar;
- sincronizar;
- visualizar variáveis;
- preview.

Não alterar status do provider localmente sem resposta do backend.

---

# 28. Auditoria

Rota:

```text
/audit
```

Filtros:

- usuário;
- ação;
- recurso;
- resultado;
- data;
- correlation ID.

Exibir:

- timestamp;
- usuário;
- ação;
- recurso;
- resultado;
- detalhes sanitizados.

Não exibir payload integral.

---

# 29. Configurações

Rota:

```text
/settings
```

Seções:

- empresa;
- clínica;
- unidades;
- usuários;
- filas;
- horários;
- permissões;
- integrações;
- segurança;
- templates internos.

Somente usuários autorizados.

---

# 30. SignalR

Criar cliente central.

Eventos:

```text
conversation.created
conversation.updated
conversation.message.created
conversation.message.status.changed
conversation.assigned
conversation.released
conversation.transferred
conversation.automation.paused
conversation.automation.resumed
conversation.closed
conversation.reopened
conversation.priority.changed
queue.item.created
queue.item.updated
queue.item.completed
appointment.created
appointment.rescheduled
appointment.cancelled
patient.updated
whatsapp.integration.updated
whatsapp.template.updated
audit.created
dashboard.invalidated
```

Requisitos:

- conectar pelo tenant autorizado;
- reconectar com backoff;
- deduplicar por EventId;
- atualizar cache específico;
- sincronizar após reconexão;
- encerrar ao trocar tenant;
- indicador de conexão;
- logs sanitizados.

---

# 31. Modo demonstração

Criar somente se houver suporte explícito.

Rota ou toggle:

```text
Modo demonstração
```

Quando ativo:

- usar tenant de demonstração;
- exibir banner;
- impedir ações destrutivas reais;
- utilizar FakeWhatsAppGateway;
- utilizar dados seed;
- não ativar Twilio;
- não permitir alteração de credenciais.

O modo demo não pode ser ativado em produção sem configuração explícita.

---

# 32. E2E-00 — Plataforma de Dados de Teste

Validar:

- reset;
- seed;
- validate;
- manifesto;
- tenant principal;
- tenant isolado;
- usuários;
- fixtures;
- IDs fixos;
- integração fake;
- Twilio desabilitada.

O frontend E2E deve consumir o manifesto sempre que possível.

---

# 33. E2E-TW-01 — Segurança e configuração

Validar visualmente:

- integração desabilitada;
- configuração ausente;
- status sanitizado;
- sender mascarado;
- nenhum secret;
- erro de validação;
- permissão insuficiente.

---

# 34. E2E-TW-02 — FakeWhatsAppGateway

Criar cenários Playwright:

- inbound simulado;
- conversa criada;
- resposta fake;
- status enviado;
- status entregue;
- falha transitória;
- falha permanente;
- DLQ refletida quando exposta;
- nenhuma chamada externa.

---

# 35. E2E-TW-03 — Webhooks e assinatura

No frontend, validar:

- último webhook;
- status da integração;
- falha de assinatura sanitizada;
- timestamp;
- diagnóstico operacional permitido.

Não expor assinatura ou payload.

---

# 36. E2E-TW-04 — Envio real e callbacks

Smoke real controlado:

- enviar mensagem de teste;
- status Pending;
- Queued;
- Sent;
- Delivered;
- Read quando disponível;
- Failed;
- erro sanitizado.

Esse teste não deve executar em todo commit.

---

# 37. E2E-TW-05 — Recebimento e orquestração

Validar:

- nova mensagem;
- conversa na Inbox;
- histórico;
- intenção;
- etapa;
- resposta;
- duplicidade;
- handoff;
- mídia não suportada;
- expiração.

---

# 38. E2E-TW-06 — Frontend operacional

Cenários completos:

```text
Login
Troca de tenant
Dashboard
Criar paciente
Editar paciente
Criar especialidade
Criar profissional
Configurar disponibilidade
Criar consulta
Reagendar
Cancelar
Simular inbound
Abrir conversa
Assumir
Enviar mensagem
Transferir
Liberar
Retomar automação
Encerrar
Reabrir
Consultar auditoria
```

---

# 39. E2E-TW-07 — CI e smoke real

Pipelines:

```text
pull_request:
  unit
  integration
  frontend tests
  fake E2E

main:
  unit
  integration
  fake E2E

manual_twilio_smoke:
  smoke real controlado
```

Smoke real:

- secrets protegidos;
- allowlist;
- limite de mensagens;
- timeout;
- cleanup;
- artefatos sem PII;
- traces Playwright;
- logs correlacionados.

---

# 40. Playwright

Criar projetos:

```text
chromium
firefox opcional
mobile-smoke opcional
```

Fixtures:

- autenticação;
- tenant;
- manifesto E2E;
- reset;
- seed;
- API helpers;
- SignalR helpers quando viável.

Testes devem ser independentes.

---

# 41. Cenários Playwright obrigatórios

## Login

- sucesso;
- falha;
- acesso negado;
- logout.

## Multi-tenant

- troca;
- limpeza de cache;
- bloqueio cruzado.

## Paciente

- criar;
- editar;
- buscar;
- detalhes.

## Agenda

- criar;
- reagendar;
- conflito;
- cancelar.

## Conversa

- inbound;
- abrir;
- assumir;
- enviar;
- transferir;
- liberar;
- retomar;
- encerrar;
- reabrir.

## WhatsApp

- fake;
- falha;
- status;
- teste manual real separado.

## Auditoria

- ação registrada;
- filtro;
- correlation ID.

---

# 42. Performance

Aplicar:

- lazy loading;
- virtualização em listas grandes;
- paginação;
- cache;
- cancelamento;
- skeleton;
- evitar N requests por item;
- evitar invalidação global;
- evitar re-render desnecessário.

---

# 43. Responsividade

Prioridade:

```text
Desktop
Notebook
Tablet
Mobile básico
```

Não exigir operação avançada completa em mobile nesta etapa.

---

# 44. Acessibilidade

- navegação por teclado;
- foco;
- contraste;
- labels;
- ARIA;
- dialogs acessíveis;
- mensagens anunciadas;
- não depender somente de cor.

Meta: WCAG 2.2 AA nas telas principais.

---

# 45. Segurança

- JWT ou sessão conforme backend;
- refresh seguro;
- RBAC;
- timeout;
- logout;
- XSS;
- CSP quando possível;
- nenhum secret público;
- nenhuma PII em telemetry;
- mensagens como entrada não confiável;
- proteção contra open redirect;
- desabilitar devtools em produção.

---

# 46. Observabilidade frontend

Registrar:

- navegação;
- erro;
- mutation;
- conflito;
- falha SignalR;
- reconexão;
- latência;
- erro E2E;
- correlation ID quando retornado.

Não registrar conteúdo integral de mensagens.

---

# 47. Documentação

Criar ou atualizar:

```text
docs/frontend/operational-e2e.md
docs/frontend/admin-modules.md
docs/frontend/realtime.md
docs/frontend/e2e-playwright.md
docs/frontend/demo-mode.md
docs/frontend/security.md
docs/frontend/troubleshooting.md
```

Atualizar a documentação Postman e E2E.

---

# 48. Critérios de aceite

A etapa estará concluída quando:

```text
1. Dashboard operacional funcionar
2. Tenant wizard funcionar
3. Clínicas funcionarem
4. Unidades funcionarem
5. Usuários funcionarem
6. Pacientes funcionarem
7. Profissionais funcionarem
8. Especialidades funcionarem
9. Disponibilidade funcionar
10. Agenda funcionar
11. Agendamento funcionar
12. Reagendamento funcionar
13. Cancelamento funcionar
14. Inbox funcionar
15. Histórico funcionar
16. Fila humana funcionar
17. Assumir funcionar
18. Transferir funcionar
19. Liberar funcionar
20. Pausar funcionar
21. Retomar funcionar
22. Encerrar funcionar
23. Reabrir funcionar
24. Envio manual utilizar Outbox
25. SignalR funcionar
26. WhatsApp administrativo funcionar
27. Templates funcionarem
28. Auditoria funcionar
29. Multi-tenancy estiver protegido
30. Concorrência retornar 409 corretamente
31. Fake E2E passar
32. Playwright passar
33. CI passar
34. Smoke real estar documentado
35. Nenhum secret estiver exposto
36. Documentação estiver atualizada
```

---

# 49. Ordem de implementação

## 9.2.1 Auditoria de dependências

- endpoints;
- permissões;
- contratos;
- SignalR;
- seeds;
- fixtures.

## 9.2.2 Administração

- tenants;
- clínicas;
- unidades;
- usuários.

## 9.2.3 Cadastros

- pacientes;
- profissionais;
- especialidades;
- disponibilidade.

## 9.2.4 Agenda

- calendário;
- criação;
- reagendamento;
- cancelamento.

## 9.2.5 Conversas

- Inbox;
- detalhe;
- fila;
- atendimento humano;
- envio.

## 9.2.6 Integrações

- WhatsApp;
- templates;
- auditoria.

## 9.2.7 Realtime

- SignalR;
- cache;
- reconexão.

## 9.2.8 E2E

- Fake;
- Playwright;
- CI;
- smoke real.

## 9.2.9 Qualidade

- segurança;
- acessibilidade;
- performance;
- documentação.

---

# 50. Prompt inicial para o Codex

Antes de alterar o código:

1. analise o frontend atual;
2. analise os endpoints administrativos;
3. analise o OpenAPI;
4. analise o Postman;
5. analise os eventos SignalR;
6. analise os seeds E2E;
7. analise o manifesto de fixtures;
8. identifique telas já prontas;
9. identifique telas incompletas;
10. identifique endpoints ausentes;
11. identifique permissões;
12. identifique conflitos entre frontend e backend;
13. liste arquivos a criar;
14. liste arquivos a alterar;
15. apresente o plano incremental;
16. não altere o código antes de concluir a análise.

Implemente inicialmente somente:

```text
9.2.1 — Auditoria de dependências
9.2.2 — Administração básica
9.2.3 — Cadastros principais
```

Primeira entrega:

```text
1. Relatório de dependências
2. Matriz tela x endpoint
3. Matriz permissão x ação
4. Tenant list
5. Tenant details
6. Tenant onboarding wizard
7. Clinics list
8. Units list
9. Users list
10. Patients list
11. Patient create
12. Patient edit
13. Professionals list
14. Specialties list
15. Form schemas
16. API services
17. Query hooks
18. Mutation hooks
19. Loading states
20. Error states
21. Conflict handling
22. Unit tests
23. Integration tests
24. Documentation
```

Não avançar para:

- agenda completa;
- conversas completas;
- SignalR;
- WhatsApp;
- Playwright completo;
- smoke real;

enquanto:

- a aplicação não compilar;
- autenticação não estiver estável;
- permissões não estiverem validadas;
- multi-tenancy não estiver testado;
- APIs administrativas básicas não estiverem integradas;
- testes da primeira entrega não passarem.

Após implementar, executar o gerenciador já adotado:

```text
install
lint
typecheck
test
build
```

Corrigir todos os erros.

Ao finalizar, apresentar:

1. arquivos criados;
2. arquivos alterados;
3. telas implementadas;
4. endpoints consumidos;
5. endpoints ausentes;
6. permissões aplicadas;
7. testes executados;
8. riscos restantes;
9. próximos passos.
