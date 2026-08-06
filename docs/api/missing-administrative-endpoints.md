# Endpoints administrativos ausentes

Este inventário registra lacunas observadas na API atual. Não constitui contrato implementado nem autoriza o frontend a simular comportamento.

## High — gestão de tenant pela plataforma

| Capacidade ausente | Proposta para etapa futura | Perfil | Regras mínimas |
| --- | --- | --- | --- |
| Detalhar tenant | `GET /api/platform/tenants/{tenantId}` | `PlatformAdmin` | retorna somente dados administrativos e agregados permitidos |
| Atualizar tenant | `PUT /api/platform/tenants/{tenantId}` | `PlatformAdmin` | nome, slug, locale, fuso, plano e limites; auditoria e evento após commit |
| Criar tenant sem onboarding | `POST /api/platform/tenants` | `PlatformAdmin` | só se houver caso de uso que não exija clínica/administrador; idempotência obrigatória |

Hoje existem listagem, onboarding transacional e `POST /api/platform/tenants/{id}/{action}` para status. O endpoint de ação não substitui detalhe e edição com contrato explícito.

## High — ciclo de vida de usuários administrativos

| Capacidade ausente | Proposta para etapa futura | Perfil | Regras mínimas |
| --- | --- | --- | --- |
| Detalhar usuário | `GET /api/platform/users/{userId}` | `PlatformAdmin` | isolamento e auditoria |
| Criar/convocar administrador | `POST /api/platform/tenants/{tenantId}/users` | `PlatformAdmin` | convite ou senha temporária; idempotência; sem retornar segredo |
| Alterar papel ou status | `PUT /api/platform/users/{userId}` | `PlatformAdmin` | não permitir remoção do último administrador ativo sem regra explícita |
| Revogar sessão | `POST /api/platform/users/{userId}/revoke-sessions` | `PlatformAdmin` | revoga refresh tokens e audita a operação |

Atualmente a API somente lista usuários globais e cria o administrador inicial pelo onboarding.

## Medium — operação e auditoria

| Capacidade ausente | Proposta para etapa futura | Perfil | Regras mínimas |
| --- | --- | --- | --- |
| Detalhe/exportação de auditoria | `GET /api/audit/{id}` e export assíncrono | `ClinicAdmin` / `PlatformAdmin` | paginação, período limitado, mascaramento e trilha de auditoria |
| Consulta operacional de Outbox/DLQ | `GET /api/operations/outbox` e reprocessamento controlado | perfil operacional dedicado | nunca expor payload sensível; autorização, auditoria e rate limit |
| Integrações por tenant na plataforma | `GET /api/platform/tenants/{tenantId}/integrations` | `PlatformAdmin` | resposta sanitizada, sem credenciais ou números completos |

## Low — conveniências administrativas

| Capacidade ausente | Proposta para etapa futura | Perfil | Regra |
| --- | --- | --- | --- |
| Busca paginada de unidades/especialidades/profissionais | filtros e paginação nos endpoints de catálogo | policies `*.View` | manter filtros por tenant |
| Exclusão lógica de paciente | ação de arquivamento, não remoção física | `Patients.Manage` | respeitar retenção, auditoria e vínculos de agenda/conversa |

## Critérios para qualquer implementação futura

Todo endpoint novo deve definir request/response no OpenAPI, policy, isolamento de tenant, idempotência quando houver mutation crítica, auditoria, evento pós-commit e request correspondente no Postman. Consulte o [checklist de documentação](../documentation-checklist.md) quando ele estiver disponível.
