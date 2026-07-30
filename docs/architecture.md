# Arquitetura

O Clinic AI Assistant é iniciado como um monólito modular em .NET 10. A separação física é:

- `Domain`: regras de negócio puras e primitivas.
- `Application`: casos de uso e portas de entrada/saída.
- `Infrastructure`: EF Core, PostgreSQL e adaptadores de infraestrutura.
- `Api`: HTTP, Swagger, health checks, logging e telemetria.
- `Worker`: processamento assíncrono; os consumidores RabbitMQ entram na Etapa 5.
- `Contracts`: contratos de integração e DTOs públicos.

As dependências seguem o sentido `Api/Worker → Infrastructure/Application → Domain`. O domínio não depende de detalhes de infraestrutura.

## Observabilidade

A API publica traces e métricas por OpenTelemetry via OTLP quando `OTEL_EXPORTER_OTLP_ENDPOINT` é configurado. O logging estruturado é feito com Serilog em stdout. Dados pessoais e segredos não devem ser colocados em logs.

## Banco de dados

PostgreSQL é o sistema de registro. O contexto EF Core já está preparado, com schema `clinic_assistant`; as entidades e a primeira migration serão introduzidas na Etapa 2, junto à modelagem de tenancy.

## Tenancy e identidade

Cada usuário possui um `TenantId` e o JWT contém o claim `tenant_id`. O `HttpTenantContext` obtém esse claim exclusivamente do token validado. As entidades pertencentes a tenant recebem filtros globais do EF Core: por padrão, apenas registros do tenant autenticado são retornados. O único bypass é o papel `PlatformAdmin`, definido por policy e claim autenticado.

Refresh tokens são gerados aleatoriamente, persistidos apenas como SHA-256 e rotacionados em cada renovação. Senhas usam PBKDF2-HMAC-SHA512 com salt aleatório.
