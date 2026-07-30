# Prompt mestre para o Codex — AI Business Assistant para Clínicas

Você atuará como arquiteto de software, engenheiro backend sênior e desenvolvedor full stack responsável por iniciar a construção de um SaaS chamado provisoriamente **Clinic AI Assistant**.

O produto será um assistente inteligente para clínicas e consultórios, integrado ao WhatsApp, capaz de realizar atendimento administrativo, responder dúvidas institucionais, consultar disponibilidade, agendar, remarcar e cancelar consultas.

O objetivo não é construir apenas um chatbot. O sistema deve funcionar como um assistente operacional, integrado à agenda da clínica e capaz de executar ações reais com segurança, rastreabilidade, resiliência e isolamento entre clientes.

---

# 1. Objetivo do produto

Construir uma plataforma SaaS multiempresa para clínicas e consultórios.

Cada clínica poderá:

* cadastrar sua empresa;
* cadastrar unidades;
* cadastrar profissionais;
* cadastrar especialidades;
* configurar horários de atendimento;
* conectar um número de WhatsApp;
* cadastrar informações institucionais;
* cadastrar perguntas frequentes;
* acompanhar conversas;
* visualizar agendamentos;
* permitir atendimento automatizado por IA;
* transferir conversas para atendentes humanos;
* acompanhar métricas operacionais.

O paciente poderá usar o WhatsApp para:

* solicitar informações;
* consultar especialidades;
* consultar profissionais;
* verificar horários disponíveis;
* agendar consulta;
* remarcar consulta;
* cancelar consulta;
* confirmar presença;
* receber lembretes;
* solicitar atendimento humano.

---

# 2. Limites funcionais do MVP

O MVP deverá focar exclusivamente em tarefas administrativas.

A IA não poderá:

* realizar diagnóstico;
* indicar medicamentos;
* interpretar exames;
* prescrever tratamentos;
* dar recomendações médicas;
* substituir atendimento profissional de saúde.

Quando o paciente fizer uma pergunta clínica ou sensível, o sistema deverá responder que não pode fornecer orientação médica e deverá encaminhar a conversa para um atendente humano.

---

# 3. Stack obrigatória

## Backend

Utilizar:

* .NET 10;
* C#;
* ASP.NET Core Web API;
* Entity Framework Core;
* PostgreSQL;
* RabbitMQ;
* Redis;
* OpenTelemetry;
* FluentValidation;
* Serilog ou logging estruturado nativo;
* Docker;
* Docker Compose;
* autenticação JWT;
* Swagger/OpenAPI;
* health checks;
* migrations do Entity Framework Core.

## Frontend

Utilizar:

* Next.js;
* TypeScript;
* App Router;
* Tailwind CSS;
* autenticação por JWT;
* painel responsivo;
* calendário de agendamentos;
* dashboard administrativo;
* área de conversas;
* área de configurações.

## Inteligência artificial

A integração com IA deverá inicialmente ser feita pelo backend .NET.

Não criar um microsserviço Python nesta primeira versão.

A arquitetura deve permitir futuramente adicionar um serviço Python para:

* processamento de áudio;
* análise de documentos;
* classificação especializada;
* modelos próprios;
* processamento de dados;
* avaliação de qualidade das respostas.

---

# 4. Arquitetura obrigatória

Construir inicialmente um **monólito modular**, evitando microsserviços prematuros.

Estrutura sugerida:

```text
src/
├── ClinicAssistant.Api
├── ClinicAssistant.Application
├── ClinicAssistant.Domain
├── ClinicAssistant.Infrastructure
├── ClinicAssistant.Worker
└── ClinicAssistant.Contracts
```

Testes:

```text
tests/
├── ClinicAssistant.UnitTests
├── ClinicAssistant.IntegrationTests
└── ClinicAssistant.ArchitectureTests
```

Frontend:

```text
frontend/
└── clinic-assistant-web
```

Infraestrutura:

```text
infra/
├── docker
├── scripts
└── observability
```

A solução deverá seguir princípios de:

* Clean Architecture;
* arquitetura hexagonal;
* Domain-Driven Design pragmático;
* SOLID;
* separação de responsabilidades;
* baixo acoplamento;
* alta coesão;
* testabilidade;
* observabilidade;
* segurança por padrão.

Não criar abstrações desnecessárias.

Evitar overengineering.

---

# 5. Módulos de domínio

A aplicação deverá ser organizada nos seguintes módulos:

```text
Modules/
├── Identity
├── Tenants
├── Clinics
├── Units
├── Professionals
├── Specialties
├── Patients
├── Scheduling
├── Conversations
├── WhatsApp
├── KnowledgeBase
├── Notifications
├── HumanHandoff
├── Billing
└── Audit
```

---

# 6. Multi-tenancy

O sistema será multiempresa.

Cada clínica será um tenant.

Toda entidade pertencente a uma clínica deverá possuir `TenantId`.

Exemplos:

* profissionais;
* pacientes;
* unidades;
* agendamentos;
* conversas;
* mensagens;
* configurações;
* documentos;
* FAQs;
* integrações.

É obrigatório garantir que uma clínica nunca consiga acessar dados de outra.

Implementar:

* resolução do tenant por usuário autenticado;
* filtro global no Entity Framework Core;
* validação do tenant na camada de aplicação;
* testes automatizados de isolamento;
* logs contendo `TenantId`;
* auditoria de acessos sensíveis.

Não confiar apenas no valor de `TenantId` enviado pelo frontend.

O tenant deve ser obtido pelo contexto autenticado.

---

# 7. Perfis de usuário

Criar inicialmente os seguintes perfis:

## PlatformAdmin

Administrador geral da plataforma.

Pode:

* gerenciar tenants;
* visualizar métricas globais;
* bloquear contas;
* consultar falhas operacionais;
* acompanhar integrações.

## ClinicAdmin

Administrador da clínica.

Pode:

* configurar a clínica;
* cadastrar unidades;
* cadastrar profissionais;
* gerenciar usuários;
* visualizar relatórios;
* configurar o assistente;
* conectar integrações.

## Receptionist

Atendente da clínica.

Pode:

* visualizar conversas;
* assumir atendimento;
* criar agendamentos;
* remarcar;
* cancelar;
* consultar pacientes.

## Professional

Profissional da clínica.

Pode:

* visualizar sua agenda;
* bloquear horários;
* consultar agendamentos próprios.

---

# 8. Entidades principais

Criar inicialmente as seguintes entidades.

## Tenant

Campos mínimos:

```text
Id
Name
Slug
Status
CreatedAt
UpdatedAt
```

## Clinic

```text
Id
TenantId
LegalName
TradeName
Document
Email
Phone
TimeZone
Status
CreatedAt
UpdatedAt
```

## ClinicUnit

```text
Id
TenantId
ClinicId
Name
Address
Phone
Status
CreatedAt
UpdatedAt
```

## User

```text
Id
TenantId
Name
Email
PasswordHash
Role
Status
LastLoginAt
CreatedAt
UpdatedAt
```

## Professional

```text
Id
TenantId
ClinicUnitId
Name
Email
Phone
RegistrationNumber
Status
CreatedAt
UpdatedAt
```

## Specialty

```text
Id
TenantId
Name
Description
Status
CreatedAt
UpdatedAt
```

## ProfessionalSpecialty

```text
ProfessionalId
SpecialtyId
```

## Patient

```text
Id
TenantId
Name
Phone
Email
BirthDate
ConsentStatus
CreatedAt
UpdatedAt
```

Evitar coletar dados desnecessários no MVP.

## AvailabilityRule

```text
Id
TenantId
ProfessionalId
DayOfWeek
StartTime
EndTime
SlotDurationMinutes
Active
CreatedAt
UpdatedAt
```

## ScheduleBlock

```text
Id
TenantId
ProfessionalId
StartsAt
EndsAt
Reason
CreatedAt
```

## Appointment

```text
Id
TenantId
ClinicUnitId
ProfessionalId
SpecialtyId
PatientId
StartsAt
EndsAt
Status
Source
Notes
CreatedAt
UpdatedAt
CancelledAt
CancellationReason
```

Status sugeridos:

```text
Pending
Confirmed
Cancelled
Completed
NoShow
Rescheduled
```

## Conversation

```text
Id
TenantId
PatientId
Channel
ExternalContactId
Status
AssignedUserId
StartedAt
ClosedAt
CreatedAt
UpdatedAt
```

Status:

```text
Bot
WaitingHuman
Human
Closed
```

## ConversationMessage

```text
Id
TenantId
ConversationId
Direction
Type
Content
ExternalMessageId
Status
SentAt
ReceivedAt
CreatedAt
```

## KnowledgeItem

```text
Id
TenantId
Title
Content
Category
Status
CreatedAt
UpdatedAt
```

## InboxMessage

```text
Id
TenantId
Provider
ExternalMessageId
Payload
Status
ReceivedAt
ProcessedAt
RetryCount
LastError
```

Criar índice único:

```text
Provider + ExternalMessageId
```

## OutboxMessage

```text
Id
TenantId
Type
Payload
Status
CreatedAt
ProcessedAt
RetryCount
LastError
```

## AuditLog

```text
Id
TenantId
UserId
Action
EntityName
EntityId
OldValues
NewValues
IpAddress
CreatedAt
```

---

# 9. Regras de agendamento

O sistema deve impedir agendamentos duplicados ou conflitantes.

A criação de um agendamento deverá:

1. validar tenant;
2. validar paciente;
3. validar profissional;
4. validar unidade;
5. validar especialidade;
6. validar regra de disponibilidade;
7. verificar bloqueios;
8. verificar conflitos;
9. executar a criação dentro de uma transação;
10. criar evento na outbox;
11. confirmar a transação;
12. enviar confirmação de forma assíncrona.

A IA nunca deverá inserir diretamente um agendamento no banco.

Ela deverá solicitar uma operação de domínio.

Exemplo de ferramenta:

```json
{
  "action": "schedule_appointment",
  "tenantId": "resolved-internally",
  "patientId": "patient-id",
  "professionalId": "professional-id",
  "specialtyId": "specialty-id",
  "slotId": "slot-id"
}
```

O `TenantId` não deve ser aceito diretamente da IA ou do cliente.

Deve ser resolvido internamente.

Antes de executar a ação, o backend deverá validar novamente todas as regras.

---

# 10. Webhook do WhatsApp

Criar um endpoint para receber webhooks do WhatsApp.

Fluxo obrigatório:

```text
WhatsApp
    ↓
Webhook ASP.NET Core
    ↓
Validação da assinatura
    ↓
Identificação da clínica
    ↓
Verificação de idempotência
    ↓
Persistência em InboxMessage
    ↓
Publicação na fila
    ↓
Resposta HTTP 200
```

O webhook deve responder rapidamente.

Não processar IA, agenda ou envio de mensagem dentro da mesma requisição HTTP.

Criar inicialmente os endpoints:

```text
GET  /api/webhooks/whatsapp
POST /api/webhooks/whatsapp
```

O GET será usado para validação do webhook.

O POST receberá mensagens e eventos.

Implementar:

* validação de assinatura;
* logs estruturados;
* idempotência;
* persistência do payload original;
* retorno rápido;
* tratamento seguro de erros.

---

# 11. Processamento assíncrono

Utilizar RabbitMQ para processamento de mensagens e eventos.

Criar inicialmente as seguintes filas:

```text
whatsapp.incoming
whatsapp.outgoing
appointments.notifications
appointments.reminders
human-handoff
dead-letter
```

Criar exchanges e routing keys coerentes.

Configurar:

* mensagens persistentes;
* acknowledgements manuais;
* retry controlado;
* dead-letter queue;
* correlation ID;
* causation ID;
* tenant ID;
* tracing distribuído;
* limite de tentativas.

Não implementar retry infinito.

Mensagens que falharem repetidamente deverão ir para dead-letter queue.

---

# 12. Transactional Outbox

Implementar o padrão Transactional Outbox.

Sempre que uma ação de domínio gerar um evento externo, salvar o estado e a mensagem da outbox na mesma transação.

Exemplo:

```text
BEGIN TRANSACTION

INSERT Appointment

INSERT OutboxMessage

COMMIT
```

Um worker deverá publicar eventos pendentes da outbox no RabbitMQ.

Após a publicação:

* marcar como processado;
* registrar horário;
* registrar tentativas;
* registrar erro em caso de falha.

Implementar também o padrão Inbox para mensagens recebidas.

---

# 13. Redis

Utilizar Redis para:

* cache de configurações do tenant;
* cache de perguntas frequentes;
* controle de rate limit;
* locks distribuídos quando necessário;
* estado temporário da conversa;
* sessões curtas;
* prevenção de processamento concorrente.

Redis não deve ser a fonte principal de verdade.

Os dados importantes devem permanecer no PostgreSQL.

---

# 14. Integração com IA

Criar uma abstração:

```csharp
public interface IAiAssistant
{
    Task<AiAssistantResult> ProcessAsync(
        AiAssistantRequest request,
        CancellationToken cancellationToken);
}
```

Criar uma implementação inicial desacoplada do provedor.

Não acoplar o domínio a OpenAI, Azure OpenAI ou qualquer fornecedor específico.

A IA deverá receber:

* contexto da clínica;
* informações institucionais;
* perguntas frequentes;
* histórico relevante da conversa;
* ferramentas permitidas;
* regras de segurança;
* estado atual da conversa.

A IA poderá solicitar ferramentas como:

```text
search_specialties
search_professionals
search_available_slots
schedule_appointment
reschedule_appointment
cancel_appointment
confirm_appointment
request_human_assistance
get_clinic_information
```

Cada ferramenta deverá ser implementada na camada de aplicação.

A IA não deverá acessar banco de dados diretamente.

A IA não deverá executar comandos livres.

A IA deverá retornar respostas estruturadas.

Exemplo:

```json
{
  "intent": "schedule_appointment",
  "confidence": 0.94,
  "response": "Encontrei três horários disponíveis.",
  "requiresHuman": false,
  "toolCalls": [
    {
      "name": "search_available_slots",
      "arguments": {
        "specialty": "Dermatologia",
        "preferredDate": "2026-08-01"
      }
    }
  ]
}
```

Criar limites para:

* quantidade de chamadas;
* tamanho do histórico;
* tempo total;
* tokens;
* ferramentas permitidas;
* tentativas;
* respostas inseguras.

---

# 15. Segurança da IA

O prompt do sistema deverá deixar explícito:

* a IA atua apenas em tarefas administrativas;
* não fornece diagnóstico;
* não prescreve;
* não interpreta exames;
* não inventa informações;
* não informa horários sem consultar o sistema;
* não confirma agendamento sem sucesso da operação;
* não revela dados de outros pacientes;
* não revela dados de outra clínica;
* deve encaminhar para humano em situações sensíveis;
* deve solicitar confirmação antes de cancelar;
* deve evitar armazenar informações médicas desnecessárias.

Implementar detecção de temas sensíveis.

Exemplos que exigem transferência:

* emergência;
* dor intensa;
* risco de vida;
* reação a medicamento;
* resultado de exame;
* diagnóstico;
* prescrição;
* reclamação grave;
* conflito;
* pedido explícito por humano.

---

# 16. Atendimento humano

Criar mecanismo de handoff.

A conversa deverá mudar de:

```text
Bot → WaitingHuman → Human → Closed
```

Quando uma conversa estiver com humano:

* a IA não deverá enviar mensagens automaticamente;
* o atendente poderá responder pelo painel;
* o histórico deverá ser preservado;
* o atendente poderá devolver a conversa para o bot;
* todas as transições deverão ser auditadas.

Criar endpoints:

```text
POST /api/conversations/{id}/request-human
POST /api/conversations/{id}/assign
POST /api/conversations/{id}/return-to-bot
POST /api/conversations/{id}/close
```

---

# 17. Autenticação e autorização

Implementar JWT com:

* access token;
* refresh token;
* expiração;
* rotação de refresh token;
* revogação;
* roles;
* policies;
* tenant resolution.

Endpoints:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

Não permitir autorregistro público de administradores de plataforma.

Criar seed opcional para desenvolvimento.

Armazenar senhas com algoritmo seguro.

Nunca salvar senha em texto puro.

---

# 18. Endpoints iniciais

Criar os seguintes grupos de endpoints.

## Clinics

```text
GET    /api/clinics/current
PUT    /api/clinics/current
```

## Units

```text
GET    /api/units
GET    /api/units/{id}
POST   /api/units
PUT    /api/units/{id}
DELETE /api/units/{id}
```

## Professionals

```text
GET    /api/professionals
GET    /api/professionals/{id}
POST   /api/professionals
PUT    /api/professionals/{id}
DELETE /api/professionals/{id}
```

## Specialties

```text
GET    /api/specialties
POST   /api/specialties
PUT    /api/specialties/{id}
DELETE /api/specialties/{id}
```

## Patients

```text
GET    /api/patients
GET    /api/patients/{id}
POST   /api/patients
PUT    /api/patients/{id}
```

## Availability

```text
GET    /api/professionals/{id}/availability
POST   /api/professionals/{id}/availability
PUT    /api/availability/{id}
DELETE /api/availability/{id}
```

## Schedule blocks

```text
GET    /api/professionals/{id}/blocks
POST   /api/professionals/{id}/blocks
DELETE /api/blocks/{id}
```

## Appointments

```text
GET    /api/appointments
GET    /api/appointments/{id}
GET    /api/appointments/availability
POST   /api/appointments
POST   /api/appointments/{id}/confirm
POST   /api/appointments/{id}/cancel
POST   /api/appointments/{id}/reschedule
```

## Conversations

```text
GET    /api/conversations
GET    /api/conversations/{id}
GET    /api/conversations/{id}/messages
POST   /api/conversations/{id}/messages
POST   /api/conversations/{id}/assign
POST   /api/conversations/{id}/close
POST   /api/conversations/{id}/return-to-bot
```

## Knowledge base

```text
GET    /api/knowledge
POST   /api/knowledge
PUT    /api/knowledge/{id}
DELETE /api/knowledge/{id}
```

---

# 19. Validação e tratamento de erros

Utilizar FluentValidation.

Padronizar erros com `ProblemDetails`.

Formato sugerido:

```json
{
  "type": "https://clinic-assistant/errors/validation",
  "title": "Validation error",
  "status": 400,
  "traceId": "trace-id",
  "errors": {
    "name": [
      "Name is required."
    ]
  }
}
```

Criar middleware global de exceções.

Não retornar stack trace em produção.

Criar exceções de domínio específicas:

```text
EntityNotFoundException
BusinessRuleException
ConflictException
UnauthorizedTenantAccessException
ExternalIntegrationException
AppointmentConflictException
```

---

# 20. Resiliência HTTP

Criar clientes HTTP tipados para:

* WhatsApp;
* provedor de IA;
* Google Calendar futuramente;
* gateway de pagamento futuramente.

Utilizar:

* timeout;
* circuit breaker;
* retry;
* limitação de concorrência;
* cancellation token;
* logging;
* tracing.

Não realizar retry automático indiscriminado em operações com efeito colateral.

Para envio de mensagens ou criação de recursos externos, utilizar:

* chave de idempotência;
* outbox;
* controle de status;
* reconciliação.

---

# 21. Observabilidade

Implementar OpenTelemetry.

Instrumentar:

* requisições HTTP;
* Entity Framework Core;
* RabbitMQ;
* Redis;
* clientes HTTP;
* workers;
* processamento de IA;
* criação de agendamentos;
* envio de mensagens.

Adicionar:

* trace ID;
* correlation ID;
* tenant ID;
* user ID;
* conversation ID;
* appointment ID;
* external message ID.

Criar métricas iniciais:

```text
http_requests_total
http_request_duration
whatsapp_messages_received_total
whatsapp_messages_sent_total
whatsapp_message_failures_total
ai_requests_total
ai_request_duration
ai_request_failures_total
appointments_created_total
appointments_cancelled_total
appointment_conflicts_total
human_handoffs_total
queue_messages_processed_total
queue_messages_failed_total
outbox_pending_total
```

Nunca registrar:

* senha;
* token;
* segredo;
* conteúdo médico sensível;
* payload integral com dados pessoais em logs de produção.

---

# 22. Health checks

Criar:

```text
/health/live
/health/ready
```

`live` deverá verificar apenas se a aplicação está em execução.

`ready` deverá verificar:

* PostgreSQL;
* RabbitMQ;
* Redis.

Integrações externas não devem necessariamente derrubar o readiness da aplicação.

---

# 23. Banco de dados

Usar PostgreSQL.

Criar migrations.

Usar:

* índices adequados;
* chaves estrangeiras;
* restrições;
* timestamps UTC;
* concorrência otimista quando necessário;
* transações explícitas em operações críticas.

Criar índices para:

```text
TenantId
Phone
Email
ProfessionalId + StartsAt
PatientId + StartsAt
ConversationId + CreatedAt
Provider + ExternalMessageId
Status + CreatedAt
```

Para impedir conflitos de agenda, adotar uma estratégia segura.

Pode ser:

* constraint com intervalo no PostgreSQL;
* lock transacional;
* isolamento serializável em operação crítica;
* advisory lock;
* combinação de validação e constraint.

Documentar a decisão.

---

# 24. Datas e fuso horário

Persistir datas em UTC.

Cada clínica deverá possuir um `TimeZone`.

Conversões para exibição devem considerar o fuso da clínica.

Não usar horário local do servidor como referência.

Não salvar datas sem timezone em operações de agenda.

---

# 25. Privacidade e LGPD

Aplicar princípios de minimização de dados.

Criar suporte inicial para:

* consentimento de comunicação;
* data de consentimento;
* origem do consentimento;
* revogação;
* anonimização futura;
* exclusão lógica;
* auditoria.

Não armazenar informações clínicas que não sejam necessárias para o agendamento.

Criar política de retenção configurável futuramente.

---

# 26. Frontend inicial

Criar as páginas:

```text
/login
/dashboard
/agenda
/agendamentos
/pacientes
/profissionais
/especialidades
/conversas
/conhecimento
/configuracoes
/usuarios
```

## Dashboard

Exibir:

* atendimentos hoje;
* agendamentos hoje;
* confirmações;
* cancelamentos;
* conversas com humano;
* tempo médio de atendimento;
* taxa de automação.

## Agenda

Criar visualização:

* diária;
* semanal;
* mensal.

Permitir:

* criar agendamento;
* editar;
* remarcar;
* cancelar;
* filtrar por unidade;
* filtrar por profissional;
* filtrar por especialidade.

## Conversas

Exibir:

* lista lateral;
* histórico;
* status;
* nome do paciente;
* telefone mascarado quando necessário;
* indicador bot/humano;
* campo para responder;
* botão assumir conversa;
* botão devolver ao bot;
* botão encerrar.

---

# 27. Design do frontend

Criar interface moderna, limpa e profissional.

Referências visuais:

* SaaS B2B;
* clínicas;
* confiança;
* simplicidade;
* boa legibilidade.

Evitar excesso de cores.

Utilizar:

* sidebar;
* topbar;
* cards;
* tabelas;
* modais;
* calendário;
* estados de loading;
* empty states;
* mensagens de erro;
* feedback de sucesso.

Criar componentes reutilizáveis.

---

# 28. Docker Compose

Criar `docker-compose.yml` com:

```text
api
worker
postgres
rabbitmq
redis
frontend
```

Adicionar volumes persistentes.

Adicionar health checks.

Criar `.env.example`.

Nunca versionar segredos.

Variáveis sugeridas:

```text
ASPNETCORE_ENVIRONMENT
CONNECTIONSTRINGS__DEFAULT
REDIS__CONNECTIONSTRING
RABBITMQ__HOST
RABBITMQ__PORT
RABBITMQ__USERNAME
RABBITMQ__PASSWORD
JWT__ISSUER
JWT__AUDIENCE
JWT__SECRET
JWT__ACCESS_TOKEN_MINUTES
JWT__REFRESH_TOKEN_DAYS
WHATSAPP__BASE_URL
WHATSAPP__ACCESS_TOKEN
WHATSAPP__VERIFY_TOKEN
WHATSAPP__APP_SECRET
AI__PROVIDER
AI__API_KEY
AI__MODEL
OTEL_EXPORTER_OTLP_ENDPOINT
```

---

# 29. Testes obrigatórios

Criar testes unitários para:

* regras de disponibilidade;
* conflito de horários;
* criação de agendamento;
* cancelamento;
* remarcação;
* isolamento de tenant;
* handoff;
* validações;
* idempotência.

Criar testes de integração para:

* autenticação;
* endpoints;
* PostgreSQL;
* inbox;
* outbox;
* webhook;
* RabbitMQ;
* criação concorrente de agendamentos.

Criar um teste específico simulando duas requisições tentando reservar o mesmo horário.

Somente uma deve ser bem-sucedida.

Criar testes de arquitetura para validar:

* Domain não depende de Infrastructure;
* Application não depende de Api;
* módulos não criam dependências circulares;
* contratos não dependem de implementações.

---

# 30. Qualidade de código

Aplicar:

* nullable reference types;
* analyzers;
* warnings tratados;
* nomes claros;
* métodos pequenos;
* records para DTOs quando adequado;
* CancellationToken;
* async/await;
* dependency injection;
* configuração tipada com Options Pattern;
* documentação mínima das decisões arquiteturais.

Evitar:

* service locator;
* classes genéricas chamadas `Helper`;
* repositório genérico excessivo;
* lógica de negócio em controller;
* lógica de negócio no Entity Framework;
* retorno direto de entidades;
* dependência direta do domínio em bibliotecas externas;
* métodos síncronos para I/O;
* captura genérica de exceção sem tratamento.

---

# 31. Documentação

Criar:

```text
README.md
docs/architecture.md
docs/domain.md
docs/messaging.md
docs/multi-tenancy.md
docs/ai-safety.md
docs/development.md
docs/deployment.md
docs/decisions/
```

Criar ADRs iniciais:

```text
ADR-001-monolith-modular.md
ADR-002-dotnet-10.md
ADR-003-postgresql.md
ADR-004-rabbitmq.md
ADR-005-transactional-outbox.md
ADR-006-multi-tenancy.md
ADR-007-ai-provider-abstraction.md
```

O README deve conter:

* visão do produto;
* stack;
* arquitetura;
* como executar;
* variáveis de ambiente;
* migrations;
* testes;
* endpoints;
* credenciais de desenvolvimento;
* decisões importantes.

---

# 32. Primeira entrega esperada

Na primeira etapa, não tente implementar o produto inteiro.

Entregue uma fundação executável.

A primeira entrega deve conter:

1. solução .NET criada;
2. projetos separados;
3. dependências configuradas;
4. Docker Compose;
5. PostgreSQL;
6. RabbitMQ;
7. Redis;
8. autenticação JWT;
9. multi-tenancy básico;
10. entidades iniciais;
11. migrations;
12. CRUD de clínicas;
13. CRUD de unidades;
14. CRUD de profissionais;
15. CRUD de especialidades;
16. regras básicas de disponibilidade;
17. criação de agendamento;
18. prevenção de conflito;
19. outbox;
20. inbox;
21. webhook inicial do WhatsApp;
22. worker RabbitMQ;
23. health checks;
24. OpenTelemetry;
25. Swagger;
26. testes iniciais;
27. README.

O frontend da primeira etapa poderá conter:

* login;
* layout autenticado;
* dashboard vazio;
* listagem de profissionais;
* listagem de agendamentos;
* calendário inicial.

---

# 33. Ordem de implementação

Siga esta ordem:

## Etapa 1 — Fundação

* criar solução;
* criar projetos;
* configurar dependências;
* configurar Docker;
* configurar banco;
* configurar logging;
* configurar health checks;
* configurar Swagger;
* configurar OpenTelemetry.

## Etapa 2 — Identidade e tenants

* criar Tenant;
* criar User;
* autenticação;
* JWT;
* refresh token;
* policies;
* tenant context;
* filtros globais;
* testes de isolamento.

## Etapa 3 — Cadastro da clínica

* Clinic;
* ClinicUnit;
* Professional;
* Specialty;
* endpoints;
* validações;
* testes.

## Etapa 4 — Agenda

* AvailabilityRule;
* ScheduleBlock;
* Patient;
* Appointment;
* disponibilidade;
* conflito;
* transações;
* testes concorrentes.

## Etapa 5 — Mensageria

* RabbitMQ;
* publisher;
* consumer;
* outbox;
* inbox;
* retry;
* dead-letter;
* idempotência.

## Etapa 6 — WhatsApp

* validação do webhook;
* recebimento;
* identificação da clínica;
* persistência;
* enfileiramento;
* resposta rápida;
* envio assíncrono.

## Etapa 7 — Conversas

* Conversation;
* ConversationMessage;
* handoff;
* atendimento humano;
* histórico.

## Etapa 8 — IA

* abstração;
* provedor inicial;
* ferramentas;
* regras de segurança;
* contexto;
* limites;
* fallback humano.

## Etapa 9 — Frontend

* autenticação;
* dashboard;
* agenda;
* profissionais;
* pacientes;
* conversas;
* configurações.

---

# 34. Forma de trabalho esperada

Antes de começar a codificação:

1. analise este documento;
2. apresente um resumo da arquitetura;
3. liste decisões que precisam ser tomadas;
4. apresente a estrutura de diretórios;
5. apresente a ordem de implementação;
6. identifique riscos;
7. identifique pontos que serão deixados preparados para evolução.

Depois, inicie a construção da Etapa 1.

Não implemente todas as etapas de uma única vez.

A cada etapa:

* mostre os arquivos criados;
* explique decisões relevantes;
* garanta que o projeto compile;
* execute testes;
* corrija falhas;
* atualize a documentação;
* não deixe código quebrado;
* não use pseudocódigo quando for solicitado código funcional.

---

# 35. Requisitos de aceite da fundação

A fundação será considerada pronta quando:

* `dotnet build` executar sem erros;
* `dotnet test` executar sem erros;
* `docker compose up` iniciar os serviços;
* a API responder;
* o Swagger abrir;
* os health checks funcionarem;
* a migration criar o banco;
* login retornar JWT;
* tenant for isolado;
* profissionais puderem ser cadastrados;
* especialidades puderem ser cadastradas;
* horários puderem ser configurados;
* agendamento puder ser criado;
* conflitos forem impedidos;
* webhook puder receber payload;
* mensagem puder ser armazenada no inbox;
* evento puder ser gravado no outbox;
* worker puder processar mensagens;
* logs apresentarem correlation ID e tenant ID.

---

# 36. Primeira instrução de execução

Comece agora pela fundação.

Entregue inicialmente:

1. estrutura completa da solução;
2. comandos utilizados para criar os projetos;
3. arquivos `.csproj`;
4. referências entre projetos;
5. `Directory.Build.props`;
6. `Directory.Packages.props`;
7. `docker-compose.yml`;
8. `.env.example`;
9. configuração inicial do ASP.NET Core;
10. configuração do PostgreSQL;
11. configuração do RabbitMQ;
12. configuração do Redis;
13. configuração do OpenTelemetry;
14. configuração de health checks;
15. configuração do Swagger;
16. middleware global de exceções;
17. `ProblemDetails`;
18. projeto de testes;
19. README inicial.

Após criar os arquivos:

* execute o build;
* execute os testes;
* informe os erros encontrados;
* corrija os erros;
* apresente o resultado final da Etapa 1.

Não avance para autenticação ou agendamento antes de a fundação estar funcional.
