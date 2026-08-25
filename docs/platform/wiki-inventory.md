# Inventário da documentação da plataforma

Inventário inicial para a Wiki do PlatformAdmin. A classificação abaixo prioriza fontes canônicas e evita publicar automaticamente todo o diretório `docs/`.

| Documento canônico | Categoria | Situação | Ação |
|---|---|---|---|
| `docs/architecture.md` | Técnica / Arquitetura | Atual | Publicar como “Arquitetura geral” |
| `docs/architecture/whatsapp-provider-abstraction.md` | Técnica / Multi-tenant | Atual | Publicar como “Arquitetura multi-tenant” |
| `docs/operations/platformadmin-whatsapp-sender-guide.md` | Técnica / WhatsApp | Atual | Publicar e revisar a cada mudança de provedor |
| `docs/deployment/production-readiness-audit.md` | Técnica / Deploy | Revisar | Conferir Railway, Vercel e variáveis atuais |
| `docs/security/authorization.md` | Governança / Segurança | Atual | Publicar somente para PlatformAdmin |
| `docs/testing/troubleshooting.md` | Governança / Operação | Atual | Publicar como troubleshooting |
| `docs/platform/clinic-onboarding.md` | Implantação | Atual | Publicar como onboarding |
| `docs/pilot/production-smoke-checklist.md` | Implantação / Go-live | Atual | Publicar como checklist |
| `docs/product/value-proposition.md` | Negócio / Produto | Atual | Publicar como visão do produto |
| `docs/product/pricing-strategy.md` | Negócio / Pricing | Atual | Publicar sem inventar billing |
| `docs/product/demo-flow.md` | Negócio / Leads | Atual | Publicar como fluxo comercial |

## Critérios aplicados

- Arquivos históricos e prompts não entram na Wiki automaticamente.
- Documentos com nomes antigos são mantidos como fonte técnica quando ainda necessários, mas recebem título amigável na interface.
- Referências a segredos, tokens, senhas e connection strings são removidas ou substituídas por placeholders.
- Um documento com instruções divergentes deve receber `NeedsReview` antes de ser promovido a `Current`.
