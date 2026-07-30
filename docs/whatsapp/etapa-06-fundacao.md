# Etapa 6 — Fundação WhatsApp/Twilio

Este documento registra a evolução da Etapa 6: contratos e entidades, gateway simulado, infraestrutura Twilio, webhook, mensageria e processamento de entrada.

## Configuração local

O padrão é `WHATSAPP__PROVIDER=Fake`. O gateway falso não chama serviços externos e pode simular atraso, erro transitório, erro permanente ou timeout usando as variáveis `WHATSAPP__FAKE__*` documentadas em `.env.example`.

Para selecionar Twilio, configure `WHATSAPP__PROVIDER=Twilio`, `TWILIO__ACCOUNT_SID`, `TWILIO__AUTH_TOKEN` e `TWILIO__WHATSAPP_FROM`. Credenciais nunca devem ser versionadas ou registradas em logs. Telefones persistidos permanecem em E.164; o prefixo `whatsapp:` é incluído somente no gateway.

## Limites desta entrega

IA, download/processamento de arquivos de mídia e onboarding de clínicas continuam fora do escopo desta etapa. O RabbitMQ e o Outbox permanecem como mecanismos transacionais para os fluxos de entrada e saída.

Integrações Meta anteriores são migradas para o status `Disabled`, pois não possuem credenciais/campos equivalentes ao Twilio. A reativação ocorrerá apenas por onboarding futuro.

## Subetapa 6.4 — webhook de entrada

O endpoint público é `POST /api/webhooks/whatsapp/twilio/{integrationKey}` e aceita somente `application/x-www-form-urlencoded`. Ele localiza uma integração Twilio conectada, valida `X-Twilio-Signature` com o `RequestValidator` oficial, persiste `InboxMessage` e `OutboxMessage` na mesma transação e retorna imediatamente. Reenvios com o mesmo `MessageSid` retornam `200` sem duplicar dados.

Configure `TWILIO__INCOMING_WEBHOOK_BASE_URL` com a origem HTTPS pública, por exemplo `https://api.example.com`. Essa origem é usada para reconstruir a URL assinada. Quando depender de headers encaminhados, informe apenas os endereços IP dos proxies confiáveis em `TWILIO__TRUSTED_PROXY_ADDRESSES__0` (e índices seguintes); a aplicação não confia em headers de qualquer origem.

Esta subetapa ainda não consome a Outbox, não cria pacientes/conversas e não executa `StatusCallback`.

## Subetapa 6.5 — mensageria

O Worker publica o contrato `WhatsAppIncomingMessageReceived` pela Outbox no exchange `clinicassistant.whatsapp`, com a routing key `whatsapp.incoming`. A publicação usa mensagens persistentes e publisher confirms; somente após a confirmação a Outbox é marcada como processada. Falhas usam backoff de 30 segundos, 2 minutos e 10 minutos e, após o limite configurado, geram um envelope seguro na fila `whatsapp.deadletter`.

## Subetapa 6.6 — processamento de entrada

O consumer manual de `whatsapp.incoming` valida tenant e integração a partir do contrato interno, localiza ou cria o paciente por `TenantId + Phone`, reutiliza uma conversa WhatsApp aberta e persiste a mensagem recebida. A criação é transacional e publica `ConversationMessageReceived` pela Outbox na fila `whatsapp.conversation`. Nenhuma chamada ao Twilio ou IA ocorre no consumer.

## Subetapa 6.7 — envio assíncrono

O contrato `SendWhatsAppMessageCommand` e o consumer manual de `whatsapp.outgoing` estabelecem o envio de texto. O processor valida tenant, integração, conversa e mensagem de saída antes de resolver o gateway. Em caso de aceite, o `MessageSid` e o status `Accepted` são persistidos; em caso de falha, somente o erro sanitizado é registrado. Não há envio direto por controller, `StatusCallback`, template ou mídia nesta subetapa.

## Subetapa 6.8 — templates e janela de atendimento

Templates usam `ContentSid` e somente são enviados quando pertencem ao tenant e à integração corretos, estão no status `Approved` e suas variáveis correspondem ao `ParametersSchema`. O schema atual é um array JSON de chaves obrigatórias, por exemplo `["1","2"]`.

A política centralizada aplica a janela de 24 horas desde a última mensagem de entrada: dentro da janela, texto livre é permitido; fora dela, ou sem histórico de entrada, o envio de texto falha de forma segura e exige um template aprovado. Endpoints administrativos de templates e `StatusCallback` continuam fora desta entrega.

## Subetapa 6.9 — StatusCallback

O endpoint `POST /api/webhooks/whatsapp/twilio/status/{integrationKey}` valida a assinatura Twilio e atualiza a mensagem identificada pelo `MessageSid`. Os status `queued`, `accepted`, `sending`, `sent`, `delivered`, `read`, `failed`, `undelivered` e `canceled` são mapeados para o modelo interno.

A política de precedência impede regressões, como `Read → Delivered` ou `Delivered → Sent`. Erros do callback não são persistidos literalmente: somente um texto sanitizado é armazenado quando o status é de falha.

## Subetapa 6.10 — mídia

Mídias de entrada agora têm os metadados persistidos em `whatsapp_media`, vinculados à mensagem de conversa e isolados por tenant. A primeira versão não baixa nem processa o arquivo: apenas armazena URL de origem, tipo, tamanho quando informado e o resultado da política. Assim, evita-se transferir arquivos grandes durante o webhook ou o consumer.

São aceitos por padrão `image/jpeg`, `image/png`, `application/pdf` e `audio/ogg`, configuráveis por `WHATSAPP__MEDIA__ALLOWED_TYPES`. O limite é `WHATSAPP__MEDIA__MAX_FILE_SIZE_BYTES` (10 MiB por padrão). Quando o tamanho não está disponível no webhook, o registro permanece em `PendingValidation`, pronto para uma validação assíncrona futura. Tipos não aceitos ou arquivos que excedem o limite colocam a conversa em `WaitingHuman`; o conteúdo não é baixado.

## Subetapa 6.11 — qualidade

O endpoint autenticado `GET /api/whatsapp/integration/status` retorna somente dados operacionais da integração do tenant corrente e mascara o número de telefone. Não expõe tokens, Account SID ou payloads. A telemetria centralizada inclui métricas de webhooks, duplicidade, assinatura inválida, mensagens de entrada e saída, resultado de envio e atualização de status; traces internos usam a source `ClinicAssistant.WhatsApp`.

Os guias operacionais, de segurança, provider e testes estão em `docs/whatsapp/`. A suíte inclui teste para mascaramento de telefone e os fluxos mantêm a validação explícita de tenant, integração, conversa e mensagem.
