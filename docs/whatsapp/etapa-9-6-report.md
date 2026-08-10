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

## Fora desta entrega

As mutations de agendamento, reagendamento, cancelamento e confirmação agora são executadas somente após confirmação explícita. O reagendamento valida a versão persistida e preserva a consulta original como `Rescheduled`, criando uma substituta.

## Validação

`dotnet build --no-restore` passou sem warnings ou erros e a collection Postman foi validada como JSON. `dotnet restore` não concluiu neste ambiente durante a execução (ficou sem saída e foi cancelado). `dotnet test` compilou os testes, mas o runner foi bloqueado pelo ambiente ao abrir o socket local (`SocketException: Permission denied`); executar diretamente no Terminal do Mac. O smoke Fake WhatsApp permanece como validação operacional manual, sem mensagens reais Twilio.
