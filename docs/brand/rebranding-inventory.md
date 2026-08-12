# Inventário de rebranding — ClinicAssistant → IA Recepção

## Regra

`IA Recepção` é a marca pública alvo. `ClinicAssistant` permanece como identidade técnica interna. Nenhum replace global foi executado.

| Ocorrência | Arquivo/área | Público/Interno | Alterar? | Risco | Justificativa |
|---|---|---|---|---|---|
| Clinic Assistant | `frontend/app/page.tsx` | Público | Após Gate A | Médio | Landing, SEO, CTA e copy dependem do símbolo aprovado |
| Clinic Assistant | `frontend/components/app-shell.tsx` | Público | Após Gate A | Médio | Sidebar e header devem usar lockup aprovado |
| Clinic Assistant | `frontend/app/login/page.tsx` | Público | Após Gate A | Médio | Login integra a identidade Fase 2 |
| Clinic Assistant | `frontend/app/layout.tsx`, `frontend/app/icon.svg` | Público | Após Gate A | Médio | Metadata e ícone dependem da identidade final |
| ClinicAssistant | solution, namespaces, projetos, assemblies, migrations, banco e schemas | Interno | Não | Alto | Renomear quebra referências sem valor técnico |
| ClinicAssistant | Postman, scripts, Docker, CI e contratos | Técnico/operacional | Não | Alto | Identificadores persistentes e automações existentes |
| clinic-assistant | Docker Compose, imagens e documentação operacional | Técnico/operacional | Não | Alto | Compatibilidade local e pipelines |
| clinicassistant | banco, connection strings, scripts e ferramentas | Técnico/infra | Não | Muito alto | Nomes persistentes do banco e dos ambientes |

## Candidatas à Fase 1

Landing, navbar pública, footer, metadata/SEO, Open Graph, social preview e textos comerciais. Aplicação definitiva aguarda o Approval Gate A.

## Estado após aprovação visual

- Direção D, variação 1 aprovada.
- `BrandLockup`, login, navbar pública e app icon já usam o símbolo IA Recepção.
- Assets claro, escuro e monocromático estão em `frontend/public/brand/`.
- Copy comercial, lead flow, domínio e decisões de pricing continuam separados e não foram inferidos.
