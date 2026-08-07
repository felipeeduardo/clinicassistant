# Etapa 9.5 — relatório inicial

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

`E2E_SKIP=true npx playwright test` foi executado com sucesso: os 18 cenários foram ignorados de forma explícita e nenhum serviço externo foi chamado. A execução autenticada continua condicionada ao ambiente E2E completo, seed determinístico e Docker ativo.
