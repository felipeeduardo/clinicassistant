# Documentação do Clinic Assistant

Este índice é o ponto de entrada da documentação de produto e operação. Os documentos em `docs/prompts/` preservam o histórico e os requisitos das etapas; não são guias operacionais.

## Começar e executar localmente

- [Visão do projeto](../README.md)
- [Desenvolvimento local](development.md)
- [Guia E2E](testing/e2e-execution-guide.md)
- [Reset, seed e validação](testing/reset-and-seed.md)
- [Solução de problemas de testes](testing/troubleshooting.md)

## Arquitetura e segurança

- [Arquitetura](architecture.md)
- [Mensageria, Inbox e Outbox](messaging.md)
- [Decisão de monólito modular](decisions/ADR-001-monolith-modular.md)
- [Sessão e refresh token](security/authentication-session.md)
- [Autorização](security/authorization.md)
- [CSP](security/csp.md)

## APIs e integração

- [OpenAPI e Swagger](api/openapi.md) — fonte técnica dos contratos HTTP.
- [Autenticação e autorização](api/authorization.md)
- [Agenda e disponibilidade](api/scheduling.md)
- [Conversas e fila humana](api/conversations.md)
- [WhatsApp e templates](api/whatsapp.md)
- [Administração de plataforma](api/platform-administration.md)
- [Tempo real](api/realtime.md)
- [Endpoints administrativos ausentes](api/missing-administrative-endpoints.md)
- [Collection Postman](postman/README.md)

## Operação e frontend

- [Visão do frontend](frontend/overview.md)
- [Matriz operacional e E2E](frontend/operational-e2e.md)
- [Tempo real no frontend](frontend/realtime.md)
- [Acessibilidade](frontend/accessibility.md)
- [Desempenho](frontend/performance.md)
- [Métricas operacionais](operations/operational-metrics.md)
- [Prontidão Twilio](operations/twilio-production-readiness.md)
- [Formulários administrativos](operations/administrative-forms.md)
- [Checklist de documentação](documentation-checklist.md)
- [Registro de documentos arquivados](archive/README.md)

## Domínios especializados

- [Conversas](conversations/overview.md)
- [WhatsApp](whatsapp/overview.md)
- [Configuração Twilio](whatsapp/twilio-setup.md)
- [Webhooks Twilio](whatsapp/twilio-webhooks.md)
- [Observabilidade Production v1](observability/README.md)
