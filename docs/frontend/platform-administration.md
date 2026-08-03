# Administração de plataforma

Rotas disponíveis para `PlatformAdmin`:

- `/platform`: tenants, usuários globais e clínicas globais;
- `/platform/onboarding`: criação transacional de tenant.

O onboarding exige `Idempotency-Key` e cria tenant, clínica, unidade inicial, `ClinicAdmin` e integração Fake desabilitada. A mesma chave retorna o resultado original, sem duplicar registros. O seed E2E inclui `platform-admin.e2e@fake.local`; a senha continua sendo fornecida por `E2E_DEFAULT_PASSWORD`.
