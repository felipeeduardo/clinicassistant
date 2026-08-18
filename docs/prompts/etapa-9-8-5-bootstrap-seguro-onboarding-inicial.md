# Etapa 9.8.5 — Bootstrap Seguro da Plataforma e Onboarding Inicial de Clínicas

## 0. Contexto

O MVP **IA Recepção** já possui ambiente de produção em evolução, backend ASP.NET Core/.NET, persistência relacional e administração multi-tenant/plataforma.

Precisamos melhorar a experiência de **primeira inicialização** quando o banco de dados ainda está vazio.

A ideia inicial é permitir que dois operadores previamente autorizados tenham papel `PlatformAdmin` e, a partir da área administrativa, consigam realizar todo o onboarding inicial de uma clínica sem depender de scripts SQL manuais.

### IMPORTANTE — decisão de segurança

NÃO inserir senha real fixa em migration, source code, snapshot do Entity Framework, seed versionado, `appsettings`, documentação ou Git.

A credencial inicial fornecida pelo solicitante deve ser tratada como **secret externo** e não deve aparecer em nenhum artefato versionado.

A migration pode criar **estrutura/dados não secretos**, mas o bootstrap dos usuários administrativos deve ocorrer por um mecanismo idempotente de inicialização que:

- utilize configuração/secrets externos;
- utilize o `UserManager`/Identity ou serviço oficial de usuários do projeto;
- gere o hash usando o mecanismo atual;
- nunca persista senha em texto puro;
- nunca escreva a senha nos logs;
- possa ser desabilitado após o primeiro bootstrap.

Se a arquitetura atual não usa ASP.NET Identity, adaptar ao mecanismo real de autenticação existente, mantendo os mesmos princípios.

---

# 1. Objetivo de negócio

Quando uma instalação nova da IA Recepção for iniciada com banco vazio, queremos transformar a experiência:

```text
Banco vazio
    ↓
Migrations estruturais
    ↓
Bootstrap seguro dos PlatformAdmins
    ↓
Login do PlatformAdmin
    ↓
Wizard de onboarding
    ↓
Criação da clínica
    ↓
Unidade inicial
    ↓
Especialidades
    ↓
Profissionais
    ↓
Horários/disponibilidade
    ↓
Usuários da clínica
    ↓
Configuração WhatsApp
    ↓
Validação
    ↓
Clínica pronta para operar
```

O objetivo é eliminar a necessidade de:

- INSERT manual no banco;
- alteração manual de roles;
- criação manual de tenant;
- configuração inicial espalhada por várias telas;
- scripts de produção contendo senha;
- dependência do desenvolvedor para ativar cada clínica.

---

# 2. Princípio arquitetural

Separar claramente:

## Schema/Data Migration

Responsável por:

- schema;
- índices;
- constraints;
- roles/perfis estáticos, se apropriado;
- dados de referência não secretos.

## Bootstrap Administrativo

Responsável por:

- criação idempotente dos PlatformAdmins iniciais;
- atribuição do papel correto;
- inicialização segura a partir de secrets;
- auditoria.

## Onboarding da Clínica

Responsável por:

- tenant/clínica;
- unidades;
- usuários administrativos;
- profissionais;
- especialidades;
- disponibilidade;
- configurações operacionais;
- integração WhatsApp;
- readiness.

NÃO misturar essas três responsabilidades em uma migration contendo credenciais.

---

# 3. Auditoria obrigatória antes de alterar

Antes de implementar, analisar o repositório e identificar:

- modelo `User`;
- mecanismo de autenticação;
- password hashing;
- roles;
- `PlatformAdmin`;
- `ClinicAdmin`;
- `Receptionist`;
- `Professional`;
- tenant/clinic isolation;
- entidades `Clinic`, `Unit`, `Professional`, `Specialty`, `Patient`;
- onboarding existente;
- APIs de platform administration;
- telas administrativas;
- migrations EF Core;
- seed atual;
- startup;
- dependency injection;
- audit trail;
- idempotency;
- authorization policies;
- feature flags;
- environment variables;
- E2E;
- Postman;
- documentação.

Produzir antes da implementação:

| Área | Implementação atual | Pode ser reutilizada? | Gap | Ação |
|---|---|---:|---|---|

Não criar fluxo paralelo se já existir serviço equivalente.

---

# 4. PlatformAdmins iniciais

Precisamos suportar bootstrap de dois PlatformAdmins autorizados.

As identidades iniciais devem ser configuráveis externamente.

Conceitualmente:

```text
PlatformBootstrap__Enabled=true
PlatformBootstrap__Admins__0__Email=<email-1>
PlatformBootstrap__Admins__0__Password=<secret>
PlatformBootstrap__Admins__1__Email=<email-2>
PlatformBootstrap__Admins__1__Password=<secret>
```

ADAPTAR à convenção real do projeto.

Não assumir obrigatoriamente arrays em environment variables se houver padrão melhor já utilizado.

### Requisitos

Para cada administrador:

1. normalizar e-mail;
2. verificar se já existe;
3. se não existir, criar pelo serviço oficial;
4. aplicar password hashing oficial;
5. atribuir `PlatformAdmin`;
6. marcar estado inicial conforme modelo atual;
7. gerar auditoria;
8. não duplicar se reiniciar a aplicação.

---

# 5. Idempotência

O bootstrap deve ser seguro em múltiplos restarts.

Resultado esperado:

```text
Primeiro startup:
2 PlatformAdmins criados

Segundo startup:
0 usuários duplicados

Terceiro startup:
0 alterações desnecessárias
```

Não utilizar apenas:

```text
if database.Users.Count() == 0
```

como regra global.

Isso é frágil.

Verificar cada identidade individualmente por chave única/e-mail normalizado.

---

# 6. Banco não totalmente vazio

O bootstrap também deve funcionar se:

- migrations já foram executadas;
- existem dados técnicos;
- existem roles;
- existe apenas um dos PlatformAdmins;
- existe usuário comum com um dos e-mails.

Tratar cada cenário explicitamente.

Se um e-mail já existir com papel incompatível:

NÃO elevar privilégio silenciosamente.

Registrar erro seguro e exigir ação administrativa/manual.

Privilege escalation automática é proibida.

---

# 7. Bootstrap Enabled

Adicionar feature/config flag:

```text
PlatformBootstrap:Enabled
```

ou equivalente.

Comportamento:

### Development
Pode ser habilitado explicitamente.

### Test/E2E
Pode usar credenciais fake controladas.

### Production
Habilitado apenas durante bootstrap inicial, conforme estratégia operacional.

Depois da inicialização:

```text
PlatformBootstrap__Enabled=false
```

deve ser recomendado.

---

# 8. Senha inicial

A senha NÃO pode existir no repositório.

O Codex deve:

- não copiar a senha fornecida pelo solicitante;
- não criar constante;
- não adicionar em `.env.example`;
- não adicionar em migration;
- não adicionar em testes;
- não adicionar em README;
- não imprimir no console.

Criar placeholder:

```text
<SET_IN_SECRET_STORE>
```

nos exemplos documentais.

---

# 9. Política de senha

Usar a política já existente.

Não reduzir complexidade para aceitar a senha inicial.

Se a credencial configurada não atender à política:

- bootstrap falha de forma explícita;
- informa qual regra não foi atendida;
- nunca imprime a senha.

---

# 10. Rotação da credencial inicial

Recomendar que a senha de bootstrap seja temporária.

Se o modelo atual suportar:

```text
MustChangePassword
PasswordChangeRequired
TemporaryPassword
```

reutilizar.

Se NÃO existir, não criar uma arquitetura grande apenas para esta feature.

Avaliar a implementação mínima segura e documentar.

Idealmente, o primeiro login deve conduzir à troca de senha quando compatível com a arquitetura atual.

---

# 11. Migration

Criar migration apenas para alterações estruturais realmente necessárias.

Exemplos possíveis:

- coluna `OnboardingStatus`;
- tabela de onboarding;
- índices;
- constraints;
- campos de bootstrap/auditoria.

NÃO criar migration com:

```csharp
InsertData(
    email: "...",
    password: "..."
)
```

NÃO armazenar password hash estático em migration.

Mesmo hash não deve ser versionado como credencial bootstrap.

---

# 12. Serviço de bootstrap

Preferência conceitual:

```text
IPlatformBootstrapService
PlatformBootstrapService
PlatformBootstrapOptions
PlatformBootstrapOptionsValidator
```

Somente criar se coerente com a arquitetura atual.

Responsabilidades:

```text
Validate configuration
↓
Check bootstrap enabled
↓
Ensure required roles/reference data
↓
Check each configured admin
↓
Create missing admin safely
↓
Assign PlatformAdmin
↓
Audit
↓
Return bootstrap result
```

---

# 13. Startup

Integrar bootstrap ao startup de maneira controlada.

Ordem conceitual:

```text
Database available
↓
Migrations applied/validated
↓
Bootstrap
↓
Application ready
```

Não gerar race condition entre múltiplas instâncias.

Se a produção puder iniciar mais de uma réplica simultaneamente, implementar proteção apropriada:

- unique constraint;
- transaction;
- retry de conflito;
- distributed lock somente se realmente necessário.

Não adicionar infraestrutura excessiva se unique constraints + tratamento transacional resolverem.

---

# 14. Falha do bootstrap

Diferenciar:

## Configuração inválida
Fail fast.

Exemplo:

- Enabled=true sem admin;
- e-mail inválido;
- password ausente;
- role inexistente sem mecanismo de criação.

## Dependência temporariamente indisponível
Seguir política de resiliência existente.

## Admin já existe
Idempotent success/no-op.

## Usuário existe mas não é PlatformAdmin
Falha segura; não elevar automaticamente.

---

# 15. Auditoria

Registrar eventos como:

```text
PlatformBootstrapStarted
PlatformAdminCreated
PlatformAdminAlreadyExists
PlatformBootstrapCompleted
PlatformBootstrapFailed
```

Nunca incluir password.

Registrar quando apropriado:

- timestamp;
- actor/system;
- user id;
- normalized email ou representação segura;
- correlation id;
- outcome.

Seguir política de PII existente.

---

# 16. PlatformAdmin authorization

Confirmar que `PlatformAdmin` é realmente uma role/policy global.

O PlatformAdmin deve poder administrar a plataforma.

NÃO deve ser automaticamente tratado como funcionário de todas as clínicas se o modelo não exigir.

Preservar isolamento multi-tenant.

---

# 17. Nova experiência após login

Após login de PlatformAdmin, fornecer uma área clara:

```text
Administração da Plataforma
```

com:

- Clínicas;
- Usuários;
- Onboarding;
- Saúde/integrações quando autorizado;
- Auditoria;
- configurações globais apropriadas.

Reutilizar telas existentes sempre que possível.

---

# 18. Dashboard de PlatformAdmin

Criar/refinar dashboard inicial com informações úteis:

- total de clínicas;
- clínicas em onboarding;
- clínicas ativas;
- clínicas com configuração incompleta;
- usuários;
- falhas recentes de onboarding;
- integrações que exigem atenção.

Não expor dados clínicos/pacientes desnecessariamente.

---

# 19. CTA principal

Quando ainda não houver clínica cadastrada, apresentar estado vazio amigável.

Exemplo conceitual:

```text
Nenhuma clínica configurada ainda.

Configure sua primeira clínica para começar a operar a IA Recepção.

[Configurar primeira clínica]
```

Evitar dashboard quebrado ou tabelas vazias sem orientação.

---

# 20. Wizard de onboarding

Criar/refinar um wizard transacional e progressivo.

Sugestão:

```text
1. Clínica
2. Unidade
3. Especialidades
4. Profissionais
5. Disponibilidade
6. Usuários e acessos
7. WhatsApp
8. Revisão
9. Ativação
```

Adaptar ao domínio real existente.

---

# 21. Passo 1 — Clínica

Formulário deve permitir os campos já existentes e necessários.

Exemplos:

- razão/nome;
- nome de exibição;
- documento, se modelado;
- telefone;
- e-mail;
- timezone;
- status;
- dados administrativos.

Não inventar campos de domínio sem necessidade.

---

# 22. Passo 2 — Unidade

Permitir configurar a primeira unidade:

- nome;
- endereço, se existente;
- telefone;
- timezone quando aplicável;
- horários de funcionamento;
- ativo/inativo.

Permitir múltiplas unidades posteriormente.

---

# 23. Passo 3 — Especialidades

Permitir:

- cadastrar;
- editar;
- ativar;
- selecionar especialidades iniciais.

Reutilizar endpoints existentes.

Não duplicar catálogo se já houver.

---

# 24. Passo 4 — Profissionais

Permitir:

- dados essenciais;
- especialidades;
- unidade;
- status;
- identificação profissional existente no modelo;
- vínculo com agenda.

Não exigir todos os campos opcionais no onboarding.

---

# 25. Passo 5 — Disponibilidade

Configurar:

- dias de atendimento;
- horários;
- duração padrão;
- intervalos;
- bloqueios iniciais quando aplicável.

O objetivo é sair do onboarding com disponibilidade real para consulta.

---

# 26. Passo 6 — Usuários e acessos

O PlatformAdmin deve poder criar o primeiro `ClinicAdmin`.

Depois, o ClinicAdmin deverá conseguir administrar os usuários permitidos da própria clínica.

Papéis possíveis, conforme existentes:

```text
ClinicAdmin
Receptionist
Professional
```

Não permitir que ClinicAdmin crie PlatformAdmin.

---

# 27. Criação de ClinicAdmin

Criar formulário seguro.

Campos conforme modelo atual:

- nome;
- e-mail;
- role;
- clínica;
- unidade quando necessário.

Para senha:

preferir fluxo de convite/definição de senha se já existir.

Se não existir, usar mecanismo temporário seguro.

Não mostrar senha persistida depois.

---

# 28. Authorization matrix

Revisar backend, não apenas frontend.

Esperado:

| Operação | PlatformAdmin | ClinicAdmin | Receptionist | Professional |
|---|---:|---:|---:|---:|
| Criar clínica | Sim | Não | Não | Não |
| Criar PlatformAdmin | Conforme política explícita | Não | Não | Não |
| Criar ClinicAdmin | Sim | Conforme regras da própria clínica | Não | Não |
| Gerenciar unidade | Sim | Própria clínica | Conforme política atual | Não |
| Gerenciar profissional | Sim | Própria clínica | Conforme política atual | Não |
| Alterar tenant | Sim | Não | Não | Não |

Adaptar às policies reais.

Testar no backend.

---

# 29. Passo 7 — WhatsApp

Não exigir secret Twilio em formulário comum sem revisar a arquitetura de secrets.

O wizard deve distinguir:

```text
Configuração funcional
```

de:

```text
Credenciais secretas de infraestrutura
```

Se o projeto já possui administração segura de Twilio por tenant, reutilizar.

Caso contrário, NÃO salvar `AuthToken` em texto puro no banco apenas para completar o wizard.

---

# 30. WhatsApp readiness

Mostrar status, por exemplo:

```text
Não configurado
Configuração parcial
Pronto para teste
Ativo
Erro
```

Permitir teste controlado usando os mecanismos existentes.

Não disparar mensagens reais automaticamente.

---

# 31. Passo 8 — Revisão

Mostrar checklist:

```text
✓ Clínica
✓ Unidade
✓ Especialidade
✓ Profissional
✓ Disponibilidade
✓ ClinicAdmin
○ WhatsApp
```

Separar:

- itens obrigatórios para ativação;
- itens opcionais;
- itens pendentes.

---

# 32. Ativação

Definir regra de readiness no backend.

Não depender apenas de botão frontend.

Exemplo conceitual:

```text
Clinic can activate when:

Clinic exists
AND active unit exists
AND active professional exists
AND specialty exists
AND availability exists
AND ClinicAdmin exists
```

WhatsApp pode ser obrigatório ou não conforme decisão atual do produto.

Não inventar regra comercial sem registrar decisão.

---

# 33. OnboardingStatus

Avaliar necessidade de status persistido.

Exemplo:

```text
NotStarted
InProgress
ReadyForActivation
Active
Blocked
```

Somente adicionar se melhorar o domínio e não duplicar status já existente.

---

# 34. Progresso

O onboarding deve poder ser interrompido e retomado.

Não exigir preenchimento de tudo em uma única sessão.

Persistir progresso de forma segura.

---

# 35. Transações

Não fazer uma única transação de banco longa cobrindo todo o wizard.

Cada passo deve ser consistente/idempotente.

Operações compostas críticas podem utilizar transação local apropriada.

---

# 36. ExpectedVersion / concorrência

Se o projeto já usa optimistic concurrency/`expectedVersion`, aplicar aos updates relevantes.

Não remover mecanismos existentes.

---

# 37. Idempotency-Key

Para operações críticas de criação/ativação, reutilizar `Idempotency-Key` se já for padrão.

Especialmente:

- criar clínica;
- criar admin;
- ativar;
- integração que possa ser repetida.

---

# 38. UX

O wizard deve ser:

- moderno;
- responsivo;
- consistente com a identidade IA Recepção;
- com os tons azuis já adotados;
- acessível;
- com feedback claro;
- com loading;
- error state;
- empty state;
- success state;
- progress indicator.

---

# 39. Mobile

Validar:

- desktop;
- tablet;
- mobile.

No mobile, usar stepper adaptado sem overflow horizontal quebrado.

---

# 40. Validação frontend

Cada passo deve validar antes de avançar.

Erros do backend devem ser traduzidos para feedback útil.

Não esconder erro real com apenas:

```text
Não foi possível concluir.
```

Quando seguro, informar ação necessária.

---

# 41. APIs

Antes de criar endpoints novos, inventariar os existentes.

Reutilizar:

- platform administration;
- clinics;
- units;
- specialties;
- professionals;
- availability;
- users;
- WhatsApp;
- audit.

Criar endpoints novos apenas para gaps reais.

---

# 42. Endpoint de onboarding

Se fizer sentido, criar um endpoint agregador de status, por exemplo:

```text
GET /platform/onboarding/{clinicId}
```

Retornando apenas dados necessários:

```text
clinicConfigured
unitConfigured
specialtiesConfigured
professionalsConfigured
availabilityConfigured
clinicAdminConfigured
whatsAppConfigured
canActivate
```

Adaptar naming ao projeto.

---

# 43. Não criar mega-endpoint

Evitar:

```text
POST /create-entire-clinic
```

com dezenas de entidades e efeitos colaterais.

O onboarding deve orquestrar APIs de domínio consistentes.

---

# 44. E-mail/invite

Se existir serviço de e-mail, avaliar convite para ClinicAdmin.

Não bloquear a feature caso e-mail ainda não esteja pronto, se houver mecanismo temporário seguro.

Documentar fallback.

---

# 45. PlatformAdmin management

Depois do bootstrap, avaliar uma tela para gerenciar PlatformAdmins.

Por segurança:

- somente PlatformAdmin autorizado;
- impedir remoção acidental do último PlatformAdmin ativo;
- auditoria obrigatória;
- confirmação reforçada.

Não é necessário criar se estiver fora do escopo atual; documentar gap.

---

# 46. Proteção do último PlatformAdmin

Se houver API para desativar/remover PlatformAdmin:

não permitir que o sistema fique sem nenhum PlatformAdmin ativo.

Adicionar regra/teste se aplicável.

---

# 47. Bootstrap depois do onboarding

Após criação bem-sucedida dos dois administradores iniciais, a operação recomendada é:

```text
PlatformBootstrap__Enabled=false
```

Documentar.

Não apagar automaticamente os usuários.

---

# 48. Rotação

Criar checklist operacional:

```text
[ ] PlatformAdmins criados
[ ] login validado
[ ] senha temporária rotacionada, quando aplicável
[ ] bootstrap desabilitado
[ ] secrets de bootstrap removidos/rotacionados
[ ] auditoria verificada
```

---

# 49. Environment variables

Criar/atualizar documentação de variáveis, sem valores.

Exemplo conceitual:

```text
PlatformBootstrap__Enabled
PlatformBootstrap__Admins__0__Email
PlatformBootstrap__Admins__0__Password
PlatformBootstrap__Admins__1__Email
PlatformBootstrap__Admins__1__Password
```

Usar convenção real.

Marcar password como SECRET.

---

# 50. Railway

Preparar instruções para configurar as credenciais iniciais no secret store da Railway.

Não hardcode.

Não executar alteração remota silenciosamente.

Se o Codex não puder configurar Railway:

`MANUAL ACTION REQUIRED`.

---

# 51. Desenvolvimento local

Local deve continuar funcionando.

Para Development:

- bootstrap default desabilitado, preferencialmente;
- permitir habilitação explícita;
- utilizar usuários fake, nunca credenciais reais;
- não depender de CloudAMQP/Twilio para testar onboarding básico.

---

# 52. Testes unitários — bootstrap

Cobrir:

1. Enabled=false → nenhuma criação;
2. Enabled=true + configuração válida → cria admins;
3. restart → não duplica;
4. apenas admin A existe → cria B;
5. ambos existem → no-op;
6. e-mail inválido → falha de config;
7. password ausente → falha;
8. usuário existe sem PlatformAdmin → não eleva;
9. password não atende policy → falha;
10. logs não contêm password.

---

# 53. Testes de concorrência

Se relevante:

duas instâncias executam bootstrap simultaneamente.

Resultado:

- somente um usuário por e-mail;
- sem duplicação;
- sem role duplicada;
- comportamento consistente.

---

# 54. Testes de autorização

Testar:

- PlatformAdmin cria clínica;
- ClinicAdmin não cria tenant global;
- Receptionist não cria clínica;
- Professional não cria clínica;
- ClinicAdmin não cria PlatformAdmin;
- isolamento entre clínicas.

Não confiar apenas na UI.

---

# 55. Testes do onboarding

Cobrir:

```text
PlatformAdmin login
→ create clinic
→ create unit
→ create specialty
→ create professional
→ configure availability
→ create ClinicAdmin
→ review
→ activate
```

---

# 56. E2E Playwright

Criar/refinar cenário E2E usando credenciais FAKE de teste.

Nunca usar os e-mails/senha reais de bootstrap em CI.

Cenário:

```text
fresh seeded test database
→ bootstrap fake PlatformAdmin
→ login
→ empty state
→ start onboarding
→ clinic
→ unit
→ specialty
→ professional
→ availability
→ ClinicAdmin
→ review
→ activation
→ dashboard
```

---

# 57. Teste de retomada

E2E:

```text
complete steps 1-3
→ logout
→ login
→ resume
→ continue at correct point
```

---

# 58. Teste de erro

Simular:

- e-mail duplicado;
- conflito;
- API 409;
- validation 400;
- integration unavailable.

UI deve permanecer consistente.

---

# 59. Auditoria E2E

Verificar eventos de:

- bootstrap;
- clinic creation;
- admin creation;
- activation.

Não verificar passwords.

---

# 60. Banco vazio

Criar teste de integração que realmente parta de database novo/migrations.

Validar:

```text
migrate
→ bootstrap
→ 2 configured test admins
→ roles correct
```

Usar usuários fake.

---

# 61. Banco já populado

Testar migration/bootstrap em banco com dados existentes.

Garantir que deploy dessa feature não destrua nem modifique usuários atuais.

---

# 62. Migration safety

Executar:

```text
dotnet ef migrations script
```

ou comando equivalente real do projeto.

Inspecionar SQL.

Garantir ausência de:

- senha;
- e-mails reais desnecessários na migration;
- DELETE;
- UPDATE global perigoso;
- privilege escalation.

---

# 63. Segurança

Auditar:

- secrets;
- auth;
- authorization;
- tenant isolation;
- password handling;
- logs;
- audit;
- CSRF quando aplicável;
- rate limiting;
- invite/reset flows.

---

# 64. Observabilidade

Adicionar métricas/eventos somente se o projeto já possuir infraestrutura adequada.

Úteis:

```text
platform_bootstrap_success
platform_bootstrap_failure
clinic_onboarding_started
clinic_onboarding_completed
clinic_onboarding_failed
```

Sem PII.

---

# 65. Health/readiness

Bootstrap não deve ficar recriando usuários em healthcheck.

Health endpoint nunca deve disparar bootstrap.

Readiness pode refletir erro crítico de configuração, conforme arquitetura atual.

---

# 66. Postman

Atualizar collection para incluir:

- PlatformAdmin login;
- platform administration;
- clinic creation;
- unit;
- specialty;
- professional;
- availability;
- ClinicAdmin;
- onboarding status;
- activation;
- audit.

Usar variáveis fake.

---

# 67. OpenAPI

Atualizar OpenAPI/documentação para endpoints novos.

Não expor propriedades secretas de bootstrap em APIs.

Bootstrap config não deve ser editável por endpoint público.

---

# 68. Documentação

Criar/atualizar:

```text
docs/platform/platform-bootstrap.md
docs/platform/clinic-onboarding.md
docs/security/bootstrap-secrets.md
docs/operations/first-production-bootstrap.md
docs/api/platform-administration.md
docs/testing/onboarding-e2e.md
docs/deployment/production-env-matrix.md
```

---

# 69. Runbook do primeiro bootstrap

Criar:

`docs/operations/first-production-bootstrap.md`

Fluxo:

```text
1. migrations
2. configure bootstrap secrets
3. enable bootstrap
4. deploy/restart controlled instance
5. inspect logs
6. verify PlatformAdmins
7. login
8. rotate temporary credentials if applicable
9. disable bootstrap
10. remove/rotate bootstrap password secrets
11. execute clinic onboarding
12. validate audit
```

---

# 70. Não colocar identidades sensíveis na documentação

Os e-mails reais solicitados não precisam aparecer nos arquivos versionados.

Usar:

```text
<PLATFORM_ADMIN_EMAIL_1>
<PLATFORM_ADMIN_EMAIL_2>
```

Os valores reais ficam no secret/config store.

---

# 71. UX de primeira execução

Se zero clínicas:

mostrar onboarding.

Se há clínica `InProgress`:

mostrar:

```text
Continuar configuração
```

Se há clínica ativa:

mostrar dashboard normal.

Se há múltiplas clínicas:

mostrar visão de plataforma.

---

# 72. Checklist visual

Na visão de onboarding, apresentar progresso claro:

```text
Configuração da clínica        Concluído
Unidade                        Concluído
Especialidades                 Concluído
Profissionais                  Pendente
Disponibilidade                Pendente
Administrador da clínica       Pendente
WhatsApp                       Não configurado
```

---

# 73. Dados essenciais vs opcionais

Evitar formulário gigantesco.

Classificar campos:

```text
Obrigatório para operar
Recomendado
Pode ser configurado depois
```

O onboarding deve minimizar time-to-value.

---

# 74. Resultado esperado de UX

Queremos sair de:

```text
Deploy
→ banco vazio
→ desenvolvedor cria SQL
→ altera role
→ cria tenant
→ cadastra dados em telas separadas
→ descobre dependências
```

para:

```text
Deploy
→ bootstrap seguro
→ PlatformAdmin login
→ "Configure sua primeira clínica"
→ wizard guiado
→ revisão
→ ativação
→ clínica operacional
```

---

# 75. Critérios de aceite — bootstrap

- nenhum password no Git;
- bootstrap configurável;
- dois admins configuráveis externamente;
- password hashing oficial;
- role PlatformAdmin correta;
- idempotente;
- não eleva usuário existente silenciosamente;
- logs seguros;
- restart seguro;
- bootstrap desabilitável;
- testes passam.

---

# 76. Critérios de aceite — onboarding

- PlatformAdmin possui fluxo claro;
- zero-clinic empty state;
- wizard funcional;
- clinic;
- unit;
- specialty;
- professional;
- availability;
- ClinicAdmin;
- review;
- activation;
- progresso retomável;
- autorização backend;
- auditoria;
- responsividade;
- E2E.

---

# 77. Critérios de aceite — regressão

Executar comandos REAIS existentes no repositório.

Backend:

```text
dotnet restore
dotnet build
dotnet test
```

Frontend, quando existentes:

```text
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1
```

Não inventar scripts.

Validar também:

- RabbitMQ;
- CloudAMQP configuration;
- Twilio;
- Outbox;
- SignalR;
- Agenda;
- Conversas;
- Auth;
- multi-tenant;
- dashboard.

Nenhuma regressão é aceitável.

---

# 78. Não fazer

NÃO:

- colocar a senha fornecida em migration;
- colocar senha em source code;
- colocar senha hash fixa na migration;
- versionar secret;
- criar INSERT SQL manual de password;
- usar `Count()==0` como única proteção;
- elevar role silenciosamente;
- criar PlatformAdmin via endpoint acessível a ClinicAdmin;
- misturar PlatformAdmin com ClinicStaff;
- quebrar multi-tenancy;
- exigir Twilio para concluir todo onboarding sem decisão de produto;
- criar mega-endpoint;
- remover APIs atuais;
- quebrar RabbitMQ/CloudAMQP;
- alterar contratos de mensagens;
- utilizar credenciais reais nos testes.

---

# 79. Relatório final obrigatório

Ao concluir, informar:

1. arquitetura de autenticação encontrada;
2. mecanismo de password hashing;
3. roles encontradas;
4. migrations criadas;
5. serviço de bootstrap;
6. options;
7. validação;
8. idempotência;
9. concorrência;
10. tratamento de usuário preexistente;
11. audit events;
12. authorization matrix;
13. PlatformAdmin UI;
14. empty state;
15. onboarding wizard;
16. passos implementados;
17. ClinicAdmin creation;
18. WhatsApp readiness;
19. activation rule;
20. resume flow;
21. APIs reutilizadas;
22. APIs novas;
23. testes unitários;
24. testes integração;
25. Playwright;
26. Postman;
27. OpenAPI;
28. documentação;
29. env vars;
30. Railway manual actions;
31. security review;
32. regression status;
33. riscos restantes;
34. próximos passos.

---

# 80. Resultado final

A arquitetura desejada é:

```text
NEW DATABASE
      ↓
EF MIGRATIONS
      ↓
SECURE PLATFORM BOOTSTRAP
      ↓
PLATFORM ADMIN LOGIN
      ↓
FIRST-RUN EXPERIENCE
      ↓
CLINIC ONBOARDING WIZARD
      ↓
CLINIC ADMIN
      ↓
OPERATIONAL DATA
      ↓
READINESS CHECK
      ↓
ACTIVATE CLINIC
      ↓
NORMAL OPERATION
```

A implementação deve priorizar:

**segurança + idempotência + boa experiência de onboarding + zero manipulação manual de banco + nenhuma regressão.**
