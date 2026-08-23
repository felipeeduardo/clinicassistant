# Etapa 9.6 — relatório da primeira entrega

## Entregue

- resolver determinístico com aliases, frases naturais e comandos globais;
- números contextuais apoiados pelas opções persistidas;
- contexto conversacional persistido em `ConversationState.ContextJson`;
- limite de entradas inválidas com recuperação e handoff;
- catálogo central de mensagens humanizadas;
- listagem de especialidades e profissionais ativos com opções numeradas;
- persistência determinística de datas `hoje`, `amanhã` e `depois de amanhã`;
- pasta Postman `E2E Flows / Conversational WhatsApp`;
- documentação de design, intents, estado, fluxos, handoff, cópia e testes.
- métricas conversacionais de intent, entradas inválidas, ciclo do fluxo, handoff, timeout e mensagens por fluxo.
- navegação curta de agendamento: especialidade → profissional → data → horário → confirmação.
- confirmação de presença, cancelamento e reagendamento com seleção contextual;
- mutations transacionais protegidas por idempotência, versão e verificação de conflito.
- reagendamento contextual por telefone/tenant, com seleção persistida da consulta,
  reaproveitamento de profissional/especialidade/unidade e pipeline de dias/horários;
- confirmação semântica `confirm_reschedule`, preservando a consulta original em
  conflitos e exibindo os horários no fuso da clínica.

## Fora desta entrega

As mutations de agendamento, reagendamento, cancelamento e confirmação agora são executadas somente após confirmação explícita. O reagendamento valida a versão persistida e preserva a consulta original como `Rescheduled`, criando uma substituta.

## Validação

`dotnet build --no-restore` passou sem warnings ou erros. `dotnet test --no-build --no-restore` passou com 97 testes. O smoke Fake WhatsApp/Twilio permanece como validação operacional manual.
