# Onboarding inicial de clínica

A tela atual de PlatformAdmin reutiliza `POST /api/platform/onboarding` com
`Idempotency-Key` para criar, em uma transação curta, tenant, clínica, primeira
unidade, ClinicAdmin e integração Fake desabilitada. Os demais catálogos
(especialidades, profissionais, disponibilidade e WhatsApp) permanecem nas APIs
de domínio existentes e devem ser concluídos antes da ativação operacional.

Para acompanhar a retomada do processo, use `GET /api/platform/onboarding/{tenantId}`.
O endpoint informa os itens configurados e `canActivate`. Se o administrador da
clínica estiver ausente, um PlatformAdmin pode executar
`POST /api/platform/tenants/{tenantId}/clinic-admins` com `Idempotency-Key`,
`name`, `email` e `temporaryPassword`. A senha é armazenada apenas como hash e
deve ser entregue ao administrador por canal seguro; nunca a versione em
collection, environment ou documentação.

O wizard incremental completo (retomada por etapa, checklist e ativação) ainda é
um próximo incremento; não se deve simular sua conclusão no frontend.
