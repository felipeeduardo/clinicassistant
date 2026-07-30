## Etapa 8 — APIs Administrativas, Operações e Tempo Real

Implementar os contratos administrativos necessários para suportar o frontend operacional da Etapa 9, conforme a especificação detalhada disponível em:

`docs/prompts/etapa-08-apis-administrativas-operacoes-realtime.md`

Esta etapa deverá preencher as lacunas existentes entre o backend operacional das Etapas 6 e 7 e o frontend.

Implementar:

### Conversas

* lista paginada de conversas;
* filtros por status, automação, intenção, prioridade, fila, responsável, paciente, unidade e período;
* busca por nome e telefone;
* ordenação;
* detalhe administrativo da conversa;
* estado resumido;
* ações permitidas;
* concorrência otimista;
* isolamento multi-tenant.

### Histórico de mensagens

* histórico paginado;
* carregamento de mensagens anteriores;
* mensagens inbound e outbound;
* mensagens humanas e automatizadas;
* templates;
* mídias;
* status de envio;
* falhas sanitizadas;
* marcação de leitura;
* contador de mensagens não lidas.

### Fila humana

* lista paginada da fila;
* filtros;
* prioridade;
* tempo de espera;
* motivo do handoff;
* responsável;
* assumir conversa;
* liberar conversa;
* transferir conversa entre usuários e filas;
* tratamento de atribuição concorrente;
* atualização do item da fila;
* auditoria.

### Operações da conversa

* pausar automação;
* retomar automação;
* retomar estado anterior;
* reiniciar fluxo;
* retornar ao menu;
* encerrar conversa;
* reabrir conversa;
* alterar prioridade;
* registrar motivo;
* aplicar concorrência otimista;
* publicar eventos após commit.

### Envio manual via Outbox

* endpoint administrativo de envio;
* criação de `ConversationMessage` com status `Pending`;
* criação de `OutboxMessage`;
* persistência na mesma transação;
* envio posterior pelo Worker;
* uso de `IWhatsAppGateway`;
* idempotency key;
* prevenção de envio duplicado;
* atualização de status;
* evento SignalR;
* nenhuma chamada direta ao Twilio pelo endpoint.

### Dashboard operacional

* conversas abertas;
* aguardando paciente;
* aguardando humano;
* em atendimento;
* fechadas;
* expiradas;
* mensagens recebidas;
* mensagens enviadas;
* mensagens entregues;
* mensagens lidas;
* falhas de envio;
* tamanho da fila;
* tempo médio de espera;
* maior tempo de espera;
* SLA excedido;
* consultas criadas;
* consultas confirmadas;
* consultas reagendadas;
* consultas canceladas;
* status da integração WhatsApp;
* filtros por período, unidade, fila e atendente.

### Reagendamento administrativo

* endpoint de reagendamento;
* revalidação do slot;
* validação de profissional, unidade, paciente e consulta;
* idempotência;
* concorrência;
* operação transacional;
* preservação da consulta original em falha;
* retorno HTTP 409 quando o slot estiver ocupado;
* evento de domínio;
* confirmação via Outbox quando configurada;
* auditoria do horário anterior e novo.

### Pacientes

* lista paginada;
* busca por nome, telefone, email e identificador administrativo;
* telefone e email mascarados;
* filtros;
* detalhes administrativos;
* consultas futuras;
* consultas anteriores resumidas;
* conversas;
* última interação;
* consentimentos;
* isolamento multi-tenant;
* nenhuma exposição de prontuário, diagnóstico ou prescrição.

### Administração WhatsApp

* consulta da integração;
* provider;
* sender mascarado;
* status;
* último webhook;
* último envio;
* última falha sanitizada;
* validação da integração;
* mensagem de teste;
* ativação;
* desativação;
* sincronização de templates;
* nenhuma exposição de credenciais ou secrets.

### Templates

* listagem paginada;
* filtros;
* detalhes;
* criação;
* edição;
* ativação;
* desativação;
* sincronização com o provider;
* validação de variáveis;
* suporte a `ContentSid`;
* versionamento;
* auditoria;
* nenhuma dependência do SDK do provider nos controllers.

### Auditoria

* consulta paginada;
* filtros por usuário, ação, recurso, resultado, período e correlation ID;
* detalhes sanitizados;
* ações administrativas;
* mudanças de estado;
* atribuições;
* mensagens manuais;
* reagendamentos;
* operações de integração;
* nenhuma exposição de payload integral, conteúdo completo de mensagens ou secrets.

### SignalR

Implementar um hub administrativo em:

```text
/admin/realtime
```

Utilizar grupos:

```text
tenant:{tenantId}
queue:{queueId}
user:{userId}
conversation:{conversationId}
```

Publicar eventos:

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

Os eventos deverão:

* possuir `EventId`;
* possuir `TenantId`;
* possuir correlation ID;
* possuir timestamp;
* possuir versão do recurso quando aplicável;
* ser publicados somente após commit;
* permitir deduplicação;
* não ser utilizados como fonte de verdade;
* respeitar autenticação e autorização;
* impedir inscrição em grupos de outro tenant.

### Segurança e consistência

Aplicar:

* políticas de autorização;
* isolamento multi-tenant;
* paginação server-side;
* projeções eficientes;
* prevenção de N+1;
* `ProblemDetails`;
* HTTP 409 para conflitos;
* concurrency token;
* idempotency key;
* rate limit;
* dados mascarados;
* logs sanitizados;
* OpenTelemetry;
* cancelamento de requisições;
* índices adequados.

### Testes

Criar:

* testes unitários;
* testes de integração;
* testes de autorização;
* testes multi-tenant;
* testes de paginação;
* testes de concorrência;
* testes de idempotência;
* testes de Outbox;
* testes de reagendamento;
* testes de auditoria;
* testes SignalR;
* testes E2E de backend sem dependência da internet.

### Ordem inicial

Antes de alterar o código:

1. analisar os endpoints administrativos existentes;
2. identificar lacunas;
3. listar módulos impactados;
4. listar arquivos novos e alterados;
5. listar migrations e índices;
6. descrever autorização;
7. descrever isolamento multi-tenant;
8. descrever idempotência;
9. descrever concorrência;
10. descrever envio manual via Outbox;
11. descrever publicação SignalR após commit;
12. identificar riscos.

Implementar inicialmente apenas:

* fundação administrativa;
* paginação;
* `ProblemDetails`;
* políticas de autorização;
* lista e detalhe de conversas;
* histórico de mensagens;
* marcação de leitura;
* lista da fila humana;
* assumir conversa;
* liberar conversa;
* pausar automação;
* retomar automação;
* concorrência otimista;
* testes;
* OpenAPI;
* documentação inicial.

Após implementar:

```bash
dotnet restore
dotnet build
dotnet test
```

Corrigir todos os erros antes de avançar.

Não avançar para envio manual, dashboard, reagendamento, pacientes, WhatsApp, templates, auditoria ou SignalR enquanto:

* a solução não compilar;
* a paginação não estiver validada;
* a autorização não estiver testada;
* o isolamento multi-tenant não estiver comprovado;
* a concorrência não estiver validada;
* todos os testes da primeira entrega não passarem.

A antiga etapa de Inteligência Artificial, RAG e Tool Calling deverá ser renumerada para uma etapa posterior e permanecer adiada até a conclusão da Etapa 9 e dos testes E2E.
::: 
