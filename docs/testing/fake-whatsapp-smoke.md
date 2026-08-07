# Smoke Fake WhatsApp — Outbox e Worker

Este roteiro valida o item 1 da Etapa 9.5 sem Twilio, ngrok ou chamadas externas. O provider Fake gera um `SM_FAKE_*`, grava o estado da mensagem e permite acompanhar o fluxo assíncrono.

## 1. Pré-requisitos

- Docker Desktop iniciado;
- repositório na raiz do projeto;
- `.env` local sem ambiente `Production`;
- `WHATSAPP_PROVIDER=Fake`;
- `WhatsApp:TestRecipient` configurado para um número de teste permitido;
- usuário `ClinicAdmin` disponível (no seed E2E: `admin.e2e@fake.local`).

Não coloque senha, Auth Token ou connection string neste documento.

## 2. Subir o ambiente

```bash
./scripts/local/start-local.sh
./scripts/local/status.sh
./scripts/local/validate.sh
```

Confirme na saída:

- API em `http://localhost:8080`;
- frontend em `http://localhost:3000`;
- API pronta em `/health/ready`;
- provider `Fake`;
- containers `api`, `worker`, `postgres`, `redis` e `rabbitmq` em execução.

## 3. Solicitar a mensagem de teste

1. Abra `http://localhost:3000/login`.
2. Entre com um usuário `ClinicAdmin` do ambiente.
3. Acesse **Integrações → WhatsApp**.
4. Clique em **Validar configuração**.
5. Clique em **Habilitar** e aguarde o status **Conectada**.
6. Clique em **Enviar teste** uma única vez.

O endpoint retorna `202 Accepted`. Isso confirma somente a criação transacional; a entrega é assíncrona.

## 4. Confirmar o processamento

Em outro terminal, acompanhe o Worker:

```bash
./scripts/local/logs.sh worker
```

Procure por mensagens do gateway Fake indicando `Success: True` e por ausência de falhas no consumo RabbitMQ.

Também é possível consultar o estado no PostgreSQL sem exibir conteúdo da mensagem:

```bash
docker compose exec postgres psql -U "${DATABASE_USER:-clinicassistant}" -d "${DATABASE_NAME:-clinicassistant_test}" -c \
  'select "Status", "ProcessedAt", "RetryCount", "LastError" from "outbox_messages" order by "CreatedAt" desc limit 5;'

docker compose exec postgres psql -U "${DATABASE_USER:-clinicassistant}" -d "${DATABASE_NAME:-clinicassistant_test}" -c \
  'select "Direction", "Status", "ProviderStatus", "ExternalMessageId", "CreatedAt" from "conversation_messages" order by "CreatedAt" desc limit 5;'
```

Resultado esperado:

- Outbox com `Status = Processed` e `LastError` nulo;
- mensagem outbound com status `Accepted` (ou posterior, quando houver callback);
- `ProviderStatus = accepted`;
- `ExternalMessageId` iniciando por `SM_FAKE_`;
- integração com **Último envio** atualizado na tela.

## 5. Confirmar no frontend

Volte à tela WhatsApp e confirme **Último envio**. Em **Conversas**, localize a conversa do destinatário de teste e verifique a mensagem outbound e o status. A tela atualiza por SignalR e faz polling de fallback a cada 15 segundos.

## 6. Diagnóstico de falha

```bash
./scripts/local/logs.sh api
./scripts/local/logs.sh worker
docker compose ps
```

- Outbox pendente: verifique Worker, RabbitMQ e conectividade com PostgreSQL.
- `LastError` preenchido: corrija a causa e repita com uma nova chave de idempotência.
- status `Connected` ausente: valide a configuração e habilite a integração.
- erro HTTP: copie apenas o `traceId` exibido na tela e procure a requisição correspondente nos logs.

## 7. Limpeza

```bash
./scripts/local/stop.sh
```

Este roteiro não executa reset destrutivo nem envia mensagem real. Para o perfil E2E isolado, use o [guia de execução E2E](e2e-execution-guide.md).
