# Smoke manual — Twilio Sandbox + ngrok

Este roteiro valida manualmente o recebimento e o processamento de mensagens WhatsApp pelo Twilio Sandbox em ambiente local. Ele utiliza o `TwilioWhatsAppGateway`, o webhook assinado, a Outbox, o Worker e o RabbitMQ.

O fluxo é deliberadamente manual: nenhum comando deste roteiro envia mensagem automaticamente para um número real.

## 1. Escopo e segurança

Este smoke valida:

- API local acessível publicamente pelo ngrok;
- webhook de mensagem recebida;
- validação de `X-Twilio-Signature`;
- resolução da integração pelo `IntegrationKey`;
- criação ou reutilização de paciente e conversa;
- persistência da mensagem recebida;
- processamento assíncrono pelo Worker;
- atualização de status por StatusCallback.

Não versionar:

- `TWILIO_AUTH_TOKEN`;
- `NGROK_AUTHTOKEN`;
- Account SID real;
- números reais de telefone;
- cookies, JWTs ou payloads com dados pessoais.

Use somente um destinatário autorizado para o Sandbox e nunca aponte o smoke para produção.

## 2. Pré-requisitos

Instale e valide:

```bash
docker --version
docker compose version
curl --version
ngrok version
```

O Docker Desktop deve estar aberto. O ngrok precisa estar autenticado:

```bash
ngrok config add-authtoken "$NGROK_AUTHTOKEN"
```

No Twilio Console, prepare:

1. uma conta Twilio ativa;
2. o WhatsApp Sandbox habilitado;
3. seu telefone participante do Sandbox;
4. `Account SID` e `Auth Token` disponíveis apenas no ambiente local;
5. o sender do Sandbox em formato E.164 ou `whatsapp:+...`.

No Sandbox mostrado no Console, o sender é `whatsapp:+14155238886`. O número listado em **Sandbox Participants** é o destinatário (`To`) e não deve ser usado como `TWILIO_WHATSAPP_FROM`.

## 3. Configurar a integração

O `.env` usado pelo smoke deve apontar para o ambiente local principal:

```text
DATABASE_TARGET=primary
POSTGRES_DB=clinicassistant
DATABASE_NAME=clinicassistant
ASPNETCORE_ENVIRONMENT=Development
WHATSAPP_PROVIDER=Twilio
```

O banco `clinicassistant_test` é reservado aos testes E2E e não deve ser usado neste roteiro. Para executar E2E, aplique overrides temporários no comando conforme o [guia de execução E2E](e2e-execution-guide.md), sem trocar o `.env` do Twilio Sandbox.

A API precisa estar conectada à integração Twilio do tenant utilizado no smoke. A integração deve possuir:

- `Provider = Twilio`;
- `Status = Connected`;
- `IntegrationKey` válido;
- `WhatsAppFrom` configurado.

Se a base foi criada pelo perfil `minimal`, ela começa com `Fake`. Para preparar a mesma base local para o Sandbox, execute o seed com a opção explícita:

```bash
WHATSAPP_PROVIDER=Twilio \
TWILIO_WHATSAPP_FROM="whatsapp:+numero-do-sender" \
TWILIO_INTEGRATION_KEY="twilio-local-main" \
./scripts/test-data/seed.sh minimal
```

Esse comando atualiza a integração determinística do tenant minimal; não cria credenciais Twilio nem envia mensagens.

A `TWILIO_INTEGRATION_KEY` identifica a integração e não é o Auth Token. Obtenha-a pelo procedimento administrativo interno ou diretamente no banco local, sem registrar o valor em documentação versionada.

Configure as variáveis somente no shell atual:

```bash
export TWILIO_ACCOUNT_SID="seu-account-sid"
export TWILIO_AUTH_TOKEN="seu-auth-token"
export TWILIO_WHATSAPP_FROM="whatsapp:+5511999999999"
export TWILIO_INTEGRATION_KEY="integration-key-do-tenant"
export NGROK_AUTHTOKEN="seu-ngrok-token"
```

Não coloque esses valores em `.env.example`, scripts SQL ou commits.

## 4. Subir o ambiente

Execute o script dedicado:

```bash
./scripts/local/start-twilio-smoke.sh
```

O script:

1. bloqueia execução em `Production`;
2. força `DATABASE_TARGET=primary`;
3. força `WHATSAPP_PROVIDER=Twilio`;
4. sobe PostgreSQL, RabbitMQ, Redis, API, Worker, frontend e ngrok;
5. aguarda `/health/ready` da API;
6. aguarda o inspector do ngrok em `http://localhost:4040`;
7. grava somente a URL pública em `.tmp/ngrok-url`;
8. reinicia API, Worker e frontend com as URLs públicas descobertas, para que a assinatura Twilio seja validada contra o endereço correto;
9. imprime os endpoints para o Twilio Console.

API e Worker recebem o mesmo `WHATSAPP_PROVIDER` e as mesmas opções `Twilio__*`. Sem essa configuração no Worker, a Outbox pode ser processada pelo gateway Fake e a mensagem aparecer como aceita apenas no frontend, sem chegar ao WhatsApp.

Verifique os serviços:

```bash
docker compose ps
curl --fail http://localhost:8080/health/ready
curl --fail http://localhost:3000/login
curl --fail http://localhost:4040/api/tunnels
```

## 5. Configurar os webhooks no Twilio

Use a URL HTTPS mostrada pelo script. Os endpoints atuais são:

```text
https://<ngrok>/api/webhooks/whatsapp/twilio/<integration-key>
https://<ngrok>/api/webhooks/whatsapp/twilio/status/<integration-key>
```

No Twilio Sandbox:

1. abra a configuração de mensagens recebidas;
2. informe a URL de inbound;
3. selecione método `POST`;
4. informe a URL de StatusCallback;
5. selecione método `POST`;
6. salve a configuração.

O endereço do ngrok pode mudar após reiniciar o ambiente; nesse caso, atualize as duas URLs no Console.

## 6. Executar o teste inbound

Com o telefone participante do Sandbox, envie uma mensagem para o sender configurado. A mensagem deve chegar ao endpoint público e seguir o fluxo:

```text
Twilio Sandbox
  -> ngrok
  -> API /api/webhooks/whatsapp/twilio/{integrationKey}
  -> validação X-Twilio-Signature
  -> Inbox/Outbox transacional
  -> Worker/RabbitMQ
  -> paciente e conversa
  -> ConversationMessage inbound
```

Acompanhe os logs:

```bash
docker compose logs -f api worker rabbitmq
```

No frontend, abra `http://localhost:3000/conversations`, procure o paciente pelo telefone e confirme:

- conversa localizada no tenant correto;
- mensagem recebida no histórico;
- status da mensagem visível;
- possibilidade de marcar a mensagem como lida;
- ausência de duplicação ao reenviar o mesmo evento Twilio.

## 7. Validar StatusCallback

Quando uma mensagem outbound for processada pelo Worker, o script configura automaticamente `TWILIO_STATUS_CALLBACK_URL` com a URL completa abaixo, e o cliente HTTP envia esse valor como `StatusCallback` ao Twilio:

```text
POST /api/webhooks/whatsapp/twilio/status/{integrationKey}
```

Verifique nos logs e no frontend que o status evolui somente por transições permitidas, por exemplo:

```text
Queued -> Sent -> Delivered -> Read
```

Callbacks repetidos devem ser idempotentes e não podem duplicar mensagens ou auditoria.

## 8. Diagnóstico rápido

### API não fica pronta

```bash
docker compose logs api postgres rabbitmq redis
docker compose ps
curl -v http://localhost:8080/health/ready
```

Confirme migrations e conexão com o banco primário. O smoke Twilio não deve usar `clinicassistant_test`.

### ngrok não apresenta URL HTTPS

```bash
curl -sS http://localhost:4040/api/tunnels
docker compose logs ngrok
```

Confirme `NGROK_AUTHTOKEN`, conectividade e se a porta `4040` está livre.
Se aparecer `ERR_NGROK_334`, encerre a sessão anterior no [dashboard do ngrok](https://dashboard.ngrok.com/endpoints) e reinicie o smoke. O script remove automaticamente o container ngrok local anterior, mas não pode encerrar uma sessão aberta em outro computador ou processo.

### Webhook retorna 401

Verifique:

- URL cadastrada exatamente, incluindo `IntegrationKey`;
- `TWILIO_AUTH_TOKEN` correto;
- URL HTTPS atual do ngrok;
- método `POST`;
- assinatura enviada pelo Twilio;
- configuração de proxies confiáveis, quando aplicável.

Nunca desabilite a validação de assinatura apenas para fazer o smoke passar.

### Webhook retorna 404

Confira se o caminho é exatamente:

```text
/api/webhooks/whatsapp/twilio/{integrationKey}
```

e não o formato legado `/api/webhooks/twilio/whatsapp/...`.

### Mensagem não aparece no frontend

Verifique, nesta ordem:

1. request no inspector do ngrok;
2. log da API;
3. validação de assinatura;
4. registro de Inbox/Outbox;
5. log do Worker;
6. RabbitMQ;
7. tenant e `IntegrationKey`;
8. query de conversas no frontend.

Use o `traceId` retornado nos `ProblemDetails` para correlacionar API e logs. Não registre Auth Token, telefone completo ou conteúdo sensível.

## 9. Encerrar o smoke

Depois da validação:

```bash
STOP_TWILIO_SMOKE=true ./scripts/local/stop.sh
rm -f .tmp/ngrok-url
```

Confirme que não existem containers do profile ativos:

```bash
docker compose --profile twilio-smoke ps
```

O smoke não altera automaticamente o Console Twilio nem envia novas mensagens durante o encerramento.

## 10. Critérios de aprovação

Considere o smoke aprovado somente quando:

- API e Worker estiverem saudáveis;
- ngrok fornecer URL HTTPS;
- webhook inbound retornar `200`;
- assinatura inválida retornar `401`;
- paciente e conversa forem resolvidos no tenant correto;
- mensagem inbound for persistida uma única vez;
- Worker processar a Outbox;
- StatusCallback atualizar o status permitido;
- nenhum segredo aparecer nos logs ou no Git.
