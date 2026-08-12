# Auditoria — Etapa 9.8.1

| Área | Estado atual | Problema | Proposta | Impacto |
|---|---|---|---|---|
| Rota pública | Server Component em `/` | Base correta, mas fluxo comercial curto | Reforçar narrativa produto → confiança → demonstração | Conversão |
| Navbar | Links âncora e login | CTA de demonstração pouco presente no desktop | Destacar demonstração e manter login secundário | Conversão |
| Hero | Copy e demo visual estática | Não há confirmação visual nem ligação explícita com Agenda | Demo controlada em CSS, com estado final “consulta confirmada” | Clareza |
| Problema | Pilares em três cards | Dor da recepção não aparece de forma editorial | Seção narrativa com exemplos de perguntas repetitivas | Posicionamento |
| Como funciona | Fluxo horizontal simples | Não evidencia ramificações para agenda e atendimento humano | Diagrama responsivo com conexões e foco acessível | Entendimento |
| Product showcase | Três placeholders textuais | Não mostra o produto real | Frames seguros e estáticos das superfícies existentes | Confiança |
| Bento grid | Ausente | Recursos não têm hierarquia visual | Blocos assimétricos, uma ideia por bloco | Produto |
| Benefícios | Misturados aos recursos | Resultado operacional pouco destacado | Benefícios orientados à rotina da recepção | Valor |
| Segurança | Capacidades reais documentadas | Falta hierarquia visual | Bloco curto sem certificações não validadas | Confiança |
| Público | Implícito | Segmentos não declarados | Clínicas, consultórios, multidisciplinares e pequenas redes | Relevância |
| FAQ | `details` acessível | Faltam perguntas sobre WhatsApp e horários | Completar perguntas obrigatórias | Objeções |
| CTA | `mailto` placeholder | Não há fluxo de leads no backend | Manter destino seguro e explicitar placeholder | Segurança |
| SEO | Metadata, robots e rota estática | Favicon, canonical e sitemap precisam ser verificados | Completar somente assets/configurações disponíveis | Descoberta |
| Responsividade | Classes responsivas | Não há suíte específica da landing | Adicionar Playwright desktop/mobile sem skips | Regressão |
| Performance | Server Component e CSS | Showcase deve evitar imagens pesadas | Frames CSS ou assets otimizados | Core Web Vitals |

## Decisões

- A Landing continua sem autenticação e sem dados reais.
- A demonstração permanece visual, sem chatbot funcional ou backend real.
- Não serão declaradas métricas, certificações ou conformidades sem evidência.
- A primeira entrega desta etapa cobre Navbar, Hero, demo, problema, fluxo, showcase e responsividade base.

## Status da etapa

As entregas de interface, acessibilidade, SEO e validação técnica estão concluídas. O fechamento consolidado está em [etapa-9-8-1-completion.md](./etapa-9-8-1-completion.md). Screenshots reais, endpoint de leads e analytics permanecem dependências externas e não devem ser simulados no código.
