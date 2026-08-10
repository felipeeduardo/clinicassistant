# Etapa 9.5 — relatório de status

## Status da etapa

Os fluxos funcionais desta etapa foram validados manualmente, incluindo ambiente local, banco `clinicassistant`, integração Fake, Twilio Sandbox, ngrok, webhook inbound, Outbox, Worker, resposta automática e menu numerado. A suíte E2E/Playwright fica temporariamente fora do escopo de evolução e não compõe as pendências funcionais desta etapa.

### Funcionalidades concluídas

- perfis local, E2E e `twilio-smoke` documentados;
- seleção explícita entre `Fake` e `Twilio` no seed `minimal`;
- separação entre `clinicassistant` e `clinicassistant_test`;
- scripts locais de start, stop, diagnóstico e smoke;
- health checks de API, frontend, PostgreSQL, RabbitMQ, Redis e ngrok;
- configuração automática das URLs públicas para validação da assinatura Twilio;
- integração do Worker com as configurações Twilio;
- webhook inbound e persistência transacional Inbox/Outbox;
- processamento assíncrono pelo Worker/RabbitMQ;
- resposta automática e menu numerado no WhatsApp;
- telas operacionais de clínicas, auditoria e agenda;
- documentação do smoke Twilio Sandbox + ngrok.

### Pendências funcionais

1. confirmar as métricas operacionais do fluxo WhatsApp em um coletor OTLP local;
2. executar os testes unitários .NET em um ambiente que permita o socket do runner.

### Fechamento operacional

- A instrumentação de métricas está ativa na API e no Worker, mas o Compose local ainda não inclui um coletor OTLP. Portanto, os contadores são emitidos sem um backend local para consulta; a validação de valores depende da configuração de `OTEL_EXPORTER_OTLP_ENDPOINT`.
- A revisão de segredos foi concluída: `.env` está listado no `.gitignore` e no `.dockerignore`; somente `.env.example`, sem credenciais reais, permanece versionado.
- As variáveis Twilio são injetadas separadamente na API e no Worker. O Sandbox usa `whatsapp:+14155238886`; valores de produção permanecem como overrides documentados.

Build backend, lint, typecheck, testes Vitest e build de produção do frontend foram validados com sucesso. O `dotnet test` compilou os projetos, mas o runner foi abortado pela restrição local de abertura de socket (`SocketException: Permission denied`).

Os testes E2E não fazem parte desta lista neste momento. Sua retomada será uma atividade futura, quando houver evolução funcional do frontend.

## Matriz de serviços

| Serviço | Porta | Health check | Dependências | Profile |
|---|---:|---|---|---|
| API | 8080 | `/health/ready` | PostgreSQL, RabbitMQ, Redis | padrão |
| Frontend | 3000 | `/login` | API | padrão |
| Worker | — | logs/outbox | PostgreSQL, RabbitMQ, Redis | padrão |
| PostgreSQL | 5432 | `pg_isready` | — | padrão |
| RabbitMQ | 5672/15672 | diagnostics ping | — | padrão |
| Redis | 6379 | `redis-cli ping` | — | padrão |
| Test data seeder | — | migrations/validation | API, PostgreSQL | e2e/development |
| ngrok | 4040 | `/api/tunnels` | API | twilio-smoke |

## Matriz de agenda

| Funcionalidade | Frontend | Endpoint | Realtime | Lacuna |
|---|---|---|---|---|
| Lista | tabela responsiva | `/api/appointments/search` | invalidação por evento | filtros avançados ainda incrementais |
| Detalhe | drawer | `/api/appointments/{id}` | atualização por evento | histórico resumido depende da API |
| Criação | formulário com disponibilidade | `/api/appointments`, `/availability` | `appointment.created` | calendário visual avançado (timeline/drag-and-drop) permanece fora do escopo |
| Confirmação/cancelamento | ações de linha | endpoints de operação | `appointment.updated` | — |
| Reagendamento | drawer com `expectedVersion` e idempotência | `/reschedule` | `appointment.updated` | drag and drop não habilitado |

## Matriz WhatsApp

| Etapa | Componente | Resultado |
|---|---|---|
| Solicitação | frontend + API | `202 Accepted` |
| Persistência | `ConversationMessage` + `OutboxMessage` | transacional |
| Entrega | Worker/RabbitMQ | Fake por padrão; Twilio manual |
| Diagnóstico | ProblemDetails | `code` e `traceId` |

## Métricas de mensagem de teste

O fluxo publica `whatsapp_test_messages_requested_total`, `whatsapp_test_messages_queued_total`, `whatsapp_test_messages_sent_total`, `whatsapp_test_messages_failed_total` e `whatsapp_test_message_duration`. As métricas não carregam telefone, conteúdo ou credenciais; o correlation ID continua disponível apenas para correlação técnica.

## Limitações

O arquivo de prompt canônico da etapa não estava materializado no workspace; esta implementação usou o conteúdo anexado à solicitação. A execução completa do Docker/E2E depende de acesso ao Docker Desktop. O smoke Twilio continua manual e não dispara mensagens automaticamente.

## Validação automatizada local

A validação automatizada E2E está temporariamente suspensa por decisão de escopo. Os fluxos correspondentes foram exercitados manualmente e não representam pendência funcional atual.
