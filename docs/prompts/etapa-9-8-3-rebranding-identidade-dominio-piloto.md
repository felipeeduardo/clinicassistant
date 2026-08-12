# Etapa 9.8.3 --- Rebranding Gradual, Identidade Final, Domínio e Prontidão para Piloto Real

## Contexto e decisão

Domínio oficial registrado: `iarecepcao.com.br`.

-   Marca pública alvo: **IA Recepção**
-   Nome técnico interno: **ClinicAssistant**
-   Regra: NÃO renomear namespaces, solution, projetos, banco,
    migrations, assemblies ou identificadores técnicos apenas por
    branding.

A migração deve ser gradual, segura e concentrada na camada pública.

## 1. Objetivos

1.  Migrar publicamente Clinic Assistant → IA Recepção.
2.  Fechar a identidade visual com opções comparáveis antes da
    aprovação.
3.  Preparar `iarecepcao.com.br`, `www`, `app` e `api`.
4.  Preservar localhost, ngrok e FakeWhatsAppGateway.
5.  Fechar pendências comerciais da 9.8.2.
6.  Substituir `mailto` por lead flow seguro.
7.  Definir analytics e eventos sem PII.
8.  Validar pricing, calculadora, CTA, branding e piloto via E2E.
9.  Consolidar riscos e checklist do primeiro piloto real.

## 2. Auditoria de rebranding

Buscar: `Clinic Assistant`, `ClinicAssistant`, `clinic-assistant`,
`clinicassistant`.

Classificar cada ocorrência em: \| Ocorrência \| Arquivo \|
Público/Interno \| Alterar? \| Risco \| Justificativa \|
\|---\|---\|---\|---\|---\|---\|

Criar `docs/brand/rebranding-inventory.md`.

Não executar replace global.

### Público --- migrar

Landing, navbar, footer, login, sidebar, títulos visíveis, metadata/SEO,
Open Graph, favicon/app icon após aprovação, e-mails, manifest/PWA,
documentação comercial e demais superfícies visíveis.

### Interno --- manter

Namespaces C#, solution/projetos, assemblies, banco, schemas,
migrations, classes, filas/tópicos persistentes, identificadores
técnicos e estruturas cujo rename gere risco sem valor para o MVP.

## 3. Arquitetura de domínio

Preparar: - `https://iarecepcao.com.br` → Landing -
`https://www.iarecepcao.com.br` → redirect canônico -
`https://app.iarecepcao.com.br` → aplicação autenticada -
`https://api.iarecepcao.com.br` → backend

Criar `docs/operations/domain-iarecepcao-runbook.md` com DNS, HTTPS,
redirect, TTL, validação e rollback. Não inventar registros A/CNAME sem
conhecer os provedores. Marcar ações externas como
`MANUAL ACTION REQUIRED`.

## 4. Ambientes

Preservar: - Development - Test/E2E - Pilot/Staging - Production

Development deve continuar funcionando com localhost, ngrok e
FakeWhatsAppGateway. O primeiro cliente real deve preferencialmente usar
Pilot/Staging antes de Production.

Centralizar public URL, app URL, API URL, CORS, callbacks, webhooks,
SignalR, links de e-mail e canonical por ambiente.

## 5. Identidade visual --- cinco direções

Recuperar e adaptar as cinco direções da 9.8.2 para **IA Recepção**: 1.
Conversation + Calendar 2. IR Monogram --- adaptar o antigo conceito CA
à nova marca 3. Smart Reception 4. Care + Connection 5. Pulse / Flow

Para cada uma mostrar: - mark; - wordmark "IA Recepção"; - horizontal
lockup; - compact lockup; - favicon preview; - app icon preview; -
dark/light; - monochrome; - navbar preview; - login preview; - sidebar
preview; - Landing Hero preview.

Se o Codex não gerar imagens finais, criar SVGs conceituais simples,
prompts detalhados e uma página de comparação. Não fingir que conceitos
são logo final.

## 6. Brand Review

Criar rota apenas de desenvolvimento, Storybook existente ou artefato
equivalente para comparar os cinco conceitos lado a lado.

Usar a matriz: \| Critério \| Peso \| \|---\|---:\| \| Reconhecimento \|
20% \| \| Simplicidade \| 15% \| \| Relação com produto \| 15% \| \|
Diferenciação \| 15% \| \| Escalabilidade \| 10% \| \| Favicon/App icon
\| 10% \| \| Dark/light \| 5% \| \| Monocromático \| 5% \| \|
Longevidade \| 5% \|

Escolha final = `APPROVAL REQUIRED`.

## 7. Logo final

Somente após aprovação explícita, produzir/aplicar: - mark; -
wordmark; - lockup horizontal; - lockup compacto; - favicon; - app
icon; - dark/light; - monocromático.

Organizar assets em `public/brand/` conforme convenção real do projeto.

## 8. Opções de imagens

Criar `docs/brand/brand-image-directions.md`.

Gerar ao menos 3 composições conceituais ou prompts detalhados para cada
família: - **Product-led:** UI real em frames. - **Conversational:**
conversa → disponibilidade → agenda → confirmação. - **Abstract Brand:**
fluxo, conexão, organização e recepção inteligente.

Não usar logo do WhatsApp/Twilio como identidade, cruz médica genérica,
cérebro de IA, robô genérico, estetoscópio como logo, cópia de
concorrentes ou assets de licença incerta.

## 9. Rebranding público gradual

Criar configuração pública única, se ainda não existir: `name`,
`domain`, `tagline`, `supportEmail`, `appUrl`.

### Fase 1

Landing, metadata, navbar, footer, Open Graph, social preview e textos
comerciais.

### Fase 2 --- somente após aprovação visual

Login, sidebar, app header, telas administrativas, e-mails, PWA,
favicon/app icon e logo definitivos.

Testes que verificam texto público devem ser atualizados; testes
técnicos não devem ser alterados para esconder regressões.

## 10. Decisões comerciais pendentes

Criar `docs/product/mvp-commercial-decisions.md` com: \| Decisão \|
Opções \| Impacto \| Recomendação técnica \| Decisão final \|
\|---\|---\|---\|---\|---\|

Itens `APPROVAL REQUIRED`: 1. preço comercial oficial; 2. taxa de
implantação; 3. limites do plano MVP; 4. cobrança por consumo; 5. piloto
gratuito ou pago; 6. publicação final da calculadora.

O Codex não pode inventar essas decisões.

## 11. Custos reais

Criar `docs/operations/mvp-cost-model.md` para: - Twilio/WhatsApp; -
frontend; - backend; - banco; - Redis; - storage; - e-mail; -
observabilidade; - domínio; - IA futura; - outros.

Separar custo fixo, variável, por tenant, por mensagem e storage. Não
inventar valores. Marcar pesquisa externa necessária como
`EXTERNAL VALIDATION REQUIRED`.

Preparar unit economics:
`MRR por clínica - custo variável - alocação de infraestrutura = margem de contribuição estimada`
e break-even aproximado somente quando inputs forem aprovados. Não
publicar unit economics na landing.

## 12. Lead flow seguro

Substituir `mailto` por:
`Landing → formulário → API → validação → persistência/encaminhamento → sucesso`.

Campos sugeridos, sujeitos a decisão: - Nome - Clínica - E-mail -
WhatsApp/telefone - Número aproximado de profissionais (opcional) -
Principal desafio (opcional)

Nunca coletar dados de pacientes.

Endpoint: - DTO dedicado; - validação/normalização; - rate limiting; -
anti-spam/honeypot; - idempotência quando apropriado; - correlation
ID; - logs sem PII desnecessária; - erro seguro; - métricas; - testes.

Preparar abstrações `DatabaseLeadSink`, `EmailLeadSink`, `CRM futuro`.
Destino definitivo = `APPROVAL REQUIRED`.

## 13. Analytics

Criar `docs/operations/analytics-decision.md` comparando privacidade,
custo, Next.js, eventos, consentimento/cookies, LGPD e manutenção.

Fornecedor = `APPROVAL REQUIRED`.

Após aprovação, implementar sem PII: `landing_view`,
`hero_demo_started`, `hero_demo_completed`, `product_tab_changed`,
`pricing_viewed`, `pricing_cta_clicked`, `roi_calculator_started`,
`roi_calculator_completed`, `demo_cta_clicked`, `pilot_cta_clicked`,
`lead_started`, `lead_submitted`, `lead_failed`.

Nunca enviar nome, e-mail, telefone, clínica ou conteúdo de mensagens.

## 14. Twilio e domínio

Manter secrets apenas em environment/secret store.

Documentar URLs reais do projeto para: - inbound webhook em
`api.iarecepcao.com.br`; - status callback em `api.iarecepcao.com.br`.

Development continua com ngrok.

Antes do piloto validar HTTPS, assinatura Twilio, idempotência,
duplicidade/replay, correlation ID, callbacks, retry, observabilidade,
multi-tenant, timeout e handoff humano.

Auditar `.env*`, CI secrets, logs, screenshots, Postman e documentação.
Se houver suspeita de exposição: `SECRET ROTATION REQUIRED`.

## 15. CORS

Allowlist explícita por ambiente. Pilot/Production deve incluir apenas
origins necessários, como `https://iarecepcao.com.br` e
`https://app.iarecepcao.com.br`. Nunca usar `*` com credenciais.

## 16. Calculadora de impacto

Revisar fórmulas, defaults, copy, disclaimer, mobile, BRL,
arredondamento e cenários.

Criar `docs/product/value-calculator-approval.md` com status: - DRAFT -
APPROVED FOR PILOT - APPROVED FOR PUBLIC

Aprovação final = `APPROVAL REQUIRED`.

Manter linguagem: - horas potencialmente liberadas; - valor equivalente
de tempo; - impacto operacional estimado.

Nunca "lucro/economia garantidos".

## 17. E2E comercial

Cobrir: - pricing; - calculadora; - CTA; - lead form; - sucesso/erro; -
rate limit; - demo/pilot/publicPricing; - rebranding; - login; -
navegação pública → app.

Não usar `test.skip`/`fixme` para fechar a etapa.

## 18. Validação visual

Validar 375, 390, 430, 768, 1024, 1280 e 1440 px em landing, pricing,
calculator, formulário, login, sidebar, navbar e logo. Sem overflow
horizontal.

Se visual regression já existir, capturar landing desktop/mobile, login,
sidebar, pricing, calculator e brand lockup.

## 19. Pilot readiness

Criar `docs/pilot/pilot-readiness-checklist.md`.

Cobrir: - Produto: WhatsApp, agenda, conversas, handoff, auditoria,
leads, branding. - Infra: domínio, HTTPS, DNS, API, DB, backups,
secrets, observabilidade. - Twilio: sender, inbound, callback,
assinatura, templates e teste real. - Comercial: gratuito/pago, duração,
limites, suporte, onboarding. - Segurança: tenant isolation,
autorização, logs e dados de teste. - Operação: rollback, incident
response, contato e métricas.

Preparar métricas do piloto sem inventar metas: conclusão de
agendamento, handoff, falhas, tempo de resposta, webhooks, agenda,
conversas e feedback da recepção. Metas numéricas = `APPROVAL REQUIRED`.

## 20. Risk register

Criar `docs/pilot/pilot-risk-register.md`: \| Risco \| Probabilidade \|
Impacto \| Mitigação \| Owner \| Status \|
\|---\|---\|---\|---\|---\|---\|

Cobrir marca, comercial, WhatsApp, infraestrutura, segurança,
privacidade, UX, operação, custos e suporte.

## 21. Ordem de execução

1.  9.8.3.1 Auditoria de rebranding
2.  9.8.3.2 Brand Review + opções de imagens
3.  9.8.3.3 **Approval Gate A --- PARAR para aprovação da identidade**
4.  9.8.3.4 Rebranding público Fase 1
5.  9.8.3.5 Logo final + Fase 2, após aprovação
6.  9.8.3.6 Commercial Decisions
7.  9.8.3.7 Cost Model
8.  9.8.3.8 Leads
9.  9.8.3.9 Analytics
10. 9.8.3.10 Domain Infrastructure
11. 9.8.3.11 Calculator Approval
12. 9.8.3.12 E2E/Visual
13. 9.8.3.13 Pilot Readiness

## 22. Approval Gates

O Codex deve PARAR antes de: - **Gate A:** escolher/aplicar logo
final; - **Gate B:** publicar preço, implantação, limites, consumo ou
condição do piloto; - **Gate C:** instalar/ativar analytics externo; -
**Gate D:** escolher destino externo de leads; - **Gate E:** alterar
DNS/Twilio real ou promover Pilot → Production.

Silêncio não significa aprovação.

## 23. Critérios de aceite

### Branding

Inventário completo; 5 conceitos comparáveis; opções de imagens; IA
Recepção aplicada publicamente; ClinicAssistant preservado internamente.

### Domínio

Arquitetura root/www/app/api; URLs configuráveis; runbook; CORS seguro;
localhost/ngrok preservados.

### Comercial

Decisões formalizadas; pricing configurável; custos modelados;
calculadora revisada.

### Leads

`mailto` removido; endpoint seguro; formulário validado; sem PII
indevida em analytics/logs.

### Qualidade

E2E comercial; breakpoints; sem regressões; documentação; risk register;
pilot checklist.

## 24. Validação técnica

Usar somente scripts reais. Quando aplicáveis:

``` bash
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1

dotnet restore
dotnet build
dotnet test
```

## 25. Não fazer

-   não renomear namespaces/solution/projetos ClinicAssistant;
-   não fazer replace global;
-   não alterar migrations antigas;
-   não alterar banco apenas por branding;
-   não escolher logo final sem aprovação;
-   não inventar preço/custos/ROI;
-   não ativar analytics externo sem aprovação;
-   não expor secrets;
-   não colocar Twilio secrets no frontend;
-   não alterar DNS real silenciosamente;
-   não publicar Production sem gate;
-   não quebrar localhost/ngrok;
-   não esconder falhas E2E.

## 26. Relatório final obrigatório

Apresentar: 1. inventário; 2. itens mantidos ClinicAssistant; 3. itens
migrados IA Recepção; 4. cinco conceitos; 5. opções de imagens; 6.
identidade aprovada/pendente; 7. assets; 8. domínio; 9. DNS/manual
actions; 10. CORS; 11. Twilio; 12. pricing; 13. implantação; 14.
limites; 15. consumo; 16. piloto; 17. custos; 18. unit economics; 19.
leads; 20. analytics; 21. eventos; 22. calculadora; 23. E2E; 24.
breakpoints; 25. riscos; 26. pilot readiness; 27. approval gates; 28.
recomendação para primeiro teste real.

## 27. Resultado esperado

Sair de um MVP tecnicamente funcional com marca provisória para:

`IA Recepção → marca pública definida → identidade aprovada → domínio estruturado → landing comercial → lead flow seguro → modelo comercial documentado → Twilio preparado → E2E validado → primeiro piloto controlado pronto`

sem refatoração técnica desnecessária do núcleo `ClinicAssistant`.
