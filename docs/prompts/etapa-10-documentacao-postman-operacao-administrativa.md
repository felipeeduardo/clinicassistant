# Etapa 10 — Consolidação da Documentação, Postman e Operação Administrativa

## Contexto

O projeto já possui múltiplos documentos produzidos durante as etapas de arquitetura, integração com WhatsApp, conversas, APIs administrativas, dados E2E, frontend e testes.

Com a evolução do sistema, parte dessa documentação pode estar:

* duplicada;
* fragmentada;
* desatualizada;
* contraditória;
* excessivamente detalhada em locais incorretos;
* sem relação direta com os endpoints implementados;
* sem roteiro operacional;
* sem exemplos reais de execução;
* sem correspondência com a collection do Postman.

Esta etapa deverá reorganizar toda a documentação para que ela se torne uma fonte confiável, prática e atualizável.

Também deverá consolidar o Postman como ferramenta de validação manual dos endpoints e criar roteiros fim a fim para os principais fluxos administrativos.

A documentação não deverá apenas explicar arquitetura. Ela deverá permitir que um desenvolvedor ou operador execute o sistema.

---

# 1. Objetivos

Ao final desta etapa, o projeto deverá possuir:

* documentação centralizada;
* eliminação de redundâncias;
* fontes únicas de verdade;
* índice navegável;
* documentação arquitetural;
* documentação operacional;
* documentação das APIs;
* documentação dos fluxos E2E;
* collection Postman atualizada;
* environments Postman;
* scripts de testes Postman;
* exemplos de payloads;
* variáveis automáticas;
* formulários e endpoints administrativos claramente definidos;
* fluxo de criação de novos clientes;
* fluxo de atualização de clientes;
* fluxo de onboarding de clínicas;
* checklist obrigatório de atualização da documentação;
* validação automática de links e inconsistências.

---

# 2. Princípios obrigatórios

* não apagar documentação antes de analisar seu conteúdo;
* não perder decisões arquiteturais relevantes;
* não manter dois documentos como fonte de verdade para o mesmo tema;
* não inventar endpoints;
* documentar somente endpoints existentes ou explicitamente marcados como pendentes;
* OpenAPI deve ser a fonte técnica dos contratos HTTP;
* Postman deve refletir o OpenAPI e os fluxos operacionais;
* documentos conceituais não devem duplicar contratos completos;
* documentos operacionais devem priorizar comandos copiáveis;
* toda alteração de endpoint deve atualizar documentação e Postman;
* exemplos não podem conter secrets;
* exemplos não podem conter dados reais;
* tokens devem ser representados por variáveis;
* telefones e emails devem ser fictícios;
* documentos obsoletos devem ser arquivados ou removidos de maneira controlada;
* todos os links internos devem ser validados;
* documentação deve permanecer em português, salvo termos técnicos e contratos.

---

# 3. Análise inicial obrigatória

Antes de alterar qualquer arquivo:

1. liste todos os arquivos Markdown do repositório;
2. liste documentos relacionados ao mesmo tema;
3. identifique documentos duplicados;
4. identifique documentos parcialmente duplicados;
5. identifique documentos contraditórios;
6. identifique documentos desatualizados;
7. identifique documentos sem referência no índice principal;
8. identifique links quebrados;
9. identifique endpoints mencionados que não existem;
10. identifique endpoints implementados que não estão documentados;
11. identifique collections Postman existentes;
12. identifique environments Postman existentes;
13. identifique variáveis utilizadas no Postman;
14. compare Postman com OpenAPI;
15. identifique requests Postman quebradas;
16. identifique endpoints administrativos ausentes;
17. identifique fluxos E2E não documentados;
18. identifique formulários administrativos necessários;
19. apresente uma proposta de consolidação;
20. não altere arquivos antes de concluir a análise.

Produza inicialmente uma matriz:

| Tema        | Documentos atuais | Fonte proposta                   | Ação       |
| ----------- | ----------------- | -------------------------------- | ---------- |
| Arquitetura | ...               | `docs/architecture/overview.md`  | consolidar |
| WhatsApp    | ...               | `docs/integrations/whatsapp.md`  | mesclar    |
| E2E         | ...               | `docs/testing/e2e-guide.md`      | consolidar |
| Conversas   | ...               | `docs/features/conversations.md` | atualizar  |

Ações possíveis:

```text
Keep
Merge
Rewrite
Archive
Delete
Move
Update
```

Não executar `Delete` sem justificar.

---

# 4. Estrutura documental proposta

Organizar a documentação em:

```text
docs/
├── README.md
├── getting-started/
│   ├── overview.md
│   ├── local-environment.md
│   ├── configuration.md
│   ├── migrations.md
│   ├── seed-and-reset.md
│   └── troubleshooting.md
├── architecture/
│   ├── overview.md
│   ├── modules.md
│   ├── multi-tenancy.md
│   ├── messaging.md
│   ├── inbox-outbox.md
│   ├── security.md
│   ├── observability.md
│   └── decisions/
├── features/
│   ├── authentication.md
│   ├── tenants.md
│   ├── clinics.md
│   ├── patients.md
│   ├── professionals.md
│   ├── specialties.md
│   ├── scheduling.md
│   ├── conversations.md
│   ├── human-service.md
│   ├── whatsapp.md
│   ├── templates.md
│   └── audit.md
├── api/
│   ├── overview.md
│   ├── authentication.md
│   ├── administrative.md
│   ├── webhooks.md
│   ├── errors.md
│   ├── pagination.md
│   ├── idempotency.md
│   └── postman.md
├── operations/
│   ├── tenant-onboarding.md
│   ├── clinic-administration.md
│   ├── whatsapp-setup.md
│   ├── user-management.md
│   ├── incident-response.md
│   └── runbooks/
├── testing/
│   ├── overview.md
│   ├── test-data-platform.md
│   ├── e2e-execution-guide.md
│   ├── e2e-api-flows.md
│   ├── e2e-whatsapp.md
│   ├── postman-execution.md
│   └── fixtures.md
└── archive/
```

Ajustar essa estrutura à realidade do projeto.

Não criar pastas vazias ou documentos sem utilidade.

---

# 5. Documento principal

Criar ou atualizar:

```text
docs/README.md
```

O documento deverá conter:

* visão geral;
* mapa da documentação;
* primeiros passos;
* arquitetura;
* módulos;
* APIs;
* Postman;
* E2E;
* operação;
* troubleshooting;
* decisões arquiteturais;
* como contribuir com a documentação.

Esse arquivo deverá ser o ponto inicial para qualquer pessoa que entre no projeto.

---

# 6. Política de fonte única de verdade

Definir claramente:

| Informação           | Fonte de verdade                  |
| -------------------- | --------------------------------- |
| Contratos HTTP       | OpenAPI                           |
| Fluxos manuais       | Documentação E2E                  |
| Requests executáveis | Postman                           |
| Estrutura do banco   | Migrations                        |
| Configuração         | Options e `.env.example`          |
| Arquitetura          | ADRs e documentos de arquitetura  |
| IDs de teste         | Manifesto E2E                     |
| Scripts de execução  | Scripts versionados               |
| Permissões           | Código e documento de autorização |

Documentos não devem copiar integralmente o OpenAPI.

Devem:

* explicar finalidade;
* indicar sequência;
* fornecer exemplos importantes;
* referenciar contratos oficiais.

---

# 7. Documentação como parte da entrega

Criar um checklist obrigatório em:

```text
docs/documentation-checklist.md
```

Toda alteração deverá verificar:

```text
[ ] OpenAPI atualizado
[ ] Postman atualizado
[ ] Documentação da feature atualizada
[ ] Fluxo E2E atualizado
[ ] `.env.example` atualizado
[ ] Permissões atualizadas
[ ] Migrations documentadas
[ ] Erros documentados
[ ] Exemplos sanitizados
[ ] Links validados
```

Adicionar esse checklist ao template de pull request quando existir.

---

# 8. Postman

Criar ou consolidar:

```text
postman/
├── ClinicAssistant.postman_collection.json
├── environments/
│   ├── Local.postman_environment.json
│   ├── E2E.postman_environment.json
│   └── Sandbox.postman_environment.json
├── data/
│   └── e2e-test-data.json
└── README.md
```

Nunca incluir:

* Auth Token Twilio;
* senha real;
* JWT válido;
* secrets;
* telefone real;
* sender real;
* credenciais de produção.

---

# 9. Estrutura da collection

Organizar pastas:

```text
00 — Health
01 — Authentication
02 — Tenants
03 — Clinics
04 — Units
05 — Users
06 — Patients
07 — Professionals
08 — Specialties
09 — Availability
10 — Appointments
11 — Conversations
12 — Human Queue
13 — WhatsApp
14 — Templates
15 — Dashboard
16 — Audit
17 — Webhooks
18 — E2E Flows
```

As pastas devem refletir os endpoints reais.

---

# 10. Variáveis Postman

Criar variáveis:

```text
baseUrl
accessToken
refreshToken
tenantId
clinicId
unitId
userId
patientId
professionalId
specialtyId
appointmentId
conversationId
queueItemId
integrationId
templateId
messageId
slotId
idempotencyKey
correlationId
```

Variáveis específicas do Twilio devem usar somente placeholders:

```text
twilioAccountSid
twilioWhatsAppFrom
twilioTestRecipient
```

Não incluir `twilioAuthToken` em arquivos versionados.

---

# 11. Scripts Postman

Após login:

```javascript
const json = pm.response.json();

if (json.accessToken) {
    pm.environment.set("accessToken", json.accessToken);
}

if (json.refreshToken) {
    pm.environment.set("refreshToken", json.refreshToken);
}
```

Após criação de paciente:

```javascript
const json = pm.response.json();
pm.environment.set("patientId", json.id);
```

Após criação de conversa ou consulta, salvar os respectivos IDs.

Gerar idempotency key:

```javascript
pm.variables.set(
    "idempotencyKey",
    crypto.randomUUID()
);
```

Quando `crypto.randomUUID()` não estiver disponível no Postman utilizado, implementar alternativa compatível.

---

# 12. Testes Postman

Cada request importante deverá validar:

* status;
* content type;
* tempo máximo razoável;
* presença de `traceId` em erros;
* campos obrigatórios;
* tenant;
* paginação;
* idempotência;
* ausência de secrets.

Exemplo:

```javascript
pm.test("Status deve ser 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Resposta deve ser JSON", function () {
    pm.expect(
        pm.response.headers.get("Content-Type")
    ).to.include("application/json");
});
```

---

# 13. Fluxo E2E administrativo principal

Criar pasta Postman:

```text
18 — E2E Flows / 01 — Onboarding de clínica
```

Passo a passo:

```text
1. Health check
2. Login como PlatformAdmin
3. Criar tenant
4. Criar clínica
5. Criar primeira unidade
6. Criar ClinicAdmin
7. Criar permissões ou associações necessárias
8. Configurar horários
9. Criar especialidade
10. Criar profissional
11. Vincular profissional à unidade
12. Vincular profissional à especialidade
13. Configurar disponibilidade
14. Criar integração WhatsApp desabilitada
15. Validar dados do tenant
16. Login como ClinicAdmin
17. Consultar dashboard inicial
```

Cada request deverá salvar os IDs necessários para o próximo passo.

---

# 14. Clientes administrativos

O termo “cliente” deverá ser definido claramente.

No sistema, distinguir:

```text
Tenant
Clinic
ClinicUnit
Patient
User
```

Não utilizar genericamente “cliente” em código ou documentação quando a entidade correta for conhecida.

Para onboarding de uma nova empresa, utilizar:

```text
Tenant + Clinic + ClinicAdmin
```

Para pessoa atendida pela clínica, utilizar:

```text
Patient
```

---

# 15. Endpoints administrativos para tenant

Verificar se existem endpoints equivalentes a:

```text
GET    /api/v1/admin/tenants
GET    /api/v1/admin/tenants/{tenantId}
POST   /api/v1/admin/tenants
PUT    /api/v1/admin/tenants/{tenantId}
POST   /api/v1/admin/tenants/{tenantId}/activate
POST   /api/v1/admin/tenants/{tenantId}/suspend
POST   /api/v1/admin/tenants/{tenantId}/deactivate
```

Caso não existam:

* documentar como pendentes;
* propor contratos;
* não inventar implementação dentro da documentação;
* criar um plano de implementação separado.

---

# 16. Formulário de tenant

Definir contrato administrativo:

```text
Nome
Slug
Timezone
Locale
Status
Plano
Data de início
Limites operacionais
Nome da clínica
Documento
Email
Telefone
Nome do administrador
Email do administrador
```

Separar dados do tenant, clínica e usuário no backend, mesmo que o frontend use um wizard único.

---

# 17. Wizard de onboarding

Planejar formulário em etapas:

## Etapa 1 — Empresa

* nome do tenant;
* slug;
* timezone;
* locale;
* plano.

## Etapa 2 — Clínica

* razão social;
* nome fantasia;
* documento;
* email;
* telefone.

## Etapa 3 — Unidade inicial

* nome;
* endereço;
* telefone;
* horário.

## Etapa 4 — Administrador

* nome;
* email;
* role;
* convite ou senha temporária.

## Etapa 5 — Integração

* provider;
* sender;
* ativação posterior;
* status.

## Etapa 6 — Revisão

* resumo;
* confirmação;
* criação.

A criação deverá ocorrer por endpoint transacional ou workflow controlado.

---

# 18. Fluxo de criação de paciente

Criar pasta Postman:

```text
18 — E2E Flows / 02 — Cadastro e atualização de paciente
```

Passos:

```text
1. Login como ClinicAdmin ou Receptionist
2. Buscar paciente por telefone
3. Confirmar que não existe
4. Criar paciente
5. Consultar detalhes
6. Atualizar dados administrativos
7. Consultar novamente
8. Validar isolamento de tenant
```

Endpoints esperados:

```text
GET  /api/v1/admin/patients
GET  /api/v1/admin/patients/{patientId}
POST /api/v1/admin/patients
PUT  /api/v1/admin/patients/{patientId}
```

Documentar endpoints ausentes como pendentes.

---

# 19. Formulário de paciente

Campos administrativos:

```text
Nome
Telefone
Email
Data de nascimento
Consentimento de comunicação
Origem
Observação administrativa não clínica
Status
```

Não incluir:

* diagnóstico;
* sintomas;
* prontuário;
* prescrição;
* informação médica desnecessária.

---

# 20. Fluxo de profissionais e especialidades

Criar fluxo:

```text
1. Criar especialidade
2. Criar profissional
3. Vincular especialidade
4. Vincular unidade
5. Configurar disponibilidade
6. Consultar horários
7. Atualizar profissional
8. Desativar e reativar
```

Documentar requests, responses, IDs e erros.

---

# 21. Fluxo de agendamento

Criar pasta Postman:

```text
18 — E2E Flows / 03 — Agendamento completo
```

Passos:

```text
1. Login
2. Obter tenant
3. Buscar ou criar paciente
4. Listar especialidades
5. Listar profissionais
6. Listar unidades
7. Consultar disponibilidade
8. Selecionar slot
9. Criar agendamento
10. Consultar agendamento
11. Confirmar consulta
12. Reagendar
13. Consultar horário atualizado
14. Cancelar
15. Consultar estado final
16. Consultar auditoria
```

Utilizar `Idempotency-Key` em operações críticas.

---

# 22. Fluxo de conversas

Criar pasta:

```text
18 — E2E Flows / 04 — Conversa e atendimento humano
```

Passos:

```text
1. Simular mensagem inbound
2. Listar conversas
3. Abrir conversa
4. Consultar mensagens
5. Solicitar handoff
6. Consultar fila
7. Assumir conversa
8. Pausar automação
9. Enviar mensagem manual
10. Consultar status
11. Transferir
12. Liberar
13. Assumir novamente
14. Retomar automação
15. Encerrar
16. Reabrir
17. Consultar auditoria
```

---

# 23. Fluxo WhatsApp

Criar:

```text
18 — E2E Flows / 05 — WhatsApp
```

Separar:

## Fake

* validar integração;
* simular inbound;
* enviar mensagem;
* simular status;
* testar falha;
* consultar histórico.

## Twilio Sandbox

* validar integração;
* mensagem de teste;
* callback;
* status;
* erro sanitizado.

Não incluir segredo no Postman.

---

# 24. Fluxo dashboard

Criar:

```text
1. Preparar seed
2. Consultar dashboard sem filtros
3. Consultar por período
4. Consultar por unidade
5. Consultar por fila
6. Comparar métricas com dados esperados
```

Adicionar testes Postman para números mínimos e consistência básica.

---

# 25. Documentação dos endpoints

Para cada domínio, criar tabela:

| Método | Endpoint | Permissão | Idempotência | Descrição |
| ------ | -------- | --------- | ------------ | --------- |

Não copiar todos os schemas quando OpenAPI já os contém.

Adicionar exemplos apenas para operações importantes.

---

# 26. Formulários administrativos

Criar documento:

```text
docs/operations/administrative-forms.md
```

Descrever formulários necessários para:

* tenant;
* clínica;
* unidade;
* usuário;
* paciente;
* profissional;
* especialidade;
* disponibilidade;
* agendamento;
* integração WhatsApp;
* template;
* configurações.

Para cada formulário, informar:

```text
Campos
Obrigatoriedade
Validação
Permissão
Endpoint
Método
Erros esperados
Ação após sucesso
```

---

# 27. Endpoints ausentes

Criar documento:

```text
docs/api/missing-administrative-endpoints.md
```

Classificar:

```text
Blocker
High
Medium
Low
```

Para cada endpoint ausente:

```text
Nome
Objetivo
Perfil
Request
Response
Regras
Erros
Idempotência
Auditoria
Evento
Frontend dependente
```

Não implementar endpoints nesta tarefa, salvo se solicitado separadamente.

---

# 28. Sincronização OpenAPI e Postman

Avaliar uma estratégia de geração:

```text
OpenAPI
   ↓
Postman base
   ↓
Pastas E2E manuais
```

A collection gerada não deve apagar scripts operacionais personalizados.

Separar:

```text
Generated Requests
E2E Flows
```

Documentar como regenerar.

Criar comando, quando aplicável:

```bash
npm run postman:generate
```

ou script equivalente.

Não adicionar ferramenta desnecessária sem avaliar o projeto.

---

# 29. Validação automática

Criar comandos ou scripts para:

* verificar links Markdown;
* detectar arquivos sem referência;
* validar JSON do Postman;
* comparar endpoints OpenAPI e Postman;
* detectar variáveis não definidas;
* detectar secrets;
* detectar URLs de localhost fixas;
* detectar tokens versionados.

Exemplo de comandos:

```bash
./scripts/docs/validate-docs.sh
./scripts/postman/validate-collection.sh
./scripts/postman/check-openapi-drift.sh
```

---

# 30. CI

Adicionar etapa de CI:

```text
Validate Markdown
Validate links
Validate Postman JSON
Validate environments
Detect secrets
Compare OpenAPI and Postman
```

Falhar quando:

* JSON estiver inválido;
* link interno estiver quebrado;
* variável obrigatória não estiver documentada;
* secret for detectado;
* endpoint removido continuar no Postman;
* request Postman usar URL fixa indevida.

---

# 31. Arquivamento

Documentos obsoletos que ainda possuam valor histórico deverão ser movidos para:

```text
docs/archive/
```

Adicionar no topo:

```text
Status: Archived
Replaced by: caminho/do/documento.md
Archived at: YYYY-MM-DD
```

Documentos sem valor deverão ser removidos somente após a consolidação e validação.

---

# 32. Critérios de aceite

A etapa estará concluída quando:

```text
1. Todos os Markdown forem inventariados
2. Redundâncias forem identificadas
3. Fontes únicas forem definidas
4. Estrutura docs estiver organizada
5. Índice principal estiver criado
6. Documentos duplicados forem consolidados
7. Documentos obsoletos forem arquivados ou removidos
8. OpenAPI estiver atualizado
9. Collection Postman estiver atualizada
10. Environments estiverem atualizados
11. Nenhum secret estiver presente
12. Login Postman salvar token
13. IDs forem propagados automaticamente
14. Fluxo de onboarding estiver documentado
15. Fluxo de paciente estiver documentado
16. Fluxo de profissional estiver documentado
17. Fluxo de agendamento estiver documentado
18. Fluxo de conversa estiver documentado
19. Fluxo WhatsApp estiver documentado
20. Fluxo de dashboard estiver documentado
21. Formulários administrativos estiverem especificados
22. Endpoints ausentes estiverem inventariados
23. Links Markdown estiverem válidos
24. JSON Postman estiver válido
25. Drift entre OpenAPI e Postman for verificado
26. Checklist de documentação estiver criado
27. Template de PR estiver atualizado quando existente
28. CI validar documentação
29. Documentação possuir passos copiáveis
30. Documentação permitir execução E2E manual
```

---

# 33. Ordem de implementação

## 10.1 Inventário

* Markdown;
* OpenAPI;
* Postman;
* scripts;
* endpoints;
* redundâncias;
* lacunas.

## 10.2 Arquitetura documental

* fontes únicas;
* estrutura;
* índice;
* política de atualização.

## 10.3 Consolidação

* mesclar;
* reescrever;
* mover;
* arquivar;
* corrigir links.

## 10.4 Postman

* collection;
* environments;
* variáveis;
* scripts;
* testes.

## 10.5 Fluxos E2E

* onboarding;
* paciente;
* profissionais;
* agenda;
* conversa;
* WhatsApp;
* dashboard.

## 10.6 Administração

* formulários;
* endpoints existentes;
* endpoints ausentes;
* permissões.

## 10.7 Automação

* validação;
* drift;
* links;
* secrets;
* CI.

---

# 34. Primeira entrega

Implemente inicialmente somente:

```text
10.1 — Inventário
10.2 — Arquitetura documental
10.3 — Consolidação da documentação principal
10.4 — Estrutura inicial do Postman
```

A primeira entrega deverá conter:

```text
1. Relatório de inventário
2. Matriz de redundâncias
3. Fontes únicas de verdade
4. Nova estrutura docs
5. docs/README.md
6. Documentos principais consolidados
7. Arquivo de documentos arquivados
8. Collection Postman consolidada
9. Environment Local
10. Environment E2E
11. README do Postman
12. Validação inicial de JSON
13. Lista de endpoints ausentes
14. Lista de formulários necessários
15. Checklist de atualização
```

Não avançar para fluxos E2E completos antes de:

* validar a nova estrutura;
* garantir que nenhum conteúdo relevante foi perdido;
* garantir que OpenAPI e Postman refletem os endpoints reais;
* garantir que não existem secrets;
* validar os links.

---

# 35. Validação final

Executar os comandos existentes do projeto para:

```text
build
test
OpenAPI generation
Postman validation
Markdown validation
link validation
secret scanning
```

Caso os scripts ainda não existam, criar somente scripts simples e documentados.

Ao finalizar, apresentar:

1. documentos criados;
2. documentos mesclados;
3. documentos arquivados;
4. documentos removidos;
5. links corrigidos;
6. endpoints adicionados ao Postman;
7. endpoints ausentes;
8. variáveis Postman;
9. fluxos documentados;
10. formulários especificados;
11. validações executadas;
12. inconsistências restantes;
13. próximos passos.
