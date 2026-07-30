# Etapa 6 — Integração com WhatsApp utilizando Twilio

Implemente a integração do SaaS de clínicas e consultórios com o WhatsApp utilizando o **Twilio como provedor inicial do MVP**.

O usuário já possui uma conta Twilio criada e configurada. Portanto, a implementação deverá priorizar a integração real com essa conta, sem depender inicialmente da configuração direta da Meta Cloud API.

Apesar disso, a arquitetura não poderá ficar acoplada ao Twilio.

O sistema deverá possuir uma abstração de gateway que permita futuramente adicionar outros provedores, como:

* Meta WhatsApp Cloud API;
* outros BSPs de WhatsApp;
* gateway simulado para desenvolvimento e testes.

O Twilio deverá existir apenas como implementação de infraestrutura.

---

# 1. Objetivo da etapa

Ao final desta etapa, o sistema deverá ser capaz de:

* receber mensagens enviadas por pacientes pelo WhatsApp;
* validar a autenticidade dos webhooks do Twilio;
* identificar a clínica responsável pela mensagem;
* impedir processamento duplicado;
* criar ou localizar o paciente;
* criar ou localizar uma conversa;
* armazenar o histórico das mensagens;
* publicar eventos no RabbitMQ;
* processar mensagens de maneira assíncrona;
* enviar mensagens de texto;
* enviar templates aprovados;
* receber atualizações de status;
* controlar retries;
* encaminhar falhas para uma dead-letter queue;
* manter isolamento entre tenants;
* registrar métricas, logs e traces;
* funcionar com gateway real ou simulado.

Fluxo principal esperado:

```text
Paciente envia mensagem pelo WhatsApp
        ↓
Twilio recebe a mensagem
        ↓
Twilio chama o webhook da aplicação
        ↓
API valida a assinatura X-Twilio-Signature
        ↓
API identifica a integração e o tenant
        ↓
API verifica idempotência pelo MessageSid
        ↓
API salva InboxMessage e OutboxMessage
        ↓
API retorna HTTP 200 rapidamente
        ↓
Outbox Worker publica evento no RabbitMQ
        ↓
Consumer processa a mensagem
        ↓
Paciente e conversa são localizados ou criados
        ↓
Mensagem é salva no histórico
        ↓
Motor de atendimento gera uma resposta
        ↓
Mensagem de saída é salva
        ↓
Outbox publica comando de envio
        ↓
Worker chama o Twilio
        ↓
Twilio envia a mensagem ao paciente
        ↓
Twilio envia atualizações de status
        ↓
Sistema atualiza a mensagem
```

Não executar chamadas à IA, consultas complexas à agenda ou envio de respostas diretamente dentro da requisição do webhook.

---

# 2. Estratégia da implementação

Dividir a Etapa 6 nas seguintes subetapas:

```text
6.1 Contratos e abstrações
6.2 Configurações e credenciais
6.3 Gateway simulado
6.4 Integração real com Twilio
6.5 Validação de webhook
6.6 Recebimento de mensagens
6.7 Inbox, Outbox e RabbitMQ
6.8 Pacientes e conversas
6.9 Envio assíncrono
6.10 Templates
6.11 Status das mensagens
6.12 Mídias
6.13 Retry e dead-letter
6.14 Segurança multi-tenant
6.15 Observabilidade
6.16 Testes
6.17 Documentação
```

A implementação deverá ocorrer em incrementos pequenos.

Não implementar toda a etapa de uma só vez.

---

# 3. Arquitetura do provedor

A aplicação deverá utilizar a seguinte arquitetura:

```text
Application
    ↓
IWhatsAppGateway
    ├── FakeWhatsAppGateway
    ├── TwilioWhatsAppGateway
    └── MetaWhatsAppGateway futuro
```

O projeto `Domain` não poderá depender do Twilio.

O projeto `Application` deverá conhecer somente interfaces e contratos internos.

O SDK ou cliente HTTP do Twilio deverá existir apenas no projeto:

```text
ClinicAssistant.Infrastructure
```

A API será responsável por receber os webhooks.

O Worker será responsável por processar mensagens de entrada e saída.

---

# 4. Interface do gateway

Criar uma interface independente de provedor:

```csharp
public interface IWhatsAppGateway
{
    Task<SendWhatsAppMessageResult> SendTextAsync(
        SendWhatsAppTextRequest request,
        CancellationToken cancellationToken);

    Task<SendWhatsAppMessageResult> SendTemplateAsync(
        SendWhatsAppTemplateRequest request,
        CancellationToken cancellationToken);

    Task<SendWhatsAppMessageResult> SendMediaAsync(
        SendWhatsAppMediaRequest request,
        CancellationToken cancellationToken);
}
```

Criar os contratos:

```csharp
public sealed record SendWhatsAppTextRequest(
    Guid TenantId,
    Guid IntegrationId,
    Guid ConversationId,
    Guid ConversationMessageId,
    string RecipientPhone,
    string Text,
    string IdempotencyKey,
    string? CorrelationId);

public sealed record SendWhatsAppTemplateRequest(
    Guid TenantId,
    Guid IntegrationId,
    Guid ConversationId,
    Guid ConversationMessageId,
    string RecipientPhone,
    string ContentSid,
    IReadOnlyDictionary<string, string> Variables,
    string IdempotencyKey,
    string? CorrelationId);

public sealed record SendWhatsAppMediaRequest(
    Guid TenantId,
    Guid IntegrationId,
    Guid ConversationId,
    Guid ConversationMessageId,
    string RecipientPhone,
    string MediaUrl,
    string? Caption,
    string IdempotencyKey,
    string? CorrelationId);

public sealed record SendWhatsAppMessageResult(
    bool Success,
    string? ExternalMessageId,
    string? ProviderStatus,
    WhatsAppFailure? Failure);
```

Criar também:

```csharp
public sealed record WhatsAppFailure(
    WhatsAppFailureType Type,
    string? ProviderCode,
    string SafeMessage,
    bool CanRetry);
```

Tipos de falha:

```csharp
public enum WhatsAppFailureType
{
    Unknown = 0,
    Transient = 1,
    Permanent = 2,
    Authentication = 3,
    RateLimit = 4,
    InvalidRecipient = 5,
    InvalidTemplate = 6,
    PolicyViolation = 7,
    IntegrationDisabled = 8
}
```

---

# 5. Seleção do provedor

Criar:

```csharp
public enum WhatsAppProvider
{
    Fake = 1,
    Twilio = 2,
    Meta = 3
}
```

O valor `Meta` será reservado para implementação futura.

Criar configuração global:

```csharp
public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    public WhatsAppProvider Provider { get; init; }

    public int MaximumRetryAttempts { get; init; } = 3;

    public int RequestTimeoutSeconds { get; init; } = 15;

    public int RawPayloadRetentionDays { get; init; } = 30;

    public int MaxWebhookBodySizeBytes { get; init; } = 1_048_576;
}
```

A implementação utilizada deverá ser selecionada por configuração e injeção de dependência.

Exemplo:

```text
WHATSAPP__PROVIDER=Twilio
```

---

# 6. Configuração do Twilio

Criar opções tipadas:

```csharp
public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; init; } = string.Empty;

    public string AuthToken { get; init; } = string.Empty;

    public string WhatsAppFrom { get; init; } = string.Empty;

    public string? MessagingServiceSid { get; init; }

    public string BaseUrl { get; init; } =
        "https://api.twilio.com";

    public string? IncomingWebhookBaseUrl { get; init; }

    public string? StatusCallbackBaseUrl { get; init; }

    public int RequestTimeoutSeconds { get; init; } = 15;
}
```

Adicionar ao `.env.example`:

```env
WHATSAPP__PROVIDER=Twilio
WHATSAPP__MAXIMUM_RETRY_ATTEMPTS=3
WHATSAPP__REQUEST_TIMEOUT_SECONDS=15
WHATSAPP__RAW_PAYLOAD_RETENTION_DAYS=30
WHATSAPP__MAX_WEBHOOK_BODY_SIZE_BYTES=1048576

TWILIO__ACCOUNT_SID=
TWILIO__AUTH_TOKEN=
TWILIO__WHATSAPP_FROM=whatsapp:+5500000000000
TWILIO__MESSAGING_SERVICE_SID=
TWILIO__INCOMING_WEBHOOK_BASE_URL=
TWILIO__STATUS_CALLBACK_BASE_URL=
TWILIO__REQUEST_TIMEOUT_SECONDS=15
```

Nunca:

* versionar credenciais;
* registrar `AuthToken`;
* expor credenciais em endpoints;
* armazenar credenciais diretamente no frontend;
* retornar credenciais em respostas administrativas.

---

# 7. Integração por tenant

Criar ou evoluir a entidade:

```text
WhatsAppIntegration
- Id
- TenantId
- Provider
- IntegrationKey
- AccountSidReference
- MessagingServiceSid
- WhatsAppFrom
- DisplayPhoneNumber
- Status
- ConnectedAt
- LastValidatedAt
- LastWebhookAt
- LastSuccessfulSendAt
- LastFailureAt
- FailureReason
- CreatedAt
- UpdatedAt
```

Status:

```text
Pending
Connected
Disconnected
InvalidCredentials
Suspended
Disabled
```

O `IntegrationKey` deverá ser:

* público;
* aleatório;
* não previsível;
* diferente do `TenantId`;
* único;
* utilizado na URL do webhook.

Exemplo:

```text
/api/webhooks/whatsapp/twilio/wha_a7d33cf9948c41b1
```

Para o MVP, as credenciais principais do Twilio poderão ser compartilhadas pela plataforma e armazenadas em variáveis de ambiente.

A modelagem deve permitir futuramente:

* uma subconta Twilio por clínica;
* um sender diferente por tenant;
* credenciais individuais;
* onboarding de novos números.

---

# 8. Gateway simulado

Criar:

```csharp
public sealed class FakeWhatsAppGateway : IWhatsAppGateway
```

O gateway deverá:

* gerar um identificador externo fictício;
* simular envio de texto;
* simular templates;
* simular mídias;
* permitir atraso artificial;
* permitir falha transitória;
* permitir falha definitiva;
* permitir timeout;
* registrar logs sem dados sensíveis.

Configurações:

```env
WHATSAPP__FAKE__DELAY_MILLISECONDS=100
WHATSAPP__FAKE__FAILURE_MODE=None
WHATSAPP__FAKE__FAILURE_RATE=0
```

Utilizar o gateway simulado em:

* testes automatizados;
* CI;
* desenvolvimento local sem Twilio;
* demonstrações isoladas.

---

# 9. Implementação real do Twilio

Criar:

```csharp
public sealed class TwilioWhatsAppGateway : IWhatsAppGateway
```

Preferencialmente, encapsular o SDK ou cliente HTTP do Twilio em uma interface interna:

```csharp
public interface ITwilioMessageClient
{
    Task<TwilioMessageResult> SendTextAsync(
        TwilioSendTextRequest request,
        CancellationToken cancellationToken);

    Task<TwilioMessageResult> SendTemplateAsync(
        TwilioSendTemplateRequest request,
        CancellationToken cancellationToken);

    Task<TwilioMessageResult> SendMediaAsync(
        TwilioSendMediaRequest request,
        CancellationToken cancellationToken);
}
```

Isso facilitará testes sem chamadas reais.

Responsabilidades do `TwilioWhatsAppGateway`:

```text
1. Validar integração
2. Normalizar destinatário
3. Garantir prefixo whatsapp:
4. Resolver remetente
5. Montar requisição Twilio
6. Configurar StatusCallback
7. Enviar mensagem
8. Interpretar resposta
9. Retornar MessageSid
10. Classificar erros
11. Não expor detalhes sensíveis
```

O gateway não deve:

* atualizar diretamente entidades;
* criar conversas;
* criar pacientes;
* publicar eventos;
* decidir retry;
* acessar controller;
* conter regras de agendamento.

---

# 10. Normalização dos números

No Twilio, os telefones deverão ser enviados no formato:

```text
whatsapp:+5581999999999
```

Criar serviço:

```csharp
public interface IWhatsAppPhoneNumberFormatter
{
    string FormatForProvider(string phoneNumber);
}
```

Também criar serviço de normalização:

```csharp
public interface IPhoneNumberNormalizer
{
    PhoneNumberNormalizationResult Normalize(
        string rawPhone,
        string defaultCountryCode);
}
```

Persistir números preferencialmente em E.164:

```text
+5581999999999
```

Adicionar o prefixo `whatsapp:` somente na integração com o provedor.

Não armazenar o prefixo `whatsapp:` como parte do telefone principal do paciente.

---

# 11. Webhook de mensagens recebidas

Criar endpoint:

```text
POST /api/webhooks/whatsapp/twilio/{integrationKey}
```

O Twilio normalmente envia o payload como:

```text
application/x-www-form-urlencoded
```

Campos relevantes:

```text
MessageSid
SmsMessageSid
AccountSid
MessagingServiceSid
From
To
Body
NumMedia
MediaUrl0
MediaContentType0
ProfileName
WaId
ButtonText
ButtonPayload
Latitude
Longitude
Address
```

O endpoint deverá:

```text
1. Localizar integração pelo integrationKey
2. Validar se a integração está ativa
3. Capturar a URL pública completa da requisição
4. Capturar os parâmetros do formulário
5. Validar X-Twilio-Signature
6. Obter MessageSid
7. Verificar duplicidade
8. Persistir InboxMessage
9. Criar OutboxMessage
10. Confirmar transação
11. Retornar HTTP 200 rapidamente
```

Não processar a conversa diretamente no controller.

---

# 12. Validação da assinatura do Twilio

Criar:

```csharp
public interface ITwilioWebhookSignatureValidator
{
    bool IsValid(
        string requestUrl,
        IReadOnlyDictionary<string, string> parameters,
        string? signature);
}
```

A assinatura deverá ser obtida do header:

```text
X-Twilio-Signature
```

Requisitos:

* utilizar o mecanismo oficial de validação do Twilio;
* usar a URL pública exata;
* considerar proxies e load balancers;
* configurar corretamente forwarded headers;
* não confiar cegamente em headers enviados pelo cliente;
* rejeitar assinatura inválida;
* não processar o payload em caso de falha;
* não registrar o token;
* não registrar o payload integral.

Em caso de assinatura inválida:

```text
HTTP 401 ou HTTP 403
```

Criar métrica:

```text
twilio_webhook_invalid_signature_total
```

---

# 13. URL pública e proxies

A assinatura do Twilio depende da URL completa usada para chamar o webhook.

Configurar:

```csharp
ForwardedHeadersMiddleware
```

Tratar corretamente:

```text
X-Forwarded-Proto
X-Forwarded-Host
X-Forwarded-For
```

Utilizar a URL pública configurada quando o ambiente estiver atrás de proxy.

Não utilizar incorretamente:

```text
http://localhost
```

quando o Twilio chamou uma URL HTTPS pública.

Documentar essa configuração, pois divergências na URL podem causar assinatura inválida.

---

# 14. Entidade InboxMessage

Criar ou evoluir:

```text
InboxMessage
- Id
- TenantId
- IntegrationId
- Provider
- EventType
- ExternalMessageId
- ExternalEventId
- PayloadHash
- RawPayload
- Status
- ReceivedAt
- QueuedAt
- ProcessingStartedAt
- ProcessedAt
- RetryCount
- LastErrorCode
- LastErrorMessage
- CorrelationId
- CreatedAt
- UpdatedAt
```

Status:

```text
Received
Queued
Processing
Processed
Ignored
Failed
DeadLettered
Duplicate
```

O `MessageSid` deverá ser usado como `ExternalMessageId`.

Criar índice único:

```text
Provider + IntegrationId + ExternalMessageId
```

A garantia de idempotência deverá existir no PostgreSQL.

Não depender apenas de Redis ou memória.

---

# 15. Idempotência

Se o Twilio reenviar o mesmo webhook:

* a API deverá retornar HTTP 200;
* nenhuma nova conversa deverá ser criada;
* nenhuma mensagem deverá ser duplicada;
* nenhum novo evento deverá ser publicado;
* nenhuma nova resposta deverá ser enviada.

Fluxo:

```text
Receber MessageSid
        ↓
Tentar inserir InboxMessage
        ↓
Violação de índice único
        ↓
Tratar como duplicado
        ↓
Retornar HTTP 200
```

Criar métrica:

```text
twilio_webhook_duplicate_total
```

---

# 16. Transactional Inbox e Outbox

Ao receber uma mensagem:

```text
BEGIN TRANSACTION

INSERT InboxMessage
INSERT OutboxMessage

COMMIT
```

O OutboxMessage representará um evento interno:

```text
WhatsAppIncomingMessageReceived
```

Não publicar diretamente no RabbitMQ antes de confirmar o banco.

Criar worker de Outbox que:

```text
1. Busca mensagens pendentes
2. Bloqueia lote para processamento
3. Publica no RabbitMQ
4. Aguarda confirmação do broker
5. Marca como processado
6. Registra falha
7. Agenda próxima tentativa
```

Não marcar a mensagem como processada antes da confirmação do RabbitMQ.

---

# 17. Evento interno de entrada

Criar contrato:

```csharp
public sealed record WhatsAppIncomingMessageReceived(
    Guid TenantId,
    Guid IntegrationId,
    Guid InboxMessageId,
    string ExternalMessageId,
    string SenderPhone,
    string RecipientPhone,
    WhatsAppIncomingMessageType Type,
    string? Text,
    IReadOnlyCollection<WhatsAppIncomingMedia> Media,
    string? ProfileName,
    DateTimeOffset ReceivedAt,
    string CorrelationId);
```

Tipos:

```csharp
public enum WhatsAppIncomingMessageType
{
    Unknown = 0,
    Text = 1,
    Media = 2,
    Location = 3,
    Interactive = 4,
    Contact = 5
}
```

Mídia:

```csharp
public sealed record WhatsAppIncomingMedia(
    string Url,
    string? ContentType,
    int Index);
```

Não permitir que contratos específicos do Twilio atravessem os módulos internos.

---

# 18. Parser do webhook

Criar:

```csharp
public interface ITwilioWhatsAppWebhookParser
{
    WhatsAppIncomingMessageReceived Parse(
        TwilioIncomingWebhook webhook,
        WhatsAppIntegration integration,
        Guid inboxMessageId,
        string correlationId);
}
```

Criar DTO de entrada:

```csharp
public sealed class TwilioIncomingWebhook
{
    public string? MessageSid { get; init; }
    public string? AccountSid { get; init; }
    public string? From { get; init; }
    public string? To { get; init; }
    public string? Body { get; init; }
    public string? ProfileName { get; init; }
    public string? WaId { get; init; }
    public int NumMedia { get; init; }
    public string? ButtonText { get; init; }
    public string? ButtonPayload { get; init; }
    public string? Latitude { get; init; }
    public string? Longitude { get; init; }
}
```

O parser deverá:

* remover prefixo `whatsapp:`;
* normalizar telefone;
* identificar tipo de mensagem;
* coletar mídias;
* tratar campos ausentes;
* rejeitar payload inválido;
* produzir contrato interno consistente.

---

# 19. RabbitMQ

Criar exchange:

```text
clinicassistant.whatsapp
```

Criar exchange de dead-letter:

```text
clinicassistant.deadletter
```

Routing keys:

```text
whatsapp.incoming
whatsapp.outgoing.text
whatsapp.outgoing.template
whatsapp.outgoing.media
whatsapp.status.changed
```

Filas:

```text
whatsapp.incoming
whatsapp.outgoing
whatsapp.status
whatsapp.deadletter
```

Configurações:

* mensagens persistentes;
* publisher confirms;
* acknowledgements manuais;
* prefetch configurável;
* dead-letter exchange;
* retry controlado;
* correlation ID;
* tenant ID;
* integration ID;
* trace context.

Não implementar retry infinito.

---

# 20. Processamento da mensagem recebida

Criar consumer:

```csharp
public sealed class WhatsAppIncomingMessageConsumer
```

Responsabilidades:

```text
1. Receber evento interno
2. Validar tenant
3. Validar integração
4. Validar idempotência de consumo
5. Normalizar telefone
6. Localizar ou criar paciente
7. Localizar ou criar conversa
8. Criar ConversationMessage
9. Atualizar LastMessageAt
10. Publicar ConversationMessageReceived
11. Confirmar consumo
```

O consumer não deverá chamar o Twilio.

---

# 21. Criação do paciente

Ao receber uma nova mensagem:

```text
1. Normalizar telefone
2. Buscar por TenantId + Phone
3. Reutilizar se existir
4. Criar cadastro mínimo se não existir
5. Atualizar LastContactAt
```

Cadastro mínimo:

```text
Patient
- Id
- TenantId
- Name
- Phone
- Source
- ConsentStatus
- FirstContactAt
- LastContactAt
- CreatedAt
- UpdatedAt
```

Usar `ProfileName` apenas como nome inicial, sem considerar que seja uma identidade verificada.

Criar índice único:

```text
TenantId + Phone
```

---

# 22. Criação da conversa

Ao receber mensagem:

```text
1. Procurar conversa aberta
2. Validar canal WhatsApp
3. Validar integração
4. Reutilizar conversa ativa
5. Criar conversa se necessário
6. Atualizar LastMessageAt
```

Entidade:

```text
Conversation
- Id
- TenantId
- PatientId
- Channel
- IntegrationId
- ExternalContactId
- Status
- AssignedUserId
- StartedAt
- LastMessageAt
- ClosedAt
- CreatedAt
- UpdatedAt
```

Status:

```text
Bot
WaitingHuman
Human
Closed
```

Evitar múltiplas conversas simultâneas desnecessárias para o mesmo paciente e integração.

---

# 23. ConversationMessage

Criar ou evoluir:

```text
ConversationMessage
- Id
- TenantId
- ConversationId
- Direction
- Type
- Content
- ContentSanitized
- Provider
- ExternalMessageId
- ExternalReplyToMessageId
- Status
- ProviderStatus
- ProviderErrorCode
- ProviderErrorMessage
- QueuedAt
- AcceptedAt
- SentAt
- DeliveredAt
- ReadAt
- FailedAt
- ReceivedAt
- CreatedAt
- UpdatedAt
```

Direção:

```text
Inbound
Outbound
```

Tipos:

```text
Text
Template
Interactive
Image
Audio
Document
Location
Contact
System
```

Status:

```text
Pending
Queued
Accepted
Sent
Delivered
Read
Failed
Received
```

---

# 24. Envio assíncrono

Nenhum controller deverá enviar diretamente pelo Twilio.

Fluxo obrigatório:

```text
Aplicação decide responder
        ↓
Cria ConversationMessage como Pending
        ↓
Cria OutboxMessage
        ↓
Commit
        ↓
Outbox publica comando
        ↓
Worker consome
        ↓
TwilioWhatsAppGateway envia
        ↓
MessageSid é salvo
        ↓
Status é atualizado
```

Criar comando:

```csharp
public sealed record SendWhatsAppMessageCommand(
    Guid TenantId,
    Guid IntegrationId,
    Guid ConversationId,
    Guid ConversationMessageId,
    WhatsAppOutgoingMessageType Type,
    string RecipientPhone,
    string? Text,
    string? ContentSid,
    IReadOnlyDictionary<string, string>? ContentVariables,
    string? MediaUrl,
    string IdempotencyKey,
    string CorrelationId);
```

Criar consumer:

```csharp
public sealed class SendWhatsAppMessageConsumer
```

O consumer deverá:

```text
1. Buscar a mensagem
2. Verificar tenant
3. Verificar integração
4. Verificar se já foi enviada
5. Resolver gateway
6. Chamar Twilio
7. Salvar MessageSid
8. Atualizar status
9. Tratar falha
10. Confirmar ou rejeitar mensagem
```

---

# 25. Envio de texto pelo Twilio

Implementar envio contendo:

```text
From = whatsapp:+55...
To = whatsapp:+55...
Body = conteúdo
StatusCallback = URL pública
```

O sistema deverá garantir que:

* o remetente possua prefixo `whatsapp:`;
* o destinatário possua prefixo `whatsapp:`;
* o telefone esteja em E.164;
* a mensagem não esteja vazia;
* o tamanho máximo seja validado;
* o `CancellationToken` seja propagado.

Persistir o `MessageSid` retornado pelo Twilio.

---

# 26. Templates com ContentSid

Para mensagens que exigirem template aprovado, utilizar:

```text
ContentSid
ContentVariables
```

Criar entidade:

```text
WhatsAppTemplate
- Id
- TenantId
- IntegrationId
- Provider
- ExternalTemplateId
- ContentSid
- Name
- LanguageCode
- Category
- Status
- ParametersSchema
- CreatedAt
- UpdatedAt
```

Status:

```text
Draft
PendingApproval
Approved
Rejected
Paused
Disabled
```

Templates iniciais:

```text
appointment_confirmation
appointment_reminder_24h
appointment_reminder_2h
appointment_rescheduled
appointment_cancelled
appointment_confirmation_request
human_handoff_notice
```

Não permitir envio de template:

* inexistente;
* rejeitado;
* desabilitado;
* pertencente a outro tenant;
* com variáveis inválidas.

Exemplo de variáveis:

```json
{
  "1": "Felipe",
  "2": "Dra. Ana",
  "3": "31/07/2026",
  "4": "14:30",
  "5": "Unidade Boa Viagem"
}
```

---

# 27. Janela de atendimento

Criar serviço:

```csharp
public interface IWhatsAppConversationWindowPolicy
{
    WhatsAppConversationWindowResult Evaluate(
        DateTimeOffset? lastInboundMessageAt,
        DateTimeOffset currentTime);
}
```

O sistema deverá distinguir:

* mensagem livre permitida;
* template necessário;
* conversa sem histórico válido;
* janela expirada.

Não deixar essa decisão espalhada pelos consumers.

Quando a janela estiver expirada:

* não enviar texto livre;
* selecionar template aprovado;
* registrar a decisão;
* falhar de forma segura se não houver template.

---

# 28. StatusCallback

Criar endpoint:

```text
POST /api/webhooks/whatsapp/twilio/status/{integrationKey}
```

Campos relevantes:

```text
MessageSid
MessageStatus
ErrorCode
ErrorMessage
To
From
AccountSid
ChannelStatusMessage
```

Fluxo:

```text
1. Localizar integração
2. Validar X-Twilio-Signature
3. Localizar mensagem por MessageSid
4. Validar tenant
5. Mapear status
6. Atualizar mensagem
7. Registrar falha quando aplicável
8. Retornar HTTP 200
```

Mapeamento:

```text
queued       → Queued
accepted     → Accepted
sending      → Queued
sent         → Sent
delivered    → Delivered
read         → Read
failed       → Failed
undelivered  → Failed
canceled     → Failed
```

---

# 29. Precedência dos status

Criar uma ordem de precedência.

Exemplo:

```text
Pending
Queued
Accepted
Sent
Delivered
Read
```

Falha deverá ser tratada separadamente.

Não permitir regressões:

```text
Read → Delivered
Delivered → Sent
Sent → Queued
```

Criar serviço:

```csharp
public interface IMessageStatusTransitionPolicy
{
    bool CanTransition(
        ConversationMessageStatus current,
        ConversationMessageStatus next);
}
```

Criar testes para todas as transições.

---

# 30. Falhas do Twilio

Criar mapeamento dos erros do provedor para categorias internas.

Exemplos:

```text
Autenticação inválida
Remetente inválido
Destinatário inválido
Template inválido
Parâmetros inválidos
Número não aprovado
Rate limit
Falha temporária
Timeout
Erro interno do provedor
```

Não retornar a mensagem bruta do Twilio ao paciente.

Armazenar apenas erro sanitizado.

Definir:

```text
CanRetry = true
```

somente para falhas transitórias.

Não repetir automaticamente:

* credencial inválida;
* template inválido;
* destinatário inválido;
* sender não aprovado;
* erro de autorização;
* payload inválido.

---

# 31. Retry

O retry deverá ocorrer no processamento assíncrono, não dentro do controller.

Política sugerida:

```text
Tentativa 1: imediata
Tentativa 2: após 30 segundos
Tentativa 3: após 2 minutos
Tentativa 4: após 10 minutos
```

O limite final deverá ser configurável.

Utilizar:

* backoff;
* jitter;
* classificação de erro;
* limite de tentativas;
* `NextAttemptAt`;
* dead-letter após esgotamento.

Não realizar retry infinito.

---

# 32. Dead-letter queue

Mensagens deverão ser enviadas para DLQ quando:

* excederem tentativas;
* apresentarem payload inválido;
* estiverem sem tenant;
* estiverem sem integração;
* apresentarem falha permanente;
* causarem exceções repetidas;
* não puderem ser desserializadas;
* violarem isolamento multi-tenant.

A mensagem da DLQ deverá conter:

```text
OriginalMessageId
TenantId quando disponível
IntegrationId quando disponível
ConversationMessageId
RoutingKey
RetryCount
FirstFailureAt
LastFailureAt
SafeError
CorrelationId
TraceId
```

Nunca incluir:

* AuthToken;
* AccountSid completo quando desnecessário;
* conteúdo integral sensível;
* dados clínicos;
* segredo de webhook.

---

# 33. Mídias

Suportar inicialmente o recebimento de:

```text
Image
Audio
Document
Location
```

Na primeira versão:

* armazenar metadados;
* não baixar automaticamente arquivos grandes;
* validar tipo;
* validar tamanho;
* encaminhar conteúdo não suportado para atendimento humano;
* permitir processamento assíncrono.

Criar política:

```csharp
public interface IWhatsAppMediaPolicy
{
    WhatsAppMediaPolicyResult Evaluate(
        string? contentType,
        long? contentLength);
}
```

Configurações:

```env
WHATSAPP__MEDIA__MAX_FILE_SIZE_BYTES=10485760
WHATSAPP__MEDIA__ALLOWED_TYPES=image/jpeg,image/png,application/pdf,audio/ogg
```

Não assumir que a URL de mídia ficará disponível indefinidamente.

---

# 34. Endpoint de simulação local

Somente em `Development`, criar:

```text
POST /api/dev/whatsapp/simulate-incoming
```

Payload:

```json
{
  "integrationKey": "wha_dev_clinic_01",
  "senderPhone": "+5581999999999",
  "profileName": "Paciente Teste",
  "type": "text",
  "text": "Quero marcar uma consulta"
}
```

Esse endpoint deverá passar pelo pipeline normal:

```text
Inbox
Outbox
RabbitMQ
Consumer
Patient
Conversation
ConversationMessage
```

Não criar um fluxo paralelo que ignore a arquitetura real.

O endpoint não deverá existir em produção.

---

# 35. Endpoint de mensagem de teste

Criar endpoint administrativo:

```text
POST /api/whatsapp/integration/test-message
```

Restrições:

* somente `ClinicAdmin`;
* rate limit;
* integração precisa estar ativa;
* telefone deve ser validado;
* envio deve ser auditado;
* processamento deve ser assíncrono;
* não chamar o Twilio diretamente no controller.

Payload:

```json
{
  "recipientPhone": "+5581999999999",
  "message": "Mensagem de teste da clínica."
}
```

---

# 36. Endpoints administrativos

Criar:

```text
GET    /api/whatsapp/integration
POST   /api/whatsapp/integration
PUT    /api/whatsapp/integration
DELETE /api/whatsapp/integration
POST   /api/whatsapp/integration/validate
POST   /api/whatsapp/integration/test-message
GET    /api/whatsapp/integration/status

GET    /api/whatsapp/templates
GET    /api/whatsapp/templates/{id}
POST   /api/whatsapp/templates
PUT    /api/whatsapp/templates/{id}
POST   /api/whatsapp/templates/synchronize
```

Não retornar:

* AuthToken;
* credenciais;
* segredos;
* payloads sensíveis.

Mascarar números quando apropriado.

---

# 37. Segurança multi-tenant

Toda operação deverá validar:

```text
TenantId
IntegrationId
ConversationId
PatientId
ConversationMessageId
TemplateId
```

Garantias obrigatórias:

* integração pertence ao tenant;
* conversa pertence ao tenant;
* paciente pertence ao tenant;
* mensagem pertence à conversa;
* template pertence à integração;
* webhook não aceita TenantId vindo do cliente;
* tenant é resolvido pelo IntegrationKey ou autenticação.

Criar testes de acesso cruzado.

---

# 38. Logs seguros

Criar logs estruturados contendo:

```text
TenantId
IntegrationId
ConversationId
ConversationMessageId
ExternalMessageId
CorrelationId
TraceId
Provider
MessageType
Status
FailureType
```

Não registrar:

* AuthToken;
* corpo completo de mensagens;
* telefone completo;
* conteúdo médico;
* URL de mídia com credenciais;
* dados pessoais desnecessários.

Mascarar telefone:

```text
+55******95348
```

---

# 39. Observabilidade

Instrumentar com OpenTelemetry:

```text
Webhook recebido
Assinatura validada
Inbox persistida
Outbox criada
Evento publicado
Mensagem consumida
Paciente localizado
Conversa localizada
Mensagem persistida
Envio iniciado
Twilio chamado
Twilio respondeu
Status atualizado
Retry agendado
Mensagem enviada para DLQ
```

Métricas:

```text
twilio_webhook_requests_total
twilio_webhook_invalid_signature_total
twilio_webhook_duplicate_total
twilio_webhook_duration
whatsapp_incoming_messages_total
whatsapp_outgoing_messages_total
whatsapp_send_success_total
whatsapp_send_failure_total
whatsapp_send_retry_total
whatsapp_status_updates_total
whatsapp_deadletter_total
whatsapp_provider_duration
whatsapp_templates_sent_total
whatsapp_media_received_total
```

Evitar usar telefone, MessageSid ou TenantId como labels de alta cardinalidade em métricas agregadas.

Esses valores podem existir em logs ou traces controlados.

---

# 40. Health checks

O readiness deverá continuar verificando:

* PostgreSQL;
* RabbitMQ;
* Redis.

Não chamar o Twilio em toda execução do health check.

Criar status operacional separado:

```text
GET /api/whatsapp/integration/status
```

Resposta sugerida:

```json
{
  "provider": "Twilio",
  "status": "Connected",
  "displayPhoneNumber": "+55******0000",
  "lastWebhookAt": "2026-07-30T14:00:00Z",
  "lastSuccessfulSendAt": "2026-07-30T14:01:00Z",
  "lastFailureAt": null,
  "failureReason": null
}
```

---

# 41. Testes unitários

Criar testes para:

* seleção do gateway;
* formatação `whatsapp:`;
* normalização de telefone;
* validação de assinatura;
* parsing de webhook;
* idempotência;
* classificação de erro;
* criação de paciente;
* reutilização de paciente;
* criação de conversa;
* reutilização de conversa;
* transição de status;
* prevenção de regressão;
* validação de template;
* janela de atendimento;
* mascaramento de telefone;
* proteção multi-tenant;
* falha simulada;
* timeout simulado;
* retry permitido;
* retry proibido.

---

# 42. Testes de integração

Utilizar PostgreSQL, RabbitMQ e Redis reais em containers de teste quando possível.

Cenários obrigatórios:

## Webhook válido

```text
Dado um webhook válido do Twilio
Quando for recebido
Então InboxMessage será criada
E OutboxMessage será criada
E HTTP 200 será retornado
```

## Assinatura inválida

```text
Dado X-Twilio-Signature inválido
Quando o webhook for recebido
Então HTTP 401 ou 403 será retornado
E nenhuma Inbox será criada
```

## Mensagem duplicada

```text
Dado o mesmo MessageSid
Quando o webhook for enviado duas vezes
Então apenas uma mensagem será processada
```

## Processamento assíncrono

```text
Dada InboxMessage pendente
Quando Outbox Worker publicar o evento
Então RabbitMQ receberá a mensagem
E o consumer criará Patient, Conversation e ConversationMessage
```

## Envio pelo fake gateway

```text
Dada uma mensagem de saída
Quando o worker processar
Então FakeWhatsAppGateway será chamado
E ExternalMessageId será persistido
```

## Envio pelo Twilio mockado

```text
Dada uma mensagem válida
Quando o gateway for chamado
Então o cliente Twilio receberá From, To, Body e StatusCallback corretos
```

## Status entregue

```text
Dada uma mensagem com status Sent
Quando o callback Delivered for recebido
Então a mensagem será atualizada para Delivered
```

## Regressão de status

```text
Dada uma mensagem Read
Quando chegar callback Delivered
Então o status deverá permanecer Read
```

## Falha transitória

```text
Dado timeout do provedor
Quando o worker processar
Então RetryCount será incrementado
E NextAttemptAt será definido
```

## Falha permanente

```text
Dado destinatário inválido
Quando o worker processar
Então não haverá retry infinito
E a mensagem será marcada como Failed
```

## DLQ

```text
Dado limite de tentativas excedido
Quando ocorrer nova falha
Então a mensagem será enviada para DLQ
```

## Isolamento

```text
Dada integração de outro tenant
Quando houver tentativa de acesso
Então a operação será rejeitada
```

---

# 43. Fixtures do Twilio

Criar:

```text
tests/Fixtures/Twilio/
```

Arquivos:

```text
incoming-text.form
incoming-media.form
incoming-location.form
incoming-button.form
status-queued.form
status-sent.form
status-delivered.form
status-read.form
status-failed.form
duplicate-message.form
invalid-message.form
```

Criar helpers para gerar assinatura válida durante testes.

Não depender da internet para executar a suíte.

---

# 44. Docker e ambiente local

O `docker-compose.yml` deverá continuar contendo:

```text
api
worker
postgres
rabbitmq
redis
frontend
```

Para receber webhooks reais em ambiente local, documentar o uso de:

```text
ngrok
ou
Cloudflare Tunnel
```

Exemplo:

```text
https://dominio-publico-temporario/api/webhooks/whatsapp/twilio/{integrationKey}
```

Configurar também:

```text
https://dominio-publico-temporario/api/webhooks/whatsapp/twilio/status/{integrationKey}
```

Não colocar URLs temporárias diretamente no código.

---

# 45. Configuração no painel Twilio

Criar documentação explicando como configurar:

```text
1. Identificar o WhatsApp Sender
2. Acessar configuração do número
3. Configurar o endpoint de mensagens recebidas
4. Selecionar método HTTP POST
5. Configurar StatusCallback
6. Confirmar AuthToken usado na assinatura
7. Validar prefixo whatsapp:
8. Enviar primeira mensagem
9. Confirmar recebimento no webhook
10. Confirmar retorno da aplicação
```

Documentar separadamente:

* Sandbox;
* sender aprovado;
* templates;
* ContentSid;
* mensagens dentro da janela;
* mensagens fora da janela.

---

# 46. Documentação obrigatória

Criar:

```text
docs/whatsapp/overview.md
docs/whatsapp/twilio-setup.md
docs/whatsapp/twilio-webhooks.md
docs/whatsapp/twilio-signature.md
docs/whatsapp/templates.md
docs/whatsapp/testing.md
docs/whatsapp/security.md
docs/whatsapp/troubleshooting.md
docs/whatsapp/provider-migration.md
```

`provider-migration.md` deverá explicar como adicionar futuramente:

```text
MetaWhatsAppGateway
```

sem alterar as regras de domínio.

---

# 47. Critérios de aceite

A Etapa 6 será considerada concluída quando:

```text
1. IWhatsAppGateway estiver desacoplado
2. FakeWhatsAppGateway funcionar
3. TwilioWhatsAppGateway funcionar
4. Provider for selecionável por configuração
5. Webhook de entrada estiver implementado
6. StatusCallback estiver implementado
7. X-Twilio-Signature for validado
8. MessageSid garantir idempotência
9. Inbox e Outbox estiverem transacionais
10. RabbitMQ processar eventos
11. Paciente for criado ou localizado
12. Conversa for criada ou localizada
13. Mensagem recebida for persistida
14. Mensagem de saída for processada assincronamente
15. Textos puderem ser enviados
16. Templates com ContentSid puderem ser enviados
17. Mídias básicas forem identificadas
18. MessageSid de saída for persistido
19. Status forem atualizados
20. Regressão de status for impedida
21. Retry controlado funcionar
22. Falhas permanentes não forem repetidas
23. DLQ funcionar
24. Multi-tenancy estiver protegido
25. Credenciais não aparecerem em logs
26. Métricas e tracing estiverem ativos
27. Testes unitários passarem
28. Testes de integração passarem
29. Docker Compose continuar funcional
30. Documentação estiver atualizada
```

---

# 48. Ordem de execução

Executar na seguinte ordem:

## 6.1 Contratos e entidades

* criar interfaces;
* criar records;
* criar enums;
* criar WhatsAppIntegration;
* criar WhatsAppTemplate;
* evoluir InboxMessage;
* evoluir ConversationMessage;
* criar migrations.

## 6.2 Gateway simulado

* implementar FakeWhatsAppGateway;
* configurar falhas;
* criar testes;
* criar endpoint de simulação local.

## 6.3 Infraestrutura Twilio

* adicionar dependências necessárias;
* criar TwilioOptions;
* criar cliente encapsulado;
* criar TwilioWhatsAppGateway;
* criar testes mockados.

## 6.4 Webhook de entrada

* criar endpoint;
* configurar formulário;
* validar assinatura;
* localizar integração;
* implementar idempotência;
* criar Inbox e Outbox;
* retornar rapidamente.

## 6.5 Mensageria

* criar exchanges;
* criar filas;
* publicar pela Outbox;
* implementar consumer;
* configurar DLQ.

## 6.6 Conversas

* criar ou localizar paciente;
* criar ou localizar conversa;
* persistir mensagem;
* publicar evento interno.

## 6.7 Envio

* criar comando;
* criar consumer;
* chamar gateway;
* atualizar ExternalMessageId;
* tratar falhas.

## 6.8 Templates

* implementar ContentSid;
* validar ContentVariables;
* validar status;
* implementar janela de atendimento.

## 6.9 Status

* criar StatusCallback;
* validar assinatura;
* mapear status;
* impedir regressão.

## 6.10 Mídia

* identificar mídia;
* validar tipo;
* validar tamanho;
* preparar processamento assíncrono.

## 6.11 Qualidade

* segurança;
* multi-tenancy;
* observabilidade;
* testes;
* documentação;
* revisão de logs;
* revisão de segredos.

---

# 49. Primeira instrução ao Codex

Antes de alterar o código:

1. analise a arquitetura atual;
2. identifique projetos e módulos impactados;
3. liste os pacotes necessários;
4. apresente os arquivos que serão criados;
5. apresente as migrations;
6. explique como o Twilio ficará isolado na infraestrutura;
7. explique como Inbox, Outbox e RabbitMQ manterão consistência;
8. explique como a assinatura será validada atrás de proxy;
9. identifique riscos;
10. não faça alterações até concluir essa análise.

Depois da análise, implemente inicialmente apenas:

```text
Subetapa 6.1 — Contratos e entidades
Subetapa 6.2 — Gateway simulado
Subetapa 6.3 — Infraestrutura básica do Twilio
```

A primeira entrega deverá conter:

```text
1. IWhatsAppGateway
2. Contratos internos
3. Enums
4. WhatsAppOptions
5. TwilioOptions
6. WhatsAppIntegration
7. WhatsAppTemplate
8. Evolução de InboxMessage
9. Evolução de ConversationMessage
10. FakeWhatsAppGateway
11. ITwilioMessageClient
12. TwilioWhatsAppGateway
13. Configuração de DI
14. Migrations
15. Testes unitários
16. .env.example
17. Documentação inicial
```

Após implementar:

```text
dotnet restore
dotnet build
dotnet test
```

Corrija todos os erros encontrados.

Não avance para os webhooks reais enquanto:

* a solução não compilar;
* os testes não passarem;
* o gateway simulado não estiver funcional;
* o cliente Twilio não estiver coberto por testes;
* nenhuma credencial estiver exposta.
