# Etapa 9.8.6 — Leads Comerciais, Solicitação de Demonstração e Recuperação Segura de Senha

## 0. Contexto

A IA Recepção já possui landing pública, área autenticada, PlatformAdmin, onboarding de clínicas, backend ASP.NET Core/.NET e frontend Next.js/React.

Nesta etapa precisamos fechar duas lacunas importantes para o MVP:

1. **Solicitação de demonstração / captação de leads comerciais**
2. **Fluxo seguro de “Esqueci minha senha” por e-mail**

A implementação deve priorizar simplicidade comercial, segurança, privacidade, boa experiência, auditoria e nenhuma exposição indevida de dados.

Esta etapa NÃO deve transformar a IA Recepção em um CRM completo nem criar automação de marketing complexa.

---

# 1. Objetivo geral

## Fluxo comercial

```text
Landing
↓
Solicitar demonstração
↓
Formulário público
↓
Validação + proteção antiabuso
↓
Lead persistido
↓
PlatformAdmin visualiza
↓
Analisa
↓
Atualiza status
↓
Registra observação
↓
Entra em contato
↓
Agenda demonstração / envia material
↓
Decide evolução comercial
```

## Fluxo de senha

```text
Login
↓
Esqueci minha senha
↓
Usuário informa e-mail
↓
Resposta genérica
↓
Token seguro e temporário
↓
E-mail
↓
Página redefinir senha
↓
Nova senha
↓
Token invalidado
↓
Sessões/tokens antigos tratados
↓
Login novamente
```

---

# 2. Auditoria obrigatória

Antes de implementar, identificar:

## Comercial
- CTA “Solicitar demonstração”;
- formulário existente, se houver;
- endpoint público atual;
- qualquer `mailto`;
- PlatformAdmin dashboard;
- audit log;
- rate limiting;
- proteção anti-spam;
- serviço de e-mail;
- Postman;
- OpenAPI;
- analytics;
- feature flags;
- política de logs/PII.

## Autenticação
- modelo `User`;
- mecanismo de autenticação;
- ASP.NET Identity ou auth própria;
- password hashing;
- reset token existente;
- refresh tokens;
- cookies/JWT;
- sessões;
- serviços de e-mail;
- templates;
- rate limiting;
- audit;
- login page;
- password policy.

Produzir:

| Área | Implementação atual | Reutilizável? | Gap | Ação |
|---|---|---:|---|---|

Não criar implementação paralela se o projeto já possuir serviços equivalentes.

---

# PARTE A — SOLICITAÇÃO DE DEMONSTRAÇÃO

# 3. CTA da Landing

O botão `Solicitar demonstração` deve navegar para um formulário próprio.

Preferência: rota pública dedicada, por exemplo:

```text
/demonstracao
```

Adaptar à arquitetura real.

Evitar `mailto` como fluxo principal.

---

# 4. Formulário público

Campos obrigatórios:

```text
Nome completo do representante
Nome da clínica ou empresa
E-mail
Telefone
```

Campo opcional:

```text
Descrição / Conte um pouco sobre sua necessidade
```

Não solicitar dados de pacientes.

Não solicitar informações clínicas sensíveis.

---

# 5. Copy sugerida

Eyebrow:

```text
SOLICITAR DEMONSTRAÇÃO
```

Headline:

```text
Vamos entender a rotina da sua clínica.
```

Supporting:

```text
Preencha seus dados e nossa equipe entrará em contato para apresentar a IA Recepção e entender como ela pode se encaixar na sua operação.
```

---

# 6. Campos e placeholders

## Nome completo
Label: `Seu nome`
Placeholder: `Maria Oliveira`

## Clínica/empresa
Label: `Clínica ou empresa`
Placeholder: `Clínica Vida`

## E-mail
Label: `E-mail`
Placeholder: `maria@clinicavida.com.br`

## Telefone
Label: `Telefone / WhatsApp`
Placeholder: `(81) 99999-9999`

## Descrição opcional
Label: `Conte um pouco sobre sua necessidade`
Helper: `Opcional. Não inclua dados de pacientes.`

---

# 7. Validação frontend

Validar:

- nome;
- clínica/empresa;
- e-mail;
- telefone;
- tamanho máximo da descrição;
- trim;
- limites de caracteres.

Não confiar apenas no frontend.

---

# 8. Validação backend

Criar/reutilizar DTO dedicado.

Validar novamente:

- required;
- length;
- email;
- telefone;
- description length;
- normalização.

Nunca mapear payload público diretamente em entidade sem validação.

---

# 9. Endpoint público

Criar/reutilizar endpoint equivalente a:

```text
POST /api/v1/leads/demo-requests
```

Naming deve seguir o padrão real do projeto.

Endpoint deve ser público, porém protegido contra abuso.

---

# 10. Entidade de Lead

Avaliar se já existe entidade equivalente.

Caso não exista, criar modelo simples, por exemplo:

```text
DemoLead
- Id
- FullName
- CompanyOrClinicName
- Email
- Phone
- Description
- Status
- Source
- CreatedAt
- UpdatedAt
- AssignedToUserId?
- LastContactAt?
- Version/concurrency token quando padrão do projeto
```

Não criar CRM completo.

---

# 11. Status comercial

Usar poucos status:

```text
New
Contacted
Qualified
DemoScheduled
Won
Lost
Archived
```

Adaptar ao padrão real.

Status inicial deve ser sempre `New`.

---

# 12. Source

Registrar origem de forma simples, por exemplo:

```text
LandingDemoForm
```

Não implementar attribution/marketing tracking complexo nesta etapa.

---

# 13. Não criar tenant automaticamente

Lead público é um registro de plataforma.

Não criar tenant/clínica automaticamente ao receber a solicitação.

A criação da clínica deve acontecer apenas após decisão comercial e onboarding.

---

# 14. Proteção anti-spam

Obrigatório:

- rate limiting;
- limite de payload;
- validação server-side;
- honeypot ou mecanismo leve equivalente, se adequado;
- logging seguro;
- deduplicação/idempotência razoável.

Captcha externo apenas se necessário e aprovado.

---

# 15. Rate limiting

Criar política específica para o endpoint público.

Considerar IP e janela temporal.

Não hardcode valores sem analisar a política existente do projeto.

---

# 16. Resposta pública

Após submissão válida:

```text
Recebemos sua solicitação.
Nossa equipe analisará as informações e entrará em contato.
```

Não revelar detalhes internos.

---

# 17. Deduplicação

Não bloquear agressivamente o mesmo e-mail para sempre.

Pode haver leads legítimos repetidos.

Preferir deduplicação em janela curta ou sinalização de possível duplicata.

---

# 18. Privacidade

Se já houver Política de Privacidade aprovada, vincular adequadamente.

Se não houver:

```text
LEGAL/PRIVACY ACTION REQUIRED
```

Não inventar texto jurídico.

---

# 19. Logs

Não logar o formulário completo.

Evitar registrar descrição, telefone e e-mail completos sem necessidade.

Usar IDs/correlation id.

---

# 20. Auditoria

Registrar eventos como:

```text
DemoLeadCreated
DemoLeadStatusChanged
DemoLeadAssigned
DemoLeadNoteAdded
DemoLeadContacted
```

Sem conteúdo excessivo.

---

# PARTE B — PLATFORMADMIN / LEADS

# 21. Visibilidade

Somente `PlatformAdmin` pode:

- listar leads;
- abrir detalhes;
- alterar status;
- adicionar notas;
- atribuir responsável;
- arquivar;
- registrar contato.

ClinicAdmin, Receptionist e Professional não podem acessar esses dados.

Backend deve impor a policy.

---

# 22. Dashboard PlatformAdmin

Adicionar resumo comercial:

```text
Leads novos
Leads aguardando contato
Demonstrações agendadas
Leads sem atualização recente
```

Não criar analytics de vendas complexo.

---

# 23. Card de atenção

Exemplo:

```text
3 novas solicitações de demonstração
[Ver solicitações]
```

Sem leads:

```text
Nenhuma solicitação nova.
```

---

# 24. Menu administrativo

Adicionar item apropriado, por exemplo:

```text
Leads comerciais
```

ou `Demonstrações`.

---

# 25. Lista de leads

Tabela/lista com:

```text
Nome
Clínica/empresa
Contato
Status
Criado em
Último contato
Responsável
Ações
```

Não mostrar descrição inteira na tabela.

Paginação server-side.

---

# 26. Filtros

Adicionar:

- status;
- período;
- responsável;
- busca por nome/empresa/e-mail.

Não carregar todos os registros no frontend.

---

# 27. Detalhe do lead

Mostrar:

```text
Dados do contato
Empresa/clínica
Descrição enviada
Origem
Status
Data da solicitação
Responsável
Histórico de ações
Notas comerciais
```

---

# 28. Ações comerciais

Permitir:

```text
Marcar como contatado
Qualificar
Agendar demonstração
Marcar como ganho
Marcar como perdido
Arquivar
```

Não implementar contrato/checkout automático nesta etapa.

---

# 29. Notas internas

PlatformAdmin pode adicionar notas privadas.

As notas nunca aparecem ao lead.

Aplicar tamanho máximo e auditoria.

---

# 30. Demonstração

Não é necessário criar calendário comercial completo.

Pode existir `DemoScheduledAt` ou status simples, se isso se encaixar no domínio.

---

# 31. Material demonstrativo

Não enviar automaticamente anexos sem material/template aprovado.

Pode ser documentado como `FUTURE`.

---

# 32. Autorização

Reutilizar/criar policy `PlatformAdminOnly` conforme padrão existente.

Proteger todos os endpoints administrativos de leads.

---

# 33. Multi-tenant

Leads são dados globais de plataforma.

Não usar `ClinicStaff` para autorização.

Não vazar leads em APIs de tenant.

---

# PARTE C — NOTIFICAÇÃO DE LEAD

# 34. E-mail interno opcional

Se serviço de e-mail já estiver confiável, considerar notificar PlatformAdmins sobre novo lead.

O lead deve ser persistido antes da tentativa de notificação.

Falha no e-mail não pode perder o lead.

Preferir Outbox/evento se o projeto já segue esse padrão.

---

# PARTE D — ESQUECI MINHA SENHA

# 35. Link no Login

Adicionar:

```text
Esqueci minha senha
```

apontando para fluxo real.

---

# 36. Página Forgot Password

Rota conceitual:

```text
/esqueci-minha-senha
```

Headline:

```text
Recupere o acesso à sua conta
```

Supporting:

```text
Informe seu e-mail. Se houver uma conta válida, enviaremos as instruções para redefinir sua senha.
```

---

# 37. Prevenção de enumeração

A resposta deve ser igual para usuário existente e inexistente:

```text
Se existir uma conta associada a este e-mail, enviaremos as instruções para redefinir sua senha.
```

Nunca responder `E-mail não encontrado`.

---

# 38. Timing

Evitar diferenças óbvias de timing entre conta existente e inexistente, quando razoável.

---

# 39. Rate limiting

Aplicar rate limit específico em:

```text
forgot-password
reset-password
```

Considerar IP, e-mail normalizado/hash e cooldown.

Não revelar existência da conta.

---

# 40. Token de reset

Usar mecanismo oficial já existente.

Se ASP.NET Identity:

preferir `UserManager` e token oficial de reset.

Se auth própria:

usar token criptograficamente seguro.

Token deve ser:

- imprevisível;
- temporário;
- escopado;
- single-use na prática;
- invalidável.

---

# 41. Persistência do token

Se houver persistência própria:

não armazenar token em plaintext.

Armazenar hash quando aplicável.

Não criar tabela paralela se ASP.NET Identity já resolver.

---

# 42. Expiração

Definir expiração centralizada.

Exemplo conceitual: 15–60 minutos.

Não hardcode em vários pontos.

---

# 43. URL de reset

E-mail deve usar frontend seguro por ambiente, por exemplo:

```text
https://app.iarecepcao.com.br/redefinir-senha?token=...
```

Adaptar à rota real.

Nunca usar localhost em Production.

---

# 44. Token em URL e logs

Não registrar reset token em analytics, logs de aplicação ou error reporting.

Revisar query-string logging quando controlável.

---

# 45. Template de e-mail

Subject:

```text
Redefinição de senha — IA Recepção
```

Body conceitual:

```text
Olá,

Recebemos uma solicitação para redefinir a senha da sua conta na IA Recepção.

[Redefinir minha senha]

Este link expira em breve e deve ser usado apenas uma vez.

Se você não solicitou a redefinição, ignore este e-mail.
```

Nunca enviar senha por e-mail.

---

# 46. Serviço de e-mail

Reutilizar provider já configurado.

Não introduzir provider novo sem necessidade.

Credenciais somente no backend/secret store.

---

# 47. Página Reset Password

Rota:

```text
/redefinir-senha
```

Campos:

```text
Nova senha
Confirmar nova senha
```

Adicionar show/hide.

Mostrar requisitos da política de senha de forma amigável.

Backend é a fonte de verdade.

---

# 48. Token inválido ou expirado

Mensagem:

```text
Este link não é mais válido.
Solicite uma nova redefinição de senha.
```

CTA:

```text
Solicitar novo link
```

Não revelar o motivo exato.

---

# 49. Single-use

Após redefinir senha com sucesso, o mesmo token não pode funcionar novamente.

Testar explicitamente.

---

# 50. Sucesso

Mensagem:

```text
Sua senha foi redefinida com sucesso.
```

CTA:

```text
Entrar
```

Preferência: voltar ao login em vez de login automático.

---

# 51. Refresh tokens e sessões

Após reset bem-sucedido, avaliar invalidar:

- refresh tokens;
- sessões persistentes;
- tokens de recuperação anteriores.

Reutilizar mecanismos existentes.

Se não houver revogação global, documentar gap de segurança.

---

# 52. Access tokens

JWT access tokens já emitidos podem permanecer válidos até expiração se não houver revocation list.

Não criar solução improvisada.

Se houver security stamp/token version, reutilizar.

---

# 53. Usuário desativado

Forgot password para usuário desativado continua retornando resposta genérica.

Não revelar status.

---

# 54. PlatformAdmin

O fluxo de recuperação deve funcionar também para PlatformAdmin, salvo regra explícita contrária.

Não criar backdoor manual.

---

# 55. Auditoria do reset

Registrar:

```text
PasswordResetRequested
PasswordResetEmailQueued
PasswordResetCompleted
PasswordResetFailed
```

Sem token, senha ou hash.

---

# 56. Falha de e-mail

Resposta pública continua genérica.

Internamente:

- log;
- métrica;
- retry/Outbox quando existente.

---

# PARTE E — UX / DESIGN

# 57. Consistência visual

Formulário de demonstração e páginas de recuperação devem usar:

- paleta oficial IA Recepção;
- mesmos inputs/buttons;
- mesmos radius;
- focus states;
- typography;
- navy/azul já padronizados.

Não criar design paralelo.

---

# 58. Mobile

Validar:

```text
375
390
430
768
1024
1440
```

Especialmente:

- formulário público;
- forgot password;
- reset password;
- lead detail PlatformAdmin.

---

# 59. Acessibilidade

Meta WCAG 2.2 AA.

Validar:

- labels;
- autocomplete;
- focus;
- keyboard;
- errors;
- status messages;
- contrast;
- required fields.

---

# 60. Autocomplete

Demo form:

```text
name
organization
email
tel
```

Forgot password:

```text
email
```

Reset password:

```text
new-password
```

---

# PARTE F — TESTES LEADS

# 61. Unit tests

Cobrir:

- validation;
- normalização;
- status inicial;
- transitions;
- authorization;
- mapping.

---

# 62. Integration tests

Cenários:

```text
public creates lead → New
ClinicAdmin lists leads → 403
PlatformAdmin lists → 200
PlatformAdmin changes status → audit
```

---

# 63. Spam/validation tests

Testar:

- payload vazio;
- e-mail inválido;
- telefone inválido;
- descrição enorme;
- honeypot;
- rate limit;
- malformed request.

---

# 64. Playwright leads

```text
Landing
→ Solicitar demonstração
→ preencher
→ submit
→ success
```

Depois:

```text
PlatformAdmin login
→ Dashboard
→ lead aparece
→ abrir
→ Contacted
→ adicionar nota
```

Usar dados fictícios.

---

# PARTE G — TESTES PASSWORD RESET

# 65. Unit tests

Cobrir:

- usuário existente;
- inexistente;
- desativado;
- rate limit;
- token válido;
- expirado;
- inválido;
- reutilizado;
- password policy;
- confirmação divergente.

---

# 66. Integration test

Com e-mail fake:

```text
create user
→ forgot password
→ capture reset link
→ reset
→ old password fails
→ new password succeeds
→ token reuse fails
```

---

# 67. Enumeration test

Mensagens públicas para e-mail conhecido e desconhecido devem ser semanticamente equivalentes.

---

# 68. Playwright recovery

Com mail sink fake:

```text
Login
→ Esqueci minha senha
→ e-mail
→ confirmation
→ obter link no fake mail
→ reset
→ sucesso
→ login nova senha
```

Nunca usar e-mail real em CI.

---

# 69. Regressão auth

Executar testes existentes de:

- login;
- refresh;
- logout;
- authorization;
- PlatformAdmin;
- ClinicAdmin;
- multi-tenant.

---

# PARTE H — SEGURANÇA

# 70. Security review

## Leads
- mass assignment;
- injection;
- rate limiting;
- spam;
- XSS na descrição;
- authorization;
- PII em logs;
- output encoding.

## Password reset
- account enumeration;
- token strength;
- expiry;
- replay;
- CSRF quando aplicável;
- rate limiting;
- session invalidation;
- logging;
- e-mail injection;
- open redirects.

---

# 71. XSS

Descrição do lead deve ser tratada como texto.

Não renderizar HTML fornecido pelo usuário.

Não usar `dangerouslySetInnerHTML`.

---

# 72. Open redirect

Validar `returnUrl`.

Não permitir destinos externos arbitrários.

---

# 73. CSRF

Se usar cookies, revisar proteção CSRF nos POSTs relevantes.

Se bearer token, seguir arquitetura existente.

---

# 74. Analytics

Somente se já aprovado/configurado:

```text
demo_form_viewed
demo_form_started
demo_form_submitted
demo_form_failed
forgot_password_started
forgot_password_submitted
password_reset_completed
```

Nunca enviar PII ou token.

---

# PARTE I — API / DOCUMENTAÇÃO

# 75. Postman

Atualizar:

```text
Public Demo Lead
PlatformAdmin Leads
Lead Detail
Lead Status
Lead Notes
Forgot Password
Reset Password
```

Usar variáveis fake.

---

# 76. OpenAPI

Documentar endpoints novos.

Não expor internals de token ou hash.

---

# 77. Documentação

Criar/atualizar:

```text
docs/product/demo-lead-flow.md
docs/platform/commercial-leads.md
docs/security/password-recovery.md
docs/operations/email-password-reset.md
docs/api/leads.md
docs/api/auth-password-recovery.md
docs/testing/leads-e2e.md
docs/testing/password-recovery-e2e.md
```

---

# 78. Environment variables

Documentar nomes reais necessários, sem valores:

```text
PublicAppUrl
Email sender
SMTP/provider secrets
Password reset expiration
Lead notification destination
Feature flags
```

---

# PARTE J — MIGRATIONS

# 79. Migration de Leads

Se nova entidade for necessária:

- criar migration;
- índices;
- constraints;
- status;
- timestamps;
- concurrency quando padrão.

Password recovery pode não precisar de migration se usar mecanismo oficial existente.

---

# 80. Índices

Avaliar somente se necessários:

```text
CreatedAt
Status
NormalizedEmail
AssignedToUserId
```

---

# 81. Segurança da migration

Migration deve ser segura em banco produtivo existente.

Inspecionar SQL gerado.

---

# PARTE K — ORDEM DE EXECUÇÃO

# 82. Sequência

```text
1. Auditoria
2. Modelagem mínima de DemoLead
3. Migration
4. Endpoint público
5. Antiabuso
6. Landing form
7. PlatformAdmin Leads API
8. PlatformAdmin UI
9. Audit
10. Notifications opcionais
11. Forgot Password backend
12. Email reset
13. Reset Password backend
14. Forgot/Reset frontend
15. Session/token handling
16. Security review
17. Unit tests
18. Integration tests
19. Playwright
20. Postman/OpenAPI
21. Documentation
22. Regression
```

---

# 83. Não fazer

NÃO:

- criar CRM completo;
- criar contrato/checkout automático;
- criar tenant ao receber lead;
- expor leads para ClinicAdmin;
- enviar PII para analytics;
- logar descrição completa;
- continuar com `mailto` como principal;
- revelar se e-mail existe;
- enviar senha por e-mail;
- criar senha temporária no forgot password;
- permitir token sem expiração;
- permitir reutilização de token;
- alterar password policy por conveniência;
- realizar login automático inseguro;
- quebrar auth atual;
- usar dados reais nos testes.

---

# 84. Critérios de aceite — Leads

- CTA abre fluxo próprio;
- 4 campos obrigatórios + descrição opcional;
- backend validation;
- rate limiting;
- anti-spam;
- lead persistido;
- status New;
- somente PlatformAdmin acessa;
- resumo no dashboard;
- status e notas;
- auditoria;
- paginação/filtros;
- sem XSS;
- sem PII indevida;
- E2E passando.

---

# 85. Critérios de aceite — Password Recovery

- link no Login;
- forgot password page;
- resposta anti-enumeração;
- e-mail;
- token temporário;
- reset page;
- password policy;
- single-use;
- token expirado tratado;
- sessões/refresh tratados conforme arquitetura;
- audit;
- rate limiting;
- E2E passando;
- auth regression passando.

---

# 86. Validação técnica

Usar somente scripts reais.

Backend:

```text
dotnet restore
dotnet build
dotnet test
```

Frontend:

```text
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1
```

Inspecionar migration SQL.

Não usar `test.skip`/`fixme` para concluir.

---

# 87. Relatório final obrigatório

Apresentar:

1. auditoria;
2. entidade Lead;
3. migration;
4. endpoint público;
5. validação;
6. anti-spam/rate limit;
7. formulário Landing;
8. PlatformAdmin API;
9. PlatformAdmin UI;
10. dashboard summary;
11. status;
12. notas;
13. audit;
14. notification;
15. forgot password backend;
16. token mechanism;
17. expiration;
18. e-mail;
19. reset frontend;
20. password policy;
21. token replay;
22. session invalidation;
23. enumeration protection;
24. security findings;
25. unit tests;
26. integration tests;
27. Playwright;
28. Postman;
29. OpenAPI;
30. docs;
31. migration SQL review;
32. regressão;
33. riscos restantes.

---

# 88. Resultado final

## Comercial

```text
VISITANTE
↓
SOLICITAR DEMONSTRAÇÃO
↓
LEAD SEGURO
↓
PLATFORMADMIN
↓
ANÁLISE
↓
CONTATO
↓
DEMONSTRAÇÃO
↓
DECISÃO COMERCIAL
```

## Acesso

```text
USUÁRIO
↓
ESQUECI MINHA SENHA
↓
EMAIL
↓
TOKEN TEMPORÁRIO
↓
NOVA SENHA
↓
TOKEN INVALIDADO
↓
LOGIN
```

A implementação deve manter o MVP simples, mas suficientemente profissional e seguro para operar com leads e usuários reais.
