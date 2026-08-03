# Etapa 9.3 — Hardening Operacional, APIs Pendentes e Fechamento E2E

## Contexto

A solução já possui uma base funcional de backend, frontend, integração com WhatsApp, conversas, agenda, multi-tenancy, SignalR parcial, testes e documentação.

Entretanto, ainda existem lacunas relevantes que impedem considerar o produto operacionalmente completo e pronto para validação fim a fim.

Esta etapa deverá corrigir problemas de autorização, concluir APIs administrativas, completar fluxos de agenda e conversas, ampliar o realtime, estabilizar autenticação, fechar os cenários E2E e revisar segurança operacional.

A prioridade obrigatória será:

```text
1. Corrigir autorização no backend
2. Disponibilizar APIs administrativas e auditoria
3. Implementar reagendamento e operações completas de conversa
4. Ampliar eventos SignalR
5. Fechar Playwright e CI
6. Revisar segurança e documentação
```

Não iniciar IA, RAG ou Tool Calling nesta etapa.

---

# 1. Objetivo

Ao final desta etapa, a solução deverá possuir:

* autorização correta no backend;
* separação clara entre perfis administrativos e operacionais;
* APIs administrativas de plataforma;
* wizard transacional de onboarding;
* cadastros administrativos completos;
* agenda com reagendamento, detalhe, filtros e concorrência;
* conversas com fila humana e operações completas;
* WhatsApp administrável;
* auditoria consultável;
* dashboard agregado;
* SignalR cobrindo todos os fluxos relevantes;
* autenticação persistente e refresh automático;
* testes Playwright completos;
* pipeline CI;
* dependências revisadas;
* CSP validada;
* Postman atualizado;
* OpenAPI versionado quando aplicável.

---

# 2. Análise inicial obrigatória

Antes de alterar o código:

1. analise as policies e roles atuais;
2. identifique endpoints protegidos apenas por `ClinicStaff`;
3. identifique mutations acessíveis por `Receptionist`;
4. identifique mutations acessíveis por `Professional`;
5. liste endpoints que deveriam exigir `ClinicAdmin`;
6. analise APIs administrativas existentes;
7. identifique APIs de tenants, usuários e clínicas globais ausentes;
8. analise pacientes, unidades, profissionais e especialidades;
9. analise agenda;
10. analise conversas;
11. analise integração WhatsApp;
12. analise auditoria;
13. analise dashboard;
14. analise eventos SignalR;
15. analise persistência de autenticação;
16. analise testes Playwright;
17. analise configuração de CI;
18. execute ou analise `npm audit`;
19. analise a CSP;
20. compare OpenAPI, Postman e endpoints reais;
21. apresente riscos;
22. não altere o código antes de concluir a análise.

Criar matrizes:

## Endpoint x Permissão

| Endpoint | Método | Policy atual | Policy correta | Risco |
| -------- | ------ | ------------ | -------------- | ----- |

## Feature x Lacuna

| Feature | Implementado | Ausente | Prioridade |
| ------- | ------------ | ------- | ---------- |

## Evento SignalR x Origem

| Evento | Origem | Publicado atualmente | Ação necessária |
| ------ | ------ | -------------------- | --------------- |

## E2E x Dependência

| Cenário | Backend | Frontend | Seed | Realtime | Status |
| ------- | ------- | -------- | ---- | -------- | ------ |

---

# 3. Autorização no backend

## Problema atual

Alterações de:

* clínica;
* unidades;
* pacientes;
* profissionais;
* especialidades;

continuam protegidas apenas por `ClinicStaff`.

Isso permite mutations para perfis como:

```text
Receptionist
Professional
```

A interface oculta essas ações, mas o backend ainda permite a execução.

Essa proteção é insuficiente.

## Correção obrigatória

Criar ou reutilizar policies específicas:

```text
Clinics.View
Clinics.Manage

Units.View
Units.Manage

Patients.View
Patients.Manage

Professionals.View
Professionals.Manage

Specialties.View
Specialties.Manage
```

Mapeamento inicial recomendado:

| Operação                    | Policy                 |
| --------------------------- | ---------------------- |
| Listar clínicas             | `Clinics.View`         |
| Editar clínica              | `Clinics.Manage`       |
| Listar unidades             | `Units.View`           |
| Criar/editar unidade        | `Units.Manage`         |
| Consultar pacientes         | `Patients.View`        |
| Criar/editar pacientes      | `Patients.Manage`      |
| Listar profissionais        | `Professionals.View`   |
| Criar/editar profissionais  | `Professionals.Manage` |
| Listar especialidades       | `Specialties.View`     |
| Criar/editar especialidades | `Specialties.Manage`   |

## Perfis

Sugestão inicial:

```text
ClinicAdmin
    todas as operações de gestão da clínica

Manager
    operações administrativas configuradas

Receptionist
    agenda, pacientes operacionais e conversas conforme permissão

Professional
    somente sua agenda e dados autorizados

Operator
    conversas e fila

Viewer
    somente leitura
```

Não confiar somente em role.

Utilizar policies e claims.

## Testes obrigatórios

Criar testes garantindo:

* `ClinicAdmin` pode executar mutations;
* `Receptionist` não pode alterar clínica;
* `Receptionist` não pode alterar especialidade;
* `Professional` não pode criar unidade;
* `Professional` não pode editar paciente;
* `Viewer` não pode executar mutations;
* outro tenant não pode acessar o recurso;
* frontend ocultar ação não substitui backend.

---

# 4. Administração da plataforma

Ainda faltam APIs e telas para administração global.

## APIs necessárias

Criar endpoints equivalentes a:

```text
GET    /api/v1/platform/tenants
GET    /api/v1/platform/tenants/{tenantId}
POST   /api/v1/platform/tenants
PUT    /api/v1/platform/tenants/{tenantId}
POST   /api/v1/platform/tenants/{tenantId}/activate
POST   /api/v1/platform/tenants/{tenantId}/suspend
POST   /api/v1/platform/tenants/{tenantId}/deactivate
```

Usuários:

```text
GET    /api/v1/platform/users
GET    /api/v1/platform/users/{userId}
POST   /api/v1/platform/users
PUT    /api/v1/platform/users/{userId}
POST   /api/v1/platform/users/{userId}/activate
POST   /api/v1/platform/users/{userId}/deactivate
```

Clínicas globais:

```text
GET /api/v1/platform/clinics
GET /api/v1/platform/clinics/{clinicId}
```

Somente `PlatformAdmin`.

---

# 5. Wizard transacional de onboarding

Criar endpoint de workflow:

```text
POST /api/v1/platform/onboarding
```

Request sugerido:

```json
{
  "tenant": {
    "name": "Clínica Exemplo",
    "slug": "clinica-exemplo",
    "timezone": "America/Recife",
    "locale": "pt-BR",
    "plan": "Starter"
  },
  "clinic": {
    "legalName": "Clínica Exemplo Ltda.",
    "tradeName": "Clínica Exemplo",
    "document": "documento-ficticio",
    "email": "contato@example.test",
    "phone": "+5500000000000"
  },
  "unit": {
    "name": "Unidade Principal"
  },
  "administrator": {
    "name": "Administrador",
    "email": "admin@example.test"
  },
  "integration": {
    "provider": "Fake",
    "enabled": false
  }
}
```

Requisitos:

* operação transacional;
* rollback em falha;
* validações;
* slug único;
* email único quando aplicável;
* criação de roles e permissões;
* criação de ClinicAdmin;
* integração desabilitada por padrão;
* idempotency key;
* auditoria;
* evento de domínio;
* resultado detalhado.

Não deixar entidades parcialmente criadas.

---

# 6. Cadastros de pacientes

## Lacunas

Ainda faltam:

* detalhes;
* busca;
* paginação;
* origem;
* histórico;
* auditoria.

## Endpoints

```text
GET  /api/v1/admin/patients
GET  /api/v1/admin/patients/{patientId}
POST /api/v1/admin/patients
PUT  /api/v1/admin/patients/{patientId}
```

Filtros:

```text
page
pageSize
search
phone
email
status
source
hasUpcomingAppointment
createdFrom
createdTo
sort
```

Detalhes devem incluir:

* dados administrativos;
* origem;
* consentimentos;
* última interação;
* consultas futuras;
* consultas anteriores resumidas;
* conversas;
* auditoria resumida.

Não incluir prontuário ou diagnóstico.

---

# 7. Unidades

## Lacunas

* horário de funcionamento;
* ativação e desativação;
* vínculo de profissionais.

## Endpoints

```text
GET    /api/v1/admin/units
GET    /api/v1/admin/units/{unitId}
POST   /api/v1/admin/units
PUT    /api/v1/admin/units/{unitId}
POST   /api/v1/admin/units/{unitId}/activate
POST   /api/v1/admin/units/{unitId}/deactivate
PUT    /api/v1/admin/units/{unitId}/business-hours
PUT    /api/v1/admin/units/{unitId}/professionals
```

Tratar:

* timezone;
* horários sobrepostos;
* profissional de outro tenant;
* unidade inativa;
* concorrência;
* auditoria.

---

# 8. Profissionais

## Lacunas

* disponibilidade;
* bloqueios;
* férias;
* agenda dedicada.

## Endpoints

```text
GET  /api/v1/admin/professionals/{professionalId}/availability
PUT  /api/v1/admin/professionals/{professionalId}/availability

GET  /api/v1/admin/professionals/{professionalId}/blocks
POST /api/v1/admin/professionals/{professionalId}/blocks
DELETE /api/v1/admin/professionals/{professionalId}/blocks/{blockId}

GET  /api/v1/admin/professionals/{professionalId}/vacations
POST /api/v1/admin/professionals/{professionalId}/vacations
DELETE /api/v1/admin/professionals/{professionalId}/vacations/{vacationId}

GET /api/v1/admin/professionals/{professionalId}/schedule
```

Validar:

* sobreposição;
* timezone;
* tenant;
* profissional ativo;
* impacto em consultas;
* auditoria.

---

# 9. Especialidades

## Lacuna

Não existe controle seguro de dependências antes de excluir ou desativar.

## Regras

Antes de excluir ou desativar, verificar:

* profissionais vinculados;
* consultas futuras;
* agenda;
* templates;
* histórico.

Preferir exclusão lógica.

Endpoints:

```text
GET  /api/v1/admin/specialties/{specialtyId}/dependencies
POST /api/v1/admin/specialties/{specialtyId}/activate
POST /api/v1/admin/specialties/{specialtyId}/deactivate
```

Retornar:

```json
{
  "canDeactivate": false,
  "dependencies": {
    "professionals": 4,
    "futureAppointments": 12
  }
}
```

---

# 10. Agenda

## Estado atual

Existem:

* criação;
* listagem;
* confirmação;
* cancelamento.

## Faltam

* reagendamento;
* detalhe;
* filtros avançados;
* `expectedVersion`;
* `Idempotency-Key`.

## Endpoints

Detalhe:

```text
GET /api/v1/admin/appointments/{appointmentId}
```

Reagendamento:

```text
POST /api/v1/admin/appointments/{appointmentId}/reschedule
```

Filtros:

```text
page
pageSize
professionalId
specialtyId
unitId
patientId
status
source
from
to
sort
```

Operações críticas:

```text
Create
Confirm
Reschedule
Cancel
```

Devem suportar:

* `Idempotency-Key`;
* `expectedVersion`;
* HTTP 409;
* transação;
* auditoria;
* Outbox;
* eventos SignalR.

---

# 11. Conversas

## Lacunas

Ainda faltam:

* fila humana visível;
* transferência;
* mensagem manual via Outbox;
* encerramento;
* reabertura;
* prioridade;
* auditoria;
* consultas vinculadas ao paciente.

## Endpoints

Fila:

```text
GET /api/v1/admin/conversation-queue
```

Operações:

```text
POST  /api/v1/admin/conversations/{id}/assign
POST  /api/v1/admin/conversations/{id}/release
POST  /api/v1/admin/conversations/{id}/transfer
POST  /api/v1/admin/conversations/{id}/pause-automation
POST  /api/v1/admin/conversations/{id}/resume-automation
POST  /api/v1/admin/conversations/{id}/close
POST  /api/v1/admin/conversations/{id}/reopen
PATCH /api/v1/admin/conversations/{id}/priority
POST  /api/v1/admin/conversations/{id}/messages
```

Consultas vinculadas:

```text
GET /api/v1/admin/conversations/{id}/appointments
```

Mensagem manual:

* criar `ConversationMessage`;
* criar `OutboxMessage`;
* mesma transação;
* nunca chamar Twilio no controller;
* idempotency key;
* status via SignalR.

---

# 12. WhatsApp administrativo

## Estado atual

Existe apenas status operacional em leitura.

## Faltam

* templates;
* sincronização;
* configuração controlada;
* diagnóstico ampliado;
* operações administrativas.

## Endpoints

```text
GET  /api/v1/admin/integrations/whatsapp
GET  /api/v1/admin/integrations/whatsapp/{id}
POST /api/v1/admin/integrations/whatsapp/{id}/validate
POST /api/v1/admin/integrations/whatsapp/{id}/test-message
POST /api/v1/admin/integrations/whatsapp/{id}/enable
POST /api/v1/admin/integrations/whatsapp/{id}/disable
POST /api/v1/admin/integrations/whatsapp/{id}/sync-templates
```

Templates:

```text
GET    /api/v1/admin/whatsapp/templates
GET    /api/v1/admin/whatsapp/templates/{id}
POST   /api/v1/admin/whatsapp/templates
PUT    /api/v1/admin/whatsapp/templates/{id}
POST   /api/v1/admin/whatsapp/templates/{id}/activate
POST   /api/v1/admin/whatsapp/templates/{id}/deactivate
POST   /api/v1/admin/whatsapp/templates/sync
```

Nunca retornar:

* Auth Token;
* Account SID completo;
* secrets;
* URLs sensíveis.

---

# 13. Auditoria

## Problema

Não existe endpoint nem tela `/audit`.

## Backend

Criar:

```text
GET /api/v1/admin/audit
```

Filtros:

```text
page
pageSize
userId
action
resourceType
resourceId
result
from
to
correlationId
```

Retornar dados sanitizados.

## Frontend

Criar:

```text
/audit
```

Exibir:

* data;
* usuário;
* ação;
* recurso;
* resultado;
* correlation ID;
* detalhes sanitizados.

Somente usuários autorizados.

---

# 14. Dashboard agregado

## Problema

Ainda não existe endpoint agregado para:

* métricas;
* SLA;
* filas;
* consultas;
* falhas recentes.

## Endpoint

```text
GET /api/v1/admin/dashboard
```

Filtros:

```text
from
to
unitId
queueId
assignedUserId
```

Métricas:

* conversas;
* mensagens;
* fila;
* SLA;
* consultas;
* integração;
* falhas recentes.

Utilizar queries agregadas.

Não carregar entidades completas.

---

# 15. SignalR

## Estado atual

Já existem:

* Hub;
* isolamento por tenant;
* eventos de agenda;
* eventos de conversas.

## Faltam eventos para

* inbound WhatsApp;
* callbacks de status;
* templates;
* auditoria;
* fila humana;
* dashboard.

## Eventos obrigatórios

```text
whatsapp.inbound.received
whatsapp.message.status.changed
whatsapp.template.created
whatsapp.template.updated
whatsapp.template.synced

audit.created

queue.item.created
queue.item.updated
queue.item.assigned
queue.item.released
queue.item.transferred
queue.item.completed

dashboard.invalidated
```

Eventos devem:

* ser publicados após commit;
* possuir EventId;
* possuir TenantId;
* possuir correlation ID;
* possuir timestamp;
* permitir deduplicação;
* não expor payload bruto;
* respeitar grupos autorizados.

---

# 16. Autenticação persistente

## Problema atual

Access e refresh tokens permanecem apenas em memória.

Após reload, o usuário precisa autenticar novamente.

## Objetivo

Implementar sessão persistente segura.

Preferir:

```text
Access Token
    curta duração

Refresh Token
    cookie HttpOnly
    Secure
    SameSite
```

## Endpoints

```text
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/session
```

Requisitos:

* rotação de refresh token;
* revogação;
* reutilização detectada;
* expiração;
* logout;
* sessão restaurada após reload;
* interceptor ou handler de refresh;
* retry único após 401;
* evitar loop;
* proteção CSRF quando aplicável;
* cookies seguros;
* testes.

Não persistir token sensível em localStorage se cookies seguros estiverem disponíveis.

---

# 17. E2E e Playwright

## Estado atual

Playwright está estruturado, mas depende de ambiente seedado.

## Cobertura pendente

* criação;
* edição;
* conflitos;
* realtime;
* FakeWhatsApp;
* isolamento multi-tenant;
* agenda completa;
* CI.

## Cenários obrigatórios

### Administração

* criar tenant;
* onboarding;
* editar clínica;
* criar unidade;
* criar usuário.

### Pacientes

* criar;
* editar;
* buscar;
* detalhe.

### Profissionais

* criar;
* disponibilidade;
* bloqueio;
* férias.

### Agenda

* criar;
* confirmar;
* reagendar;
* conflito;
* cancelar;
* detalhe;
* filtros.

### Conversas

* inbound fake;
* fila;
* assumir;
* transferir;
* enviar mensagem;
* pausar;
* retomar;
* encerrar;
* reabrir;
* prioridade.

### Multi-tenant

* acesso cruzado bloqueado;
* cache limpo ao trocar tenant;
* SignalR isolado.

### Realtime

* mensagem recebida;
* status atualizado;
* fila atualizada;
* dashboard invalidado.

---

# 18. CI

Criar pipelines:

```text
pull_request:
  backend build
  backend tests
  frontend lint
  frontend typecheck
  frontend tests
  fake E2E

main:
  todos os anteriores

manual_twilio_smoke:
  smoke real controlado
```

Requisitos:

* banco isolado;
* seed E2E;
* migrations;
* API;
* Worker;
* Redis;
* RabbitMQ;
* frontend;
* Playwright;
* artifacts;
* traces;
* screenshots em falha;
* nenhum secret exposto.

---

# 19. Segurança operacional

## npm audit

O `npm install` reportou 12 vulnerabilidades de alta severidade.

Executar:

```bash
npm audit
```

Analisar:

* pacote;
* versão;
* CVE;
* dependência direta ou transitiva;
* correção disponível;
* risco de breaking change;
* uso real no projeto.

Não executar `npm audit fix --force` sem análise.

Criar relatório:

```text
docs/security/npm-audit-report.md
```

Classificar:

```text
Resolved
Mitigated
Accepted Temporarily
Blocked
```

---

# 20. CSP

Validar a CSP com a URL real da API no build.

Revisar:

```text
connect-src
script-src
style-src
img-src
font-src
frame-src
```

Incluir somente:

* frontend;
* API;
* SignalR;
* recursos realmente necessários.

Não usar:

```text
*
unsafe-eval
```

sem justificativa explícita.

Criar testes ou validação de headers.

---

# 21. Postman

Atualizar a collection para incluir:

* pacientes;
* agenda;
* conversas;
* fila;
* SignalR ou documentação do hub;
* WhatsApp;
* templates;
* auditoria;
* dashboard;
* onboarding;
* refresh token.

Fluxos E2E:

```text
Onboarding
Paciente
Agenda
Conversa
WhatsApp
Auditoria
Dashboard
```

Salvar IDs automaticamente.

Nenhum secret versionado.

---

# 22. OpenAPI estático

Avaliar se governança exige OpenAPI versionado.

Caso sim, gerar:

```text
openapi/clinic-assistant-v1.json
```

ou:

```text
openapi/clinic-assistant-v1.yaml
```

Requisitos:

* gerado a partir da aplicação;
* não mantido manualmente;
* validado no CI;
* comparação de drift;
* versão;
* endpoints administrativos completos.

Caso não seja requisito, documentar a decisão.

---

# 23. Documentação

Atualizar:

```text
docs/api/authorization.md
docs/api/platform-administration.md
docs/api/patients.md
docs/api/scheduling.md
docs/api/conversations.md
docs/api/whatsapp.md
docs/api/audit.md
docs/api/dashboard.md
docs/api/realtime.md
docs/security/npm-audit-report.md
docs/security/csp.md
docs/testing/e2e-execution-guide.md
docs/testing/playwright.md
postman/README.md
```

---

# 24. Observabilidade

Adicionar métricas:

```text
authorization_denied_total
platform_onboarding_total
platform_onboarding_failures_total
appointments_rescheduled_total
appointment_conflicts_total
manual_messages_total
audit_entries_total
dashboard_requests_total
signalr_events_published_total
signalr_publish_failures_total
refresh_token_rotations_total
refresh_token_reuse_detected_total
```

Logs devem conter:

* TenantId;
* UserId;
* recurso;
* operação;
* policy;
* correlation ID;
* trace ID;
* resultado.

Sem dados sensíveis.

---

# 25. Ordem obrigatória de implementação

## 9.3.1 Autorização

* policies;
* endpoints;
* testes;
* frontend alinhado.

## 9.3.2 Administração de plataforma

* tenants;
* users;
* clínicas globais;
* onboarding.

## 9.3.3 Cadastros completos

* pacientes;
* unidades;
* profissionais;
* especialidades.

## 9.3.4 Agenda

* detalhe;
* filtros;
* reagendamento;
* concorrência;
* idempotência.

## 9.3.5 Conversas

* fila;
* transferência;
* envio manual;
* close;
* reopen;
* prioridade;
* auditoria;
* appointments.

## 9.3.6 WhatsApp e auditoria

* integração;
* templates;
* audit endpoint;
* tela audit.

## 9.3.7 Dashboard e SignalR

* agregações;
* eventos pendentes;
* atualização frontend.

## 9.3.8 Autenticação

* refresh;
* cookies;
* restauração de sessão.

## 9.3.9 E2E e CI

* Playwright;
* seed;
* realtime;
* FakeWhatsApp;
* CI;
* smoke.

## 9.3.10 Segurança e documentação

* npm audit;
* CSP;
* Postman;
* OpenAPI;
* documentação.

---

# 26. Primeira entrega

Implemente inicialmente somente:

```text
9.3.1 — Autorização
9.3.2 — Administração de plataforma
9.3.3 — Cadastros administrativos prioritários
```

Primeira entrega esperada:

1. matriz endpoint x policy;
2. policies novas;
3. correção de endpoints;
4. testes de autorização;
5. APIs de tenants;
6. APIs de usuários da plataforma;
7. APIs de clínicas globais;
8. onboarding transacional;
9. pacientes paginados;
10. detalhe de paciente;
11. atualização de paciente;
12. unidades com status;
13. unidades com horários;
14. vínculo unidade-profissional;
15. profissionais com disponibilidade;
16. especialidades com verificação de dependências;
17. telas administrativas correspondentes;
18. testes backend;
19. testes frontend;
20. OpenAPI atualizado;
21. Postman atualizado;
22. documentação atualizada.

Não avançar para agenda, conversas, SignalR, autenticação ou Playwright enquanto:

* autorização não estiver corrigida;
* Receptionist e Professional não puderem executar mutations administrativas;
* APIs de plataforma não estiverem testadas;
* onboarding não for transacional;
* multi-tenancy não estiver validado;
* testes não passarem.

---

# 27. Validação

Backend:

```bash
dotnet restore
dotnet build
dotnet test
```

Frontend:

```bash
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```

Segurança:

```bash
npm audit
```

Executar também os validadores de:

* OpenAPI;
* Postman;
* documentação;
* links;
* secrets.

Não corrigir vulnerabilidades com `--force` sem análise.

---

# 28. Critérios de aceite finais

A etapa somente estará concluída quando:

```text
1. Autorização backend estiver correta
2. ClinicStaff não proteger mutations administrativas genéricas
3. Receptionist não puder administrar clínica
4. Professional não puder executar mutations administrativas
5. PlatformAdmin possuir APIs próprias
6. Onboarding transacional funcionar
7. Pacientes estiverem completos
8. Unidades estiverem completas
9. Profissionais estiverem completos
10. Especialidades tratarem dependências
11. Agenda possuir reagendamento
12. Agenda possuir detalhe
13. Agenda possuir filtros
14. Agenda possuir expectedVersion
15. Agenda possuir Idempotency-Key
16. Conversas possuírem fila humana
17. Transferência funcionar
18. Mensagem manual usar Outbox
19. Encerramento funcionar
20. Reabertura funcionar
21. Prioridade funcionar
22. Auditoria existir
23. Consultas vinculadas à conversa existirem
24. WhatsApp administrativo funcionar
25. Templates funcionarem
26. Dashboard agregado existir
27. SignalR cobrir eventos pendentes
28. Sessão sobreviver ao reload
29. Refresh automático funcionar
30. Playwright cobrir fluxos principais
31. Multi-tenancy E2E estiver validado
32. FakeWhatsApp E2E funcionar
33. CI executar E2E fake
34. Vulnerabilidades npm estiverem tratadas ou justificadas
35. CSP estiver validada
36. Postman estiver completo
37. OpenAPI estiver versionado ou decisão documentada
38. Documentação estiver atualizada
39. Build e testes passarem
40. Nenhum secret estiver exposto
```

---

# 29. Relatório final

Ao finalizar cada incremento, apresente:

1. problemas encontrados;
2. vulnerabilidades corrigidas;
3. arquivos criados;
4. arquivos alterados;
5. endpoints criados;
6. endpoints protegidos;
7. policies criadas;
8. telas criadas;
9. eventos SignalR adicionados;
10. testes executados;
11. resultado dos comandos;
12. endpoints ainda ausentes;
13. riscos pendentes;
14. próximos passos.

Não avance automaticamente para a próxima subetapa.
