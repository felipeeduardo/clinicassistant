# Aquisição, SEO e demonstração

O site público usa páginas de intenção comercial estáticas, artigos versionados no código e uma demonstração simulada. Isso permite publicar conteúdo com segurança sem criar clínicas ou acessar dados reais.

## Rotas públicas

- `/recepcionista-ia-para-clinicas`
- `/agendamento-whatsapp-para-clinicas`
- `/automacao-whatsapp-clinica`
- `/clinicas-pioneiras`
- `/conteudos` e `/conteudos/[slug]`
- `/demo`

Cada página tem metadata própria, canonical, Open Graph e links para `/demonstracao`. A lista de artigos vive em `frontend/lib/content/public-content.ts`.

## Leads

O formulário envia dados de atribuição UTM e referrer. O backend persiste esses campos em colunas opcionais da migration `202608250001_DemoLeadAttribution`, sem alterar o contrato de leads já existentes.

## Novas campanhas

Use URLs como `/demonstracao?utm_source=google&utm_medium=cpc&utm_campaign=recepcao`. Não altere o fluxo de autenticação, WhatsApp ou mensageria para adicionar campanhas.
