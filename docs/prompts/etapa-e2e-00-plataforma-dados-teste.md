# Etapa E2E-00 — Plataforma de Dados de Teste e Seed Determinístico

## Contexto

Antes de executar testes fim a fim com Twilio, frontend, SignalR, filas, agenda e APIs administrativas, o sistema precisa possuir um conjunto de dados conhecido, reproduzível, seguro e isolado.

Esta etapa deverá criar uma plataforma de dados de teste baseada prioritariamente em scripts SQL versionados para PostgreSQL.

Os scripts deverão permitir:

* recriar os dados de teste;
* limpar os dados de teste;
* popular um ambiente mínimo;
* popular o ambiente E2E;
* criar usuários administrativos;
* criar tenants;
* criar profissionais;
* criar especialidades;
* criar unidades;
* criar pacientes fictícios;
* criar agendas e disponibilidades;
* criar consultas;
* criar conversas;
* criar mensagens;
* criar fila humana;
* criar integrações fake;
* criar integração Twilio desabilitada;
* criar templates;
* criar auditoria;
* manter os testes determinísticos.

A execução desta etapa é obrigatória antes dos testes E2E reais com Twilio.

---

## Objetivo

Implementar uma plataforma de dados de teste que permita inicializar ambientes completos e reproduzíveis por meio de scripts SQL.

Ao final, deverá ser possível executar comandos equivalentes a:

```bash
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh minimal
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

Também deverá ser possível executar pelo Docker Compose:

```bash
docker compose --profile e2e run --rm test-data-seeder e2e
```

---

## Princípios obrigatórios

* todos os dados devem ser fictícios;
* nenhum dado real de paciente pode ser utilizado;
* nenhum telefone real deve ser criado por padrão;
* nenhum email real deve ser criado;
* nenhuma credencial real deve ser inserida;
* nenhum token Twilio deve estar nos scripts;
* nenhum sender Twilio real deve ser ativado automaticamente;
* os scripts devem ser idempotentes;
* os scripts devem ser determinísticos;
* o mesmo perfil deve gerar os mesmos identificadores;
* os dados devem respeitar todas as foreign keys;
* os dados devem respeitar `TenantId`;
* os scripts devem falhar quando o schema esperado não existir;
* os scripts não podem executar em produção;
* deve existir proteção explícita contra execução em produção;
* migrations devem continuar sendo a fonte da estrutura do banco;
* scripts de seed não devem substituir migrations;
* não alterar manualmente tabelas de controle do EF Core;
* senhas devem ser armazenadas somente com hash compatível com a aplicação;
* os scripts devem ser compatíveis com PostgreSQL;
* o reset deve apagar somente dados do ambiente de teste.

---

## Estratégia

Separar responsabilidades:

```text
Migrations
    ↓
Estrutura do banco

Seed scripts
    ↓
Dados de referência e dados fictícios

Fixtures
    ↓
Dados específicos de testes

Reset scripts
    ↓
Limpeza controlada
```

Criar a estrutura:

```text
database/
├── seeds/
│   ├── common/
│   │   ├── 001_reference_data.sql
│   │   ├── 002_permissions.sql
│   │   ├── 003_roles.sql
│   │   └── 004_conversation_templates.sql
│   ├── minimal/
│   │   ├── 001_tenants.sql
│   │   ├── 002_users.sql
│   │   └── 003_basic_catalog.sql
│   └── e2e/
│       ├── 001_tenants.sql
│       ├── 002_users.sql
│       ├── 003_units.sql
│       ├── 004_specialties.sql
│       ├── 005_professionals.sql
│       ├── 006_patients.sql
│       ├── 007_schedules.sql
│       ├── 008_appointments.sql
│       ├── 009_conversations.sql
│       ├── 010_messages.sql
│       ├── 011_human_queue.sql
│       ├── 012_integrations.sql
│       ├── 013_whatsapp_templates.sql
│       └── 014_audit.sql
├── reset/
│   ├── reset_test_data.sql
│   ├── reset_tenant.sql
│   └── truncate_test_schema.sql
└── validation/
    ├── validate_seed.sql
    ├── validate_tenant_isolation.sql
    ├── validate_foreign_keys.sql
    └── validate_counts.sql

scripts/
└── test-data/
    ├── seed.sh
    ├── reset.sh
    ├── validate.sh
    ├── seed.ps1
    ├── reset.ps1
    └── validate.ps1
```

---

## Perfis

### Minimal

Criar:

```text
1 tenant
2 usuários
1 unidade
2 especialidades
2 profissionais
5 pacientes
10 slots
3 consultas
2 conversas
10 mensagens
1 integração fake
```

Objetivo:

* subir a aplicação;
* permitir login;
* permitir smoke test.

### E2E

Criar dados estáveis e determinísticos para testes automatizados.

Criar:

```text
2 tenants
6 usuários
3 unidades
8 especialidades
10 profissionais
30 pacientes
30 dias de agenda
50 consultas
20 conversas
200 mensagens
10 itens de fila
1 integração fake ativa
1 integração Twilio desabilitada
templates
auditoria
```

Não utilizar dados aleatórios nos fixtures principais.

---

## Identificadores determinísticos

Utilizar UUIDs fixos para os dados principais.

Exemplo:

```sql
-- Tenant principal E2E
'00000000-0000-0000-0000-000000000101'

-- Tenant isolado E2E
'00000000-0000-0000-0000-000000000102'

-- Admin E2E
'00000000-0000-0000-0000-000000000201'

-- Atendente E2E
'00000000-0000-0000-0000-000000000202'

-- Segundo atendente E2E
'00000000-0000-0000-0000-000000000203'

-- Paciente principal E2E
'00000000-0000-0000-0000-000000000301'

-- Conversa aguardando humano
'00000000-0000-0000-0000-000000000401'

-- Consulta reagendável
'00000000-0000-0000-0000-000000000501'
```

Documentar os IDs em:

```text
docs/testing/e2e-fixtures.md
```

---

## Tenants

Criar tenant principal:

```text
Nome: Clínica Saúde Mais E2E
Slug: clinica-saude-mais-e2e
Timezone: America/Recife
Locale: pt-BR
Status: Active
```

Criar tenant isolado:

```text
Nome: Clínica Isolada E2E
Slug: clinica-isolada-e2e
Timezone: America/Sao_Paulo
Locale: pt-BR
Status: Active
```

Nenhum usuário do primeiro tenant poderá acessar dados do segundo.

---

## Usuários

Criar usuários:

```text
admin.e2e@fake.local
manager.e2e@fake.local
reception.e2e@fake.local
operator.e2e@fake.local
operator2.e2e@fake.local
viewer.e2e@fake.local
```

Perfis:

```text
Administrator
Manager
Receptionist
Operator
Viewer
```

Requisitos:

* senha conhecida apenas em ambiente de teste;
* hash compatível com a aplicação;
* usuários ativos;
* tenant correto;
* permissões completas por perfil;
* nenhum email real;
* nenhuma senha em texto puro no banco.

Preferir:

```env
E2E_DEFAULT_PASSWORD=
```

O hash deve ser gerado pelo mecanismo oficial da aplicação.

Não criar algoritmo de hash incompatível diretamente em SQL.

---

## Unidades

Criar:

```text
Boa Viagem
Casa Forte
Olinda
```

Cada unidade deverá possuir:

* TenantId;
* nome;
* slug;
* timezone;
* endereço fictício;
* telefone fictício;
* status;
* horário de funcionamento.

---

## Especialidades

Criar:

```text
Clínico Geral
Cardiologia
Dermatologia
Pediatria
Ortopedia
Neurologia
Ginecologia
Oftalmologia
```

Requisitos:

* vinculadas ao tenant;
* ativas;
* nomes únicos por tenant.

---

## Profissionais

Criar profissionais fictícios, por exemplo:

```text
Dra. Ana Souza
Dr. Bruno Lima
Dra. Carla Mendes
Dr. Daniel Rocha
Dra. Elisa Barros
```

Cada profissional deverá possuir:

* TenantId;
* nome;
* registro claramente fictício;
* especialidades;
* unidades;
* status;
* duração padrão da consulta;
* horários de atendimento.

---

## Pacientes

Criar pacientes fictícios.

Campos:

* nome;
* telefone fictício;
* telefone normalizado;
* email fake;
* data de nascimento fictícia;
* status;
* origem;
* consentimento;
* data de criação.

Domínios permitidos:

```text
fake.local
example.test
invalid.local
```

Criar pacientes fixos:

```text
Paciente E2E Principal
Paciente E2E Secundário
Paciente Outro Tenant
```

Nenhum telefone fake deverá ser utilizado em envio real.

Quando a entidade suportar, marcar:

```text
IsTestData = true
```

---

## Agenda e disponibilidade

Criar agenda determinística.

Usar data-base configurável:

```env
E2E_BASE_DATE=2026-08-03
```

Persistir datas em UTC e respeitar o timezone do tenant.

Criar:

* slots livres;
* slots ocupados;
* bloqueios;
* intervalos;
* profissional sem agenda;
* slot concorrente;
* slot expirado;
* slot cancelado.

Criar IDs fixos para:

```text
slot_livre
slot_ocupado
slot_reagendamento
slot_concorrente
```

---

## Consultas

Criar consultas nos status:

```text
Pending
Confirmed
Rescheduled
Cancelled
Completed
NoShow
```

Cenários obrigatórios:

* consulta futura confirmada;
* consulta cancelável;
* consulta reagendável;
* consulta já cancelada;
* consulta concluída;
* consulta de outro tenant;
* conflito de slot;
* consulta vinculada à conversa.

---

## Conversas

Criar conversas nos estados:

```text
Automated
Paused
Human
Closed
WaitingPatient
WaitingHuman
```

Cenários fixos:

```text
conversation_automated
conversation_waiting_human
conversation_assigned_operator
conversation_paused
conversation_closed
conversation_reopenable
conversation_other_tenant
conversation_with_failed_message
```

Cada conversa deverá possuir:

* paciente;
* integração;
* status;
* automation mode;
* intenção;
* etapa atual;
* prioridade;
* responsável;
* versão;
* timestamps coerentes.

---

## Mensagens

Criar tipos:

```text
Inbound
Outbound
System
Human
Automated
Template
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

Cenários:

* mensagem recebida;
* mensagem enviada;
* mensagem lida;
* mensagem com falha;
* mensagem pendente;
* mensagem automatizada;
* mensagem humana;
* template;
* duplicidade simulada;
* `MessageSid` fake.

Exemplo:

```text
SM_TEST_000000000000000000000000000001
```

Nunca utilizar SID real.

---

## Fila humana

Criar itens nos status:

```text
Waiting
Assigned
Completed
Released
Transferred
```

Prioridades:

```text
Low
Normal
High
Urgent
```

A prioridade é operacional, não clínica.

Criar cenários:

* aguardando;
* atribuído ao operador;
* disponível para concorrência;
* transferível;
* liberável;
* item de outro tenant.

---

## Integrações

Criar integração fake ativa:

```text
Provider: Fake
Status: Active
Environment: Test
Sender: whatsapp:+5500000000000
```

Criar integração Twilio desabilitada:

```text
Provider: Twilio
Status: Disabled
Environment: Sandbox
Sender: placeholder
```

Regras:

* não ativar Twilio;
* não inserir Account SID;
* não inserir Auth Token;
* não inserir sender real;
* não inserir URL contendo secret;
* toda integração real dependerá de variáveis de ambiente.

---

## Templates

Criar:

```text
welcome
main_menu
appointment_confirmation
appointment_reschedule
appointment_cancellation
human_handoff
automation_resumed
```

Status:

```text
Draft
Approved
Rejected
Disabled
```

Usar `ContentSid` fake:

```text
HX_TEST_000000000000000000000000000001
```

---

## Auditoria

Criar registros para:

* login;
* conversa assumida;
* conversa liberada;
* conversa transferida;
* automação pausada;
* automação retomada;
* mensagem manual;
* consulta criada;
* consulta reagendada;
* consulta cancelada;
* integração validada;
* template sincronizado.

Usar correlation IDs fictícios e dados sanitizados.

---

## Scripts idempotentes

Os scripts devem poder ser executados mais de uma vez.

Preferir:

```sql
INSERT ... ON CONFLICT (...) DO UPDATE
```

ou:

```sql
INSERT ... ON CONFLICT DO NOTHING
```

Cada script deve:

* utilizar transação;
* falhar de forma atômica;
* validar pré-condições;
* não duplicar dados;
* não apagar dados fora do escopo.

---

## Reset

Criar comandos:

```bash
./scripts/test-data/reset.sh minimal
./scripts/test-data/reset.sh e2e
./scripts/test-data/reset.sh tenant <tenant-id>
```

O reset deverá:

* validar ambiente;
* bloquear produção;
* apagar na ordem correta;
* respeitar foreign keys;
* preservar migrations;
* limpar Inbox;
* limpar Outbox;
* limpar registros de idempotência;
* limpar auditoria de teste;
* limpar fila;
* limpar mensagens;
* limpar conversas;
* limpar consultas;
* limpar pacientes;
* limpar usuários de teste;
* limpar tenants de teste.

Não utilizar `DROP DATABASE` em ambiente compartilhado.

---

## Proteção contra produção

Antes de executar qualquer script destrutivo:

* ler `ASPNETCORE_ENVIRONMENT`;
* ler `DATABASE_NAME`;
* ler `ALLOW_TEST_DATA_RESET`;
* validar o nome do banco;
* bloquear quando o ambiente for `Production`;
* bloquear quando o banco não contiver `test`, `e2e`, `dev` ou valor permitido;
* exigir confirmação fora do CI.

Variável obrigatória:

```env
ALLOW_TEST_DATA_RESET=true
```

Sem essa variável, o reset deve falhar.

---

## Validação

Criar:

```text
database/validation/validate_seed.sql
database/validation/validate_counts.sql
database/validation/validate_foreign_keys.sql
database/validation/validate_tenant_isolation.sql
```

Validar:

* tenants;
* usuários;
* roles;
* permissões;
* unidades;
* especialidades;
* profissionais;
* pacientes;
* agenda;
* consultas;
* conversas;
* mensagens;
* fila;
* integrações;
* templates;
* auditoria;
* isolamento por tenant;
* ausência de telefone real;
* ausência de secrets;
* ausência de emails externos.

---

## Docker Compose

Adicionar serviço opcional:

```yaml
test-data-seeder:
  profiles:
    - e2e
    - development
  depends_on:
    postgres:
      condition: service_healthy
  environment:
    ASPNETCORE_ENVIRONMENT: Test
    ALLOW_TEST_DATA_RESET: "true"
  volumes:
    - ./database:/database:ro
    - ./scripts/test-data:/scripts:ro
```

O serviço deverá:

1. aguardar o PostgreSQL;
2. validar migrations;
3. executar reset;
4. executar seed;
5. executar validação;
6. retornar código diferente de zero em falha.

---

## Manifesto E2E

Criar:

```text
database/seeds/e2e/manifest.json
```

Exemplo:

```json
{
  "profile": "e2e",
  "version": "1.0.0",
  "tenantIds": {
    "main": "00000000-0000-0000-0000-000000000101",
    "isolated": "00000000-0000-0000-0000-000000000102"
  },
  "users": {
    "adminEmail": "admin.e2e@fake.local",
    "operatorEmail": "operator.e2e@fake.local",
    "operator2Email": "operator2.e2e@fake.local"
  },
  "conversations": {
    "waitingHuman": "00000000-0000-0000-0000-000000000401"
  }
}
```

Os testes Playwright e de integração deverão consumir esse manifesto sempre que possível.

Não duplicar IDs manualmente em diversos arquivos.

---

## CI

Pipeline E2E:

```text
1. subir PostgreSQL
2. aplicar migrations
3. executar reset E2E
4. executar seed E2E
5. validar dados
6. subir API
7. subir Worker
8. subir RabbitMQ
9. subir Redis
10. subir frontend
11. executar testes
12. coletar evidências
13. destruir ambiente
```

Não compartilhar o mesmo banco entre execuções paralelas.

Preferir:

* banco por execução;
* schema por execução;
* ou tenant isolado por execução.

---

## Testes obrigatórios

Criar testes para:

### Seed inicial

```text
Dado banco migrado e vazio
Quando executar perfil E2E
Então todos os dados obrigatórios serão criados
```

### Idempotência

```text
Dado perfil E2E já aplicado
Quando executar novamente
Então não haverá duplicidade
```

### Reset

```text
Dado ambiente E2E populado
Quando executar reset
Então somente os dados E2E serão removidos
```

### Tenant isolation

```text
Dados dois tenants
Quando consultar dados do tenant principal
Então dados do tenant isolado não serão retornados
```

### Produção

```text
Dado ambiente Production
Quando tentar reset
Então a operação será bloqueada
```

### Integridade

```text
Dado seed concluído
Quando validar foreign keys
Então nenhuma referência inválida será encontrada
```

---

## Documentação

Criar:

```text
docs/testing/test-data-platform.md
docs/testing/test-data-profiles.md
docs/testing/e2e-fixtures.md
docs/testing/reset-and-seed.md
docs/testing/security.md
docs/testing/troubleshooting.md
```

Documentar:

* pré-requisitos;
* comandos;
* perfis;
* IDs;
* usuários;
* reset;
* proteção;
* Docker;
* CI;
* problemas comuns.

---

## Critérios de aceite

A etapa estará concluída quando:

1. scripts SQL estiverem versionados;
2. migrations continuarem separadas;
3. perfil Minimal funcionar;
4. perfil E2E funcionar;
5. seed for idempotente;
6. reset for seguro;
7. execução em produção estiver bloqueada;
8. IDs E2E forem determinísticos;
9. usuários E2E forem criados;
10. roles e permissões funcionarem;
11. tenants de teste forem criados;
12. isolamento multi-tenant for validado;
13. unidades forem criadas;
14. especialidades forem criadas;
15. profissionais forem criados;
16. pacientes fictícios forem criados;
17. agenda for criada;
18. consultas forem criadas;
19. conversas forem criadas;
20. mensagens forem criadas;
21. fila humana for criada;
22. integração Fake estiver ativa;
23. integração Twilio permanecer desabilitada;
24. templates forem criados;
25. auditoria for criada;
26. nenhum secret estiver presente;
27. nenhum dado real estiver presente;
28. Docker Compose executar o seed;
29. CI executar o seed;
30. testes unitários passarem;
31. testes de integração passarem;
32. scripts de validação passarem;
33. documentação estiver completa.

---

## Instrução inicial ao Codex

Antes de alterar o código:

1. analise todas as migrations;
2. liste as entidades e tabelas;
3. liste as foreign keys;
4. liste os enums;
5. liste os campos obrigatórios;
6. liste os índices únicos;
7. identifique como `TenantId` é aplicado;
8. identifique como as senhas são geradas;
9. identifique roles e permissões;
10. identifique dados de referência já existentes;
11. identifique scripts atuais;
12. identifique riscos de executar SQL diretamente;
13. identifique a ordem correta de inserção;
14. identifique a ordem correta de exclusão;
15. apresente os arquivos que serão criados e alterados;
16. não altere o código antes de concluir a análise.

Implemente inicialmente somente:

```text
E2E-00.1 — Fundação dos scripts
E2E-00.2 — Dados comuns
E2E-00.3 — Perfil Minimal
E2E-00.4 — Perfil E2E
E2E-00.5 — Reset e validação
```

A primeira entrega deverá conter:

1. estrutura `database/seeds`;
2. estrutura `scripts/test-data`;
3. proteção contra produção;
4. runner de scripts;
5. manifesto E2E;
6. roles e permissões;
7. tenant principal E2E;
8. tenant isolado E2E;
9. usuários E2E;
10. unidades;
11. especialidades;
12. profissionais;
13. pacientes;
14. agenda;
15. consultas;
16. conversas;
17. mensagens;
18. fila humana;
19. integração Fake;
20. integração Twilio desabilitada;
21. templates;
22. auditoria;
23. reset E2E;
24. validação E2E;
25. Docker Compose;
26. testes;
27. documentação.

Regras adicionais:

* não adicionar credenciais reais;
* não adicionar telefone real;
* não ativar Twilio;
* não alterar migrations existentes sem necessidade;
* não inserir senha em texto puro;
* não executar scripts em `Production`;
* não usar dados aleatórios nos fixtures principais;
* não duplicar IDs;
* não avançar para grandes volumes antes do perfil E2E passar;
* não executar testes reais com Twilio nesta etapa.

Após implementar, executar:

```bash
dotnet restore
dotnet build
dotnet test
docker compose up -d postgres
./scripts/test-data/reset.sh e2e
./scripts/test-data/seed.sh e2e
./scripts/test-data/validate.sh e2e
```

Corrigir todos os erros.

Ao final, apresentar:

1. arquivos criados;
2. arquivos alterados;
3. comandos de execução;
4. usuários criados;
5. IDs principais;
6. quantidades por entidade;
7. testes executados;
8. riscos restantes;
9. evidências de idempotência;
10. evidências de bloqueio em produção.
