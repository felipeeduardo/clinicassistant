# Etapa 9.6 — Conversação WhatsApp Humanizada e Fluxos Curtos

## Contexto

O Clinic Assistant já possui integração com WhatsApp, conversas, orquestração, fila humana, agenda, profissionais, especialidades e operações administrativas.

O fluxo atual utiliza um menu funcional, porém rígido:

```text
Escolha uma opção do menu para continuar.

1 - Ver especialidades
2 - Ver profissionais
3 - Consultar disponibilidade
4 - Agendar consulta
5 - Reagendar consulta
6 - Cancelar consulta
7 - Confirmar consulta
8 - Falar com atendente
```

Essa abordagem funciona como fallback, mas pode gerar uma experiência mecânica, longa e pouco natural.

O objetivo desta etapa é transformar o fluxo em uma conversa curta, humana, previsível e segura, sem depender de IA generativa para funcionar.

A IA, RAG e Tool Calling continuam fora do escopo desta etapa.

---

# 1. Objetivo

Evoluir a conversação do WhatsApp para permitir que o paciente:

- escreva em linguagem natural;
- continue podendo usar números;
- receba respostas curtas;
- tenha poucas perguntas por fluxo;
- possa voltar ao menu facilmente;
- possa cancelar uma operação;
- possa falar com atendente a qualquer momento;
- não fique preso em loops;
- não precise repetir informações já fornecidas;
- receba confirmação clara antes de ações críticas;
- tenha mensagens coerentes com o estado atual da conversa.

---

# 2. Princípios conversacionais obrigatórios

A experiência deverá seguir:

```text
curta
clara
humana
determinística
recuperável
auditável
segura
```

Regras:

- uma pergunta por mensagem;
- no máximo uma decisão principal por etapa;
- não repetir menu completo após toda resposta;
- não repetir dados já conhecidos;
- confirmar apenas ações importantes;
- permitir "voltar", "menu", "cancelar" e "atendente";
- evitar mais de 3 mensagens consecutivas sem progresso;
- oferecer saída humana após erros repetidos;
- nunca esconder que o usuário está falando com automação;
- não utilizar linguagem excessivamente robótica;
- não enviar mensagens longas;
- não criar loops infinitos;
- não depender exclusivamente de números;
- aceitar linguagem natural simples;
- não interpretar mensagens ambíguas como confirmação de ação destrutiva.

---

# 3. Novo menu principal

Substituir o menu rígido por uma abertura humanizada.

Mensagem sugerida:

```text
Olá! 👋
Posso ajudar você com sua consulta.

Você pode me dizer o que precisa ou escolher uma opção:

1. Especialidades
2. Profissionais
3. Horários disponíveis
4. Agendar consulta
5. Reagendar consulta
6. Cancelar consulta
7. Confirmar consulta
8. Falar com atendente

Ex.: "Quero marcar uma consulta com cardiologista."
```

Não repetir essa mensagem inteira em toda interação.

Depois da primeira exibição, utilizar versão curta:

```text
Como posso ajudar?

1. Especialidades
2. Profissionais
3. Horários
4. Agendar
5. Reagendar
6. Cancelar
7. Confirmar
8. Atendente
```

---

# 4. Entrada livre + atalhos numéricos

O orquestrador deverá reconhecer tanto:

```text
1
2
3
4
5
6
7
8
```

quanto frases equivalentes.

Exemplos:

## Especialidades

```text
quais especialidades vocês têm?
tem cardiologista?
quero ver especialidades
1
```

## Profissionais

```text
quais médicos atendem?
quem atende cardiologia?
quero ver os profissionais
2
```

## Disponibilidade

```text
tem horário amanhã?
quais horários estão disponíveis?
tem vaga sexta?
3
```

## Agendamento

```text
quero marcar uma consulta
preciso agendar
quero marcar cardiologista
4
```

## Reagendamento

```text
quero mudar meu horário
preciso remarcar
quero reagendar
5
```

## Cancelamento

```text
quero cancelar
não vou conseguir ir
cancela minha consulta
6
```

## Confirmação

```text
quero confirmar minha consulta
vou comparecer
confirmar horário
7
```

## Atendimento humano

```text
quero falar com alguém
atendente
humano
recepção
8
```

---

# 5. Intent Resolver

Criar ou evoluir um resolver determinístico:

```csharp
public interface IConversationIntentResolver
{
    ConversationIntentResolution Resolve(
        string message,
        ConversationContext context);
}
```

Intents iniciais:

```text
ViewSpecialties
ViewProfessionals
CheckAvailability
ScheduleAppointment
RescheduleAppointment
CancelAppointment
ConfirmAppointment
HumanHandoff

GoBack
MainMenu
CancelCurrentFlow
Repeat
Help
Unknown
```

Não utilizar LLM nesta etapa.

Utilizar:

- normalização;
- aliases;
- keywords;
- expressões equivalentes;
- estado atual;
- regras explícitas.

---

# 6. Normalização

Antes de interpretar:

- trim;
- lowercase;
- remover espaços extras;
- normalizar acentos apenas para matching;
- manter mensagem original para auditoria;
- normalizar números;
- não remover conteúdo necessário para nomes.

Exemplo:

```text
"Quero REAGENDAR minha consulta!"
→ "quero reagendar minha consulta"
```

---

# 7. Navegação global

Os comandos abaixo devem funcionar em qualquer etapa:

```text
menu
início
inicio
voltar ao menu
começar de novo
```

Resultado:

```text
MainMenu
```

Voltar:

```text
voltar
anterior
```

Resultado:

```text
GoBack
```

Cancelar operação atual:

```text
cancelar operação
sair
desistir
```

Resultado:

```text
CancelCurrentFlow
```

Atendente:

```text
atendente
humano
falar com alguém
recepção
```

Resultado imediato:

```text
HumanHandoff
```

Nunca obrigar o usuário a terminar um fluxo antes de pedir atendimento humano.

---

# 8. Prevenção de loops

Adicionar controle de tentativas.

Por etapa:

```text
MaxInvalidAttempts = 2
```

Fluxo:

```text
1ª resposta inválida
→ explicar de forma curta

2ª resposta inválida
→ oferecer opções simples

3ª tentativa necessária
→ oferecer atendente ou menu
```

Exemplo:

```text
Não consegui identificar essa opção.

Você pode responder com o número da opção ou escrever o que deseja fazer.
```

Na repetição:

```text
Ainda não consegui entender.

Quer voltar ao menu ou falar com um atendente?
```

Não repetir indefinidamente a mesma mensagem.

---

# 9. Contexto da conversa

Criar ou evoluir:

```text
ConversationContext
```

Campos equivalentes:

```text
TenantId
ConversationId
PatientId
CurrentIntent
CurrentStep
PreviousStep
InvalidAttemptCount
SelectedSpecialtyId
SelectedProfessionalId
SelectedUnitId
SelectedDate
SelectedSlotId
SelectedAppointmentId
PendingConfirmation
LastUserMessage
LastBotMessage
FlowStartedAt
LastInteractionAt
```

Persistir apenas o necessário.

Não depender de memória local do processo.

---

# 10. Stack de navegação curta

Não criar uma state machine excessivamente complexa.

Implementar histórico curto:

```text
PreviousStep
```

ou stack limitada.

Permitir:

```text
voltar
```

sem reconstruir toda a conversa.

Limitar profundidade.

---

# 11. Fluxo — Ver especialidades

Entrada:

```text
quais especialidades vocês têm?
```

Resposta:

```text
Claro. Estas são algumas especialidades disponíveis:

1. Cardiologia
2. Dermatologia
3. Pediatria
4. Ortopedia

Qual delas você procura?
```

Se lista grande:

- mostrar primeiras opções;
- permitir busca textual;
- não enviar dezenas de itens.

Se usuário escrever:

```text
cardiologia
```

responder:

```text
Perfeito. Posso mostrar os profissionais de Cardiologia ou consultar horários disponíveis.

O que prefere?
```

---

# 12. Fluxo — Ver profissionais

Entrada:

```text
quais médicos atendem cardiologia?
```

Se especialidade identificada:

não perguntar novamente.

Resposta:

```text
Para Cardiologia, encontrei:

1. Dra. Ana Souza
2. Dr. Bruno Lima

Quer ver os horários de algum deles?
```

Se especialidade não informada:

```text
Qual especialidade você procura?
```

Não mostrar profissionais de todas as especialidades sem necessidade.

---

# 13. Fluxo — Consultar disponibilidade

Objetivo: chegar a horários com poucas perguntas.

Se contexto já possui:

```text
Specialty
Professional
Unit
```

reutilizar.

Fluxo mínimo:

```text
Especialidade/profissional
↓
Data ou período
↓
Horários
```

Exemplo:

Usuário:

```text
tem cardiologista amanhã?
```

Sistema:

```text
Sim. Para amanhã encontrei estes horários:

1. 09:00 — Dra. Ana Souza
2. 10:30 — Dr. Bruno Lima
3. 14:00 — Dra. Ana Souza

Quer agendar algum deles?
```

Não perguntar novamente por dados já informados.

---

# 14. Datas em linguagem natural

Suportar deterministicamente:

```text
hoje
amanhã
depois de amanhã
segunda
terça
quarta
quinta
sexta
sábado
domingo
```

Quando ambíguo:

```text
Você quis dizer terça-feira, 11/08?
```

Backend continua responsável por disponibilidade real.

---

# 15. Fluxo — Agendar consulta

Fluxo ideal:

```text
Intent
↓
Specialty
↓
Professional opcional
↓
Date
↓
Slot
↓
Confirmation
↓
Create
```

Perguntar somente campos ausentes.

Exemplo:

Usuário:

```text
quero marcar cardiologista amanhã
```

Resposta:

```text
Encontrei estes horários para Cardiologia amanhã:

1. 09:00 — Dra. Ana Souza
2. 10:30 — Dr. Bruno Lima
3. 14:00 — Dra. Ana Souza

Qual você prefere?
```

Usuário:

```text
2
```

Resposta:

```text
Certo. Ficará assim:

Cardiologia
Dr. Bruno Lima
Amanhã às 10:30

Posso confirmar o agendamento?
```

Aceitar:

```text
sim
confirmar
pode
ok
```

Não aceitar mensagens ambíguas como confirmação.

---

# 16. Pós-agendamento

Após sucesso:

```text
Consulta agendada ✅

Cardiologia
Dr. Bruno Lima
11/08 às 10:30

Se precisar, você pode escrever "reagendar", "cancelar" ou "menu".
```

Não reenviar menu completo.

---

# 17. Idempotência no agendamento

Uma confirmação repetida não pode criar múltiplas consultas.

Usar:

```text
Idempotency-Key
PendingConfirmation
Appointment creation lock
```

Depois do sucesso, limpar contexto temporário.

---

# 18. Fluxo — Reagendar

Entrada:

```text
quero mudar minha consulta
```

Sistema:

1. buscar consultas futuras;
2. se houver uma, apresentar diretamente;
3. se houver várias, permitir escolher;
4. perguntar nova data;
5. listar slots;
6. confirmar mudança.

Exemplo:

```text
Encontrei sua consulta:

Cardiologia
Dra. Ana Souza
12/08 às 09:00

Qual nova data você prefere?
```

Depois:

```text
Para 14/08 tenho:

1. 08:30
2. 10:00
3. 15:30
```

Confirmação:

```text
Deseja mudar de 12/08 às 09:00 para 14/08 às 10:00?
```

Utilizar:

```text
expectedVersion
Idempotency-Key
```

---

# 19. Conflito no reagendamento

Se o slot for ocupado:

```text
Esse horário acabou de ficar indisponível.

Tenho estas alternativas:

1. 10:30
2. 11:00
3. 14:00
```

Não reiniciar todo o fluxo.

Preservar contexto útil.

---

# 20. Fluxo — Cancelar consulta

Nunca cancelar somente porque o usuário escreveu "cancelar".

Primeiro identificar consulta.

Exemplo:

```text
Encontrei esta consulta:

Dermatologia
Dra. Carla Mendes
15/08 às 14:00

Deseja realmente cancelar?
```

Somente confirmação explícita executa mutation.

---

# 21. Fluxo — Confirmar consulta

Se houver consulta futura elegível:

```text
Encontrei sua consulta:

Pediatria
Dr. João Lima
13/08 às 16:00

Deseja confirmar presença?
```

Após sucesso:

```text
Presença confirmada ✅

Esperamos você no dia 13/08 às 16:00.
```

Se já confirmada:

```text
Essa consulta já está confirmada.
```

---

# 22. Fluxo — Atendimento humano

Deverá funcionar em qualquer ponto.

Mensagem:

```text
Tudo bem. Vou encaminhar sua conversa para nossa equipe.

Assim que alguém assumir o atendimento, você continuará por aqui.
```

Ações:

```text
PauseAutomation
CreateHumanQueueItem
HumanHandoff event
```

Não continuar enviando menus automáticos depois do handoff.

---

# 23. Retomada da automação

Quando humano liberar e automação for retomada:

```text
O atendimento automático foi retomado.

Se precisar, é só me dizer o que deseja fazer.
```

Não enviar menu grande automaticamente.

---

# 24. Unknown intent

Primeira ocorrência:

```text
Não consegui entender exatamente o que você precisa.

Você pode escrever algo como:
"quero marcar uma consulta"
"quero mudar meu horário"
"quero falar com atendente"
```

Segunda ocorrência:

```text
Quer escolher pelo menu ou falar com um atendente?
```

---

# 25. Ajuda contextual

Se usuário escrever:

```text
ajuda
o que posso fazer?
```

Responder de acordo com o estado.

No menu:

```text
Posso ajudar a consultar médicos e horários, marcar, reagendar, cancelar ou confirmar consultas.
```

Durante reagendamento:

```text
Estamos escolhendo um novo horário para sua consulta.

Você pode informar uma data, escrever "voltar" ou "cancelar operação".
```

---

# 26. Mensagens curtas

Meta:

```text
2–6 linhas
```

na maior parte das interações.

Evitar múltiplas perguntas na mesma mensagem.

---

# 27. Formatação WhatsApp

Usar com moderação:

```text
*negrito*
emoji
listas numeradas
```

Sugestão:

```text
👋 abertura
✅ sucesso
⚠️ atenção
```

---

# 28. Mensagens interativas

Quando o provider suportar:

- avaliar quick replies;
- avaliar list messages;
- manter fallback textual.

A orquestração não pode depender exclusivamente de componentes interativos.

---

# 29. Arquitetura

Fluxo:

```text
Inbound webhook
      ↓
Inbox
      ↓
Message normalization
      ↓
Global command resolver
      ↓
Intent resolver
      ↓
Conversation state
      ↓
Flow handler
      ↓
Domain/API operation
      ↓
Conversation response
      ↓
Outbox
      ↓
Worker
      ↓
WhatsApp gateway
```

Não enviar diretamente no webhook.

---

# 30. Flow handlers

Criar handlers pequenos:

```text
MainMenuFlow
SpecialtiesFlow
ProfessionalsFlow
AvailabilityFlow
ScheduleFlow
RescheduleFlow
CancellationFlow
ConfirmationFlow
HumanHandoffFlow
```

Evitar um único `ConversationOrchestrator` gigante.

---

# 31. Contrato de flow handler

Exemplo:

```csharp
public interface IConversationFlowHandler
{
    ConversationIntent Intent { get; }

    Task<ConversationFlowResult> HandleAsync(
        ConversationFlowContext context,
        CancellationToken cancellationToken);
}
```

Resultado:

```text
Messages
NextStep
ContextChanges
DomainCommands
ShouldPauseAutomation
ShouldEndFlow
```

---

# 32. Seleções numéricas contextuais

O número `1` só significa `ViewSpecialties` no `MainMenu`.

Nos demais estados, números devem representar seleção contextual de:

- especialidade;
- profissional;
- slot;
- consulta.

---

# 33. Expiração de fluxo

Criar timeout de fluxo, por exemplo:

```text
30 minutos
```

Após expiração:

```text
CurrentIntent = null
CurrentStep = MainMenu
PendingConfirmation = false
```

Próxima mensagem:

```text
Vamos continuar por aqui. Como posso ajudar?
```

---

# 34. Persistência

Persistir estado conversacional de forma transacional.

Requisitos:

- version;
- concorrência;
- timestamp;
- tenant;
- conversation;
- intent;
- step.

Evitar duas mensagens simultâneas corromperem estado.

---

# 35. Idempotência inbound

Webhooks duplicados não podem executar a operação duas vezes.

Deduplicar por:

```text
MessageSid
```

ou external message ID.

Garantir especialmente ações transacionais.

---

# 36. Auditoria

Auditar:

```text
intent.detected
flow.started
flow.completed
flow.cancelled
flow.expired
human_handoff.requested
appointment.schedule.requested
appointment.reschedule.requested
appointment.cancel.requested
appointment.confirm.requested
```

---

# 37. Observabilidade

Métricas:

```text
conversation_intent_total
conversation_unknown_intent_total
conversation_invalid_input_total
conversation_flow_started_total
conversation_flow_completed_total
conversation_flow_abandoned_total
conversation_flow_timeout_total
conversation_handoff_total
conversation_messages_per_flow
```

---

# 38. Eficiência

Medir:

```text
AverageMessagesToSchedule
AverageMessagesToReschedule
AverageMessagesToCancel
```

Meta inicial:

```text
Schedule <= 6 mensagens do paciente
Reschedule <= 5
Cancel <= 3
Confirm <= 2
```

---

# 39. Testes unitários

Cobrir intents, comandos globais, números contextuais, loops e normalização.

---

# 40. Testes de contexto

Validar:

```text
"1" no menu
→ primeira opção do menu

"1" escolhendo profissional
→ primeiro profissional

"1" escolhendo slot
→ primeiro slot
```

---

# 41. Testes de loops

Após respostas inválidas repetidas:

- não repetir indefinidamente;
- oferecer menu ou atendente;
- resetar contador após input válido.

---

# 42. Testes de agendamento

Fluxo completo com linguagem natural, seleção numérica e confirmação.

---

# 43. Testes de reagendamento

Cobrir conflito concorrente sem reiniciar o fluxo inteiro.

---

# 44. Testes de cancelamento

Validar confirmação explícita obrigatória.

---

# 45. Testes de handoff

`atendente` em qualquer etapa deverá:

```text
PauseAutomation
CreateQueueItem
StopBotResponses
```

---

# 46. E2E Fake WhatsApp

Criar cenários:

```text
linguagem natural
menu numérico
voltar
menu
unknown intent
handoff
```

---

# 47. Duplicidade

Mesmo external ID deve produzir apenas um processamento.

---

# 48. Postman

Adicionar pasta:

```text
E2E Flows / Conversational WhatsApp
```

Cenários:

```text
Main Menu
Natural Language
Availability
Schedule
Reschedule
Cancel
Confirm
Human Handoff
Unknown Intent
Duplicate Webhook
```

---

# 49. Documentação

Criar ou atualizar:

```text
docs/whatsapp/conversation-design.md
docs/whatsapp/conversation-intents.md
docs/whatsapp/conversation-flows.md
docs/whatsapp/conversation-state.md
docs/whatsapp/human-handoff.md
docs/whatsapp/conversation-testing.md
docs/whatsapp/conversation-copy.md
```

---

# 50. Catálogo de mensagens

Centralizar textos do bot.

Categorias:

```text
Greeting
MainMenu
Help
InvalidInput
Specialties
Professionals
Availability
Schedule
Reschedule
Cancel
Confirm
HumanHandoff
Errors
```

Preparar para futura internacionalização.

---

# 51. Tom de voz

Tom:

```text
profissional
acolhedor
objetivo
simples
```

Evitar linguagem técnica ou robótica.

---

# 52. Erros técnicos

Nunca mostrar ao paciente:

```text
HTTP 409
stack trace
Twilio error code
database error
```

Mapear para mensagens conversacionais.

---

# 53. Falha geral

Mensagem:

```text
Não consegui concluir isso agora.

Você pode tentar novamente ou falar com um atendente.
```

Registrar `traceId` internamente.

---

# 54. Critérios de aceite

A etapa estará concluída quando:

1. menu estiver humanizado;
2. linguagem natural simples funcionar;
3. números continuarem funcionando;
4. números forem contextuais;
5. menu global funcionar;
6. voltar funcionar;
7. cancelar fluxo funcionar;
8. humano funcionar em qualquer etapa;
9. loops forem limitados;
10. fluxos informativos forem curtos;
11. agendamento reutilizar contexto;
12. agendamento for idempotente;
13. reagendamento preservar contexto em conflito;
14. cancelamento exigir confirmação;
15. confirmação não duplicar mutation;
16. estado persistido estiver seguro;
17. duplicidade inbound estiver tratada;
18. Outbox continuar obrigatória;
19. handlers estiverem separados;
20. mensagens estiverem centralizadas;
21. métricas existirem;
22. testes passarem;
23. E2E Fake passar;
24. Postman estiver atualizado;
25. documentação estiver atualizada;
26. nenhum LLM for obrigatório;
27. nenhum fluxo entrar em loop infinito.

---

# 55. Ordem de implementação

```text
9.6.1 Auditoria
9.6.2 Fundação conversacional
9.6.3 Catálogo de mensagens
9.6.4 Especialidades
9.6.5 Profissionais
9.6.6 Disponibilidade
9.6.7 Agendamento
9.6.8 Reagendamento
9.6.9 Cancelamento e confirmação
9.6.10 Human handoff
9.6.11 Resiliência
9.6.12 Testes e observabilidade
9.6.13 Documentação
```

---

# 56. Primeira entrega

Implemente inicialmente:

```text
9.6.1 Auditoria
9.6.2 Fundação conversacional
9.6.3 Catálogo de mensagens
9.6.4 Especialidades
9.6.5 Profissionais
9.6.6 Disponibilidade
```

Não implementar ainda mutations de agendamento/reagendamento/cancelamento/confirmação até a fundação estar estável.

---

# 57. Segunda entrega

Depois da fundação validada:

```text
Schedule
Reschedule
Cancel
Confirm
```

com idempotência, `expectedVersion`, confirmação explícita e conflito.

---

# 58. Relatório final

Apresentar:

1. arquitetura anterior;
2. problemas encontrados;
3. intents criados;
4. comandos globais;
5. handlers;
6. estados;
7. mensagens alteradas;
8. redução de passos por fluxo;
9. tratamento de loop;
10. tratamento de conflito;
11. idempotência;
12. handoff;
13. testes;
14. métricas;
15. Postman;
16. documentação;
17. riscos restantes;
18. próximos passos.

Não implementar IA nesta etapa.
