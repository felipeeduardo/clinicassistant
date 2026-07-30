Implemente agora a Etapa 7 do projeto Clinic Assistant.

Utilize como fonte de verdade o documento:

`docs/prompts/etapa-07-conversas-orquestracao.md`

A Etapa 7 será responsável pelo módulo de conversas, controle do estado do atendimento e orquestração dos fluxos administrativos.

Considere que a Etapa 6 já implementou ou preparou:

* integração com WhatsApp por meio do Twilio;
* `IWhatsAppGateway`;
* `FakeWhatsAppGateway`;
* `TwilioWhatsAppGateway`;
* webhook de mensagens recebidas;
* `StatusCallback`;
* validação de `X-Twilio-Signature`;
* Inbox;
* Outbox;
* RabbitMQ;
* persistência de mensagens;
* criação ou localização inicial de pacientes;
* criação ou localização inicial de conversas;
* envio assíncrono.

A Etapa 7 deverá consumir os eventos produzidos pela Etapa 6 e transformar mensagens isoladas em atendimentos administrativos coerentes e controlados.

O sistema não deve funcionar como um chatbot genérico.

Ele deverá atuar como um assistente administrativo para clínicas e consultórios, capaz de:

* iniciar e continuar conversas;
* manter contexto;
* identificar intenções administrativas;
* apresentar menus;
* consultar informações institucionais;
* listar especialidades;
* listar profissionais;
* consultar disponibilidade;
* conduzir agendamentos;
* conduzir reagendamentos;
* conduzir cancelamentos;
* confirmar consultas;
* solicitar dados faltantes;
* tratar respostas inválidas;
* transferir para atendimento humano;
* pausar e retomar automação;
* encerrar e reabrir conversas.

Não implementar:

* diagnóstico médico;
* recomendação de tratamento;
* prescrição;
* interpretação de sintomas;
* triagem clínica automatizada;
* aconselhamento médico.

Perguntas clínicas ou sensíveis deverão provocar handoff para atendimento humano.

## Antes de alterar o código

Execute primeiro uma análise completa da solução atual.

Apresente:

1. os projetos e módulos existentes;
2. o que já foi implementado pela Etapa 6;
3. entidades existentes que serão evoluídas;
4. novos agregados e entidades necessários;
5. arquivos que serão criados;
6. arquivos que serão alterados;
7. migrations necessárias;
8. dependências adicionais;
9. fluxo atual de uma mensagem recebida;
10. ponto exato em que o módulo de conversas será acionado;
11. desenho da máquina de estados;
12. estratégia de idempotência;
13. estratégia de lock por conversa;
14. estratégia de concorrência otimista;
15. integração com o módulo de Scheduling;
16. estratégia de handoff humano;
17. riscos técnicos encontrados;
18. possíveis conflitos com decisões arquiteturais anteriores.

Não altere o código antes de concluir essa análise.

## Arquitetura obrigatória

Respeite as responsabilidades:

```text
ClinicAssistant.Domain
    Entidades, regras, invariantes, transições e eventos de domínio.

ClinicAssistant.Application
    Commands, queries, handlers, orquestração, contratos e validações.

ClinicAssistant.Infrastructure
    PostgreSQL, Redis, RabbitMQ, locks e repositórios.

ClinicAssistant.Api
    Endpoints administrativos e atendimento humano.

ClinicAssistant.Worker
    Consumers, Outbox, expiração e processamento assíncrono.
```

O módulo de conversas não poderá:

* depender diretamente do SDK do Twilio;
* enviar mensagens diretamente pelo Twilio;
* publicar diretamente no RabbitMQ antes do commit;
* manter o estado apenas em memória;
* usar Redis como única fonte de verdade;
* executar agendamentos sem transação;
* confiar em IDs recebidos sem validar o tenant.

As mensagens de saída deverão seguir:

```text
ConversationOrchestrator
        ↓
ConversationMessage Pending
        ↓
OutboxMessage
        ↓
Commit
        ↓
Worker
        ↓
IWhatsAppGateway
```

## Implemente inicialmente apenas

### Subetapa 7.1 — Fundação

Implemente:

* evolução da entidade `Conversation`;
* entidade `ConversationState`;
* entidade `ConversationProcessedMessage`;
* entidade `ConversationOption`;
* enums de status, intenção, prioridade e modo de automação;
* configurações tipadas;
* índices;
* constraints;
* migrations;
* configuração de injeção de dependência.

### Subetapa 7.2 — Máquina de estados

Implemente:

* `IConversationStateMachine`;
* `ConversationInput`;
* `ConversationTransitionResult`;
* `ConversationAction`;
* regras básicas de transição;
* menu inicial;
* identificação determinística inicial de intenção;
* tratamento de resposta inválida;
* comando de voltar;
* comando de menu;
* cancelamento do fluxo;
* solicitação de handoff;
* expiração básica do estado.

As intenções iniciais deverão incluir:

```text
Unknown
Greeting
InstitutionalQuestion
ListSpecialties
ListProfessionals
CheckAvailability
ScheduleAppointment
RescheduleAppointment
CancelAppointment
ConfirmAppointment
TalkToHuman
Farewell
Unsupported
```

### Subetapa 7.3 — Orquestração básica

Implemente:

* `IConversationOrchestrator`;
* `ProcessConversationMessageCommand`;
* implementação inicial do orquestrador;
* consumer do evento `ConversationMessageReceived`;
* lock por `TenantId` e `ConversationId`;
* implementação do lock com Redis;
* TTL do lock;
* liberação segura por token;
* concorrência otimista com campo `Version`;
* idempotência por `ConversationMessageId`;
* persistência transacional do estado;
* criação de mensagens de saída;
* criação de `OutboxMessage`;
* logs;
* métricas;
* traces;
* tratamento de falhas.

## Fluxo mínimo esperado

Implemente inicialmente o seguinte fluxo:

```text
1. Paciente envia uma mensagem
2. Etapa 6 persiste a mensagem
3. Evento ConversationMessageReceived é publicado
4. Consumer recebe o evento
5. Lock da conversa é obtido
6. Conversa e estado são carregados
7. Idempotência é verificada
8. Entrada é normalizada
9. Intenção é identificada
10. Máquina de estados calcula a transição
11. Estado é atualizado
12. Resposta é criada como ConversationMessage
13. OutboxMessage é criada
14. Transação é confirmada
15. Lock é liberado
16. Worker da Etapa 6 envia a resposta
```

## Fluxos que ainda não devem ser implementados completamente

Não implemente nesta primeira entrega:

* criação real de agendamento;
* reagendamento real;
* cancelamento real;
* reserva de slot;
* busca completa de disponibilidade;
* fila humana completa;
* painel de atendimento;
* classificação por IA;
* geração de respostas por IA;
* integração com prontuário;
* processamento clínico;
* frontend completo de conversas.

Crie somente interfaces ou contratos quando forem necessários para evitar acoplamento futuro.

## Concorrência

Implemente duas camadas de proteção:

```text
1. Lock distribuído no Redis
2. Concorrência otimista no PostgreSQL
```

O lock deverá utilizar:

```text
TenantId
ConversationId
Token único
TTL
Timeout de aquisição
```

A liberação deverá ocorrer somente pelo proprietário do token.

O banco deverá possuir um campo de versão ou concurrency token.

Duas mensagens simultâneas da mesma conversa não poderão:

* produzir duas respostas conflitantes;
* sobrescrever o estado;
* criar ações duplicadas;
* avançar a conversa duas vezes;
* provocar agendamentos duplicados.

## Idempotência

Criar uma garantia persistente baseada em:

```text
TenantId + ConversationMessageId
```

Ao receber novamente uma mensagem já processada:

* não executar a máquina de estados;
* não atualizar novamente o estado;
* não criar nova mensagem de saída;
* não criar nova Outbox;
* não executar ação administrativa;
* considerar o processamento concluído com sucesso;
* registrar métrica de duplicidade.

## Multi-tenancy

Todas as consultas e comandos deverão validar:

```text
TenantId
ConversationId
PatientId
ConversationMessageId
IntegrationId
ConversationStateId
```

Não buscar recursos apenas por ID.

Criar testes que comprovem que dados de um tenant não podem ser acessados ou modificados por outro tenant.

## Mensagens configuráveis

Não codifique textos diretamente nos handlers.

Crie uma abstração de composição:

```csharp
public interface IConversationResponseComposer
{
    ConversationResponse Compose(
        ConversationResponseRequest request);
}
```

Prepare suporte para chaves como:

```text
conversation.greeting
conversation.menu
conversation.invalid_answer
conversation.expired
conversation.handoff
conversation.closed
```

Pode existir uma implementação inicial em memória ou por configuração, desde que a aplicação não fique acoplada aos textos.

## Configurações

Adicionar opções equivalentes a:

```env
CONVERSATION__STATE_EXPIRATION_MINUTES=30
CONVERSATION__IDLE_CLOSE_HOURS=24
CONVERSATION__MAXIMUM_INVALID_ATTEMPTS=3
CONVERSATION__LOCK_TIMEOUT_SECONDS=10
CONVERSATION__LOCK_TTL_SECONDS=60
CONVERSATION__MAX_OPTIONS_PER_MESSAGE=10
CONVERSATION__MAX_MESSAGE_LENGTH=2000
CONVERSATION__DEFAULT_LANGUAGE=pt-BR
CONVERSATION__REOPEN_CLOSED_CONVERSATIONS=true
```

Validar configurações no startup.

## Testes obrigatórios da primeira entrega

Criar testes unitários para:

* início de conversa;
* saudação;
* exibição do menu;
* identificação de intenção;
* transição válida;
* transição inválida;
* comando menu;
* comando voltar;
* cancelamento de fluxo;
* resposta inválida;
* limite de respostas inválidas;
* solicitação de handoff;
* estado expirado;
* idempotência;
* isolamento multi-tenant;
* lock obtido;
* lock indisponível;
* conflito de versão;
* criação de mensagem de saída;
* criação de Outbox.

Criar testes de integração para:

### Nova conversa

```text
Dada uma mensagem recebida
Quando o consumer processar
Então a conversa será carregada
E o estado inicial será criado
E uma resposta será registrada
E uma OutboxMessage será criada
```

### Mensagem duplicada

```text
Dado o mesmo ConversationMessageId
Quando for processado duas vezes
Então apenas uma resposta será criada
```

### Concorrência

```text
Dadas duas mensagens simultâneas
Quando forem processadas
Então somente uma modificará o estado por vez
```

### Tenant incorreto

```text
Dado um recurso pertencente a outro tenant
Quando o processamento for tentado
Então a operação será rejeitada
```

### Estado expirado

```text
Dado um estado expirado
Quando uma mensagem for recebida
Então a ação anterior não será executada
E o fluxo retornará ao menu
```

## Documentação

Criar inicialmente:

```text
docs/conversations/overview.md
docs/conversations/state-machine.md
docs/conversations/concurrency.md
docs/conversations/security.md
docs/conversations/testing.md
```

Utilizar diagramas Mermaid para:

* fluxo de processamento;
* máquina de estados inicial;
* lock e concorrência;
* idempotência;
* criação da resposta pela Outbox.

## Critérios da primeira entrega

A primeira entrega estará concluída quando:

```text
1. A solução compilar
2. Conversation estiver evoluída
3. ConversationState estiver persistida
4. Máquina de estados estiver implementada
5. Orquestrador básico estiver funcional
6. Consumer estiver funcional
7. Idempotência estiver persistida no PostgreSQL
8. Lock Redis estiver implementado
9. Concorrência otimista estiver configurada
10. Menu inicial funcionar
11. Intenções básicas forem classificadas
12. Respostas inválidas forem tratadas
13. Estado expirado for tratado
14. Handoff básico estiver preparado
15. Mensagens de saída utilizarem Outbox
16. Nenhuma classe chamar o Twilio diretamente
17. Multi-tenancy estiver protegido
18. Logs não expuserem conteúdo sensível
19. Testes unitários passarem
20. Testes de integração passarem
21. Documentação estiver atualizada
```

## Validação final

Após implementar, execute:

```bash
dotnet restore
dotnet build
dotnet test
```

Caso o projeto utilize comandos adicionais de lint, formatação ou arquitetura, execute-os também.

Corrija todos os erros encontrados.

Ao finalizar, apresente:

1. resumo da implementação;
2. arquivos criados;
3. arquivos modificados;
4. migrations criadas;
5. decisões arquiteturais;
6. testes executados;
7. resultado de cada comando;
8. limitações da entrega;
9. riscos pendentes;
10. próximos passos recomendados.

Não avance para as Subetapas 7.4 em diante enquanto:

* a solução não compilar;
* todos os testes não passarem;
* a idempotência não estiver comprovada;
* a concorrência não estiver testada;
* o isolamento multi-tenant não estiver validado;
* a documentação não estiver atualizada.
