# Collection Postman

Importe `clinic-assistant.postman_collection.json` e um environment da pasta `environments/`:

- `ClinicAssistant-Local.postman_environment.json` para desenvolvimento local;
- `ClinicAssistant-E2E.postman_environment.json` para o perfil de dados determinísticos;
- `ClinicAssistant-Sandbox.postman_environment.json` como modelo sanitizado para sandbox.

Para executar requests com fixtures determinísticas pelo Collection Runner/Newman, use [data/e2e-test-data.json](data/e2e-test-data.json) como arquivo de dados. Ele contém apenas IDs e e-mails fictícios; a senha continua local à sessão.

Execute **Login** primeiro: ele grava apenas `accessToken`; o refresh token é administrado pelo cookie `HttpOnly` do próprio Postman e não deve ser salvo nem exportado em variável.

As pastas principais são numeradas por domínio: **00 — Health**, **01 — Authentication**, **02 — Tenants**, **03–09 — Clinics and catalog**, **06, 10–12, 15–16 — Operational APIs** e **09, 10–14 — Scheduling, conversations and WhatsApp**. A coleção está em migração incremental de nomes por etapa para domínios; nenhum request foi removido. Preencha os IDs de fixture ou os IDs retornados pelas operações de criação antes de executar requisições dependentes.

Depois de uma operação concorrente (confirmar, cancelar, reagendar ou alterar uma conversa), consulte o detalhe novamente e use o `expectedVersion` retornado mais recente. Os exemplos usam `1` apenas como ponto de partida. As requisições que exigem idempotência usam `{{idempotencyKey}}`.

Os exemplos de template usam um `ContentSid` fictício. `twilioAccountSid`, `twilioWhatsAppFrom` e `twilioTestRecipient` são somente placeholders. A collection não contém `AuthToken`, Account SID real, destinatário real ou qualquer outra credencial Twilio.

## Variáveis e propagação

A collection guarda os IDs retornados por criação de paciente, consulta e template. As demais variáveis (`tenantId`, `clinicId`, `userId`, `unitId`, `professionalId`, `conversationId`, `queueItemId`, `integrationId`, `slotId` e `correlationId`) devem receber valores de fixtures ou respostas anteriores antes de executar requisições dependentes.

Os requests de registro/login validam resposta JSON, token e perfil, e propagam `accessToken`, `tenantId` e `userId`. As criações de unidade, especialidade, profissional, paciente, consulta e template validam `201` e gravam o ID retornado para o próximo passo.

A pasta **02 — Tenants** contém o onboarding transacional. Antes de enviá-lo, informe valores fictícios únicos em `onboardingTenantName` e `onboardingTenantSlug`, e uma senha temporária apenas na sessão em `onboardingTemporaryPassword`. A resposta propaga tenant, clínica, unidade, administrador e integração. Em seguida, **Status do onboarding** consulta a prontidão da clínica; **Criar ClinicAdmin** é uma operação de recuperação idempotente para provisionar um administrador quando o onboarding inicial não o criou. Use somente e-mails e senhas de teste na collection.

`{{idempotencyKey}}` usa `{{$guid}}`, compatível com o Postman atual. Para repetir intencionalmente uma operação idempotente, substitua temporariamente a variável por um UUID fixo e reutilize-o apenas nesse teste.

O environment E2E traz somente o e-mail fictício do administrador. Preencha `loginEmail` e `loginPassword` apenas como valores locais da sessão antes de chamar **Login**; não salve nem exporte a senha em environment ou collection.

## Fluxos manuais

Os roteiros encadeados de onboarding, paciente, agenda, conversa, WhatsApp e dashboard estão em [Fluxos manuais E2E](e2e-flows.md).

Não preencha, exporte ou versione cookies de sessão, senhas de produção, tokens, chaves do Twilio ou números de telefone reais.
