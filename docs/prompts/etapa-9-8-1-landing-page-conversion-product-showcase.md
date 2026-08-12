# Etapa 9.8.1 --- Landing Page Conversion & Product Showcase

## Contexto

A Etapa 9.8.1 deve evoluir a landing atual do Clinic Assistant sem
reconstruí-la do zero. A página já comunica WhatsApp, Agenda, Gestão,
segurança, FAQ e solicitação de demonstração. O objetivo agora é
transformar uma apresentação institucional correta em uma experiência de
produto orientada à conversão.

Fluxo desejado:

`capturar atenção → comunicar a dor → demonstrar a solução → mostrar o produto real → gerar confiança → solicitar demonstração`

## 1. Objetivo

Criar uma landing premium, moderna, responsiva e comercialmente
convincente para clínicas e consultórios.

Direção visual: - Editorial SaaS; - Bento Grid; - AI Gradient
Minimalism; - Product Showcase; - Ambient UI; - microinterações sutis.

Evitar cyberpunk, neon excessivo, glassmorphism indiscriminado, excesso
de cards iguais e animações chamativas.

## 2. Princípios

1.  Produto real é o protagonista.
2.  Resultado vem antes de feature.
3.  Screenshots reais têm prioridade sobre ilustrações genéricas.
4.  Atendimento humano deve ser apresentado como parte da solução.
5.  Não inventar clientes, depoimentos, logos, certificações ou
    métricas.
6.  Não usar claims quantitativos sem evidência.
7.  Não prometer capacidades de IA inexistentes.

## 3. Auditoria obrigatória

Antes de modificar código: - localizar rota e componentes da landing; -
mapear layout público, design tokens, tipografia e componentes
compartilhados; - verificar biblioteca de ícones e animações; -
localizar assets e screenshots disponíveis; - verificar CTA
`Solicitar demonstração` e seu destino; - verificar `Entrar`; - revisar
SEO/metadata; - revisar mobile, acessibilidade e testes; - medir
dependências client-side e performance quando possível.

Produzir: \| Área \| Estado atual \| Problema \| Proposta \| Impacto \|
\|---\|---\|---\|---\|---\|

Somente depois iniciar mudanças estruturais.

## 4. Nova arquitetura narrativa

Organizar: 1. Navbar 2. Hero + Product Demo 3. Problem / Pain 4. How It
Works 5. Product Showcase 6. Bento Product Capabilities 7. Benefits /
Outcomes 8. Control & Security 9. Target Audience 10. FAQ 11. Final
Conversion CTA 12. Footer

## 5. Navbar

Desktop:
`Clinic Assistant | Como funciona | Produto | Recursos | Segurança | FAQ | Entrar | [Solicitar demonstração]`

Requisitos: - sticky discreto; - altura compacta; - CTA visível; -
anchors; - scroll suave respeitando `prefers-reduced-motion`; - menu
mobile acessível; - Escape e focus management.

## 6. Hero

O Hero deve responder em segundos: o que é, para quem é, qual problema
resolve e qual próximo passo.

Direção de headline:
`Transforme o WhatsApp da sua clínica em uma recepção inteligente.`

Subheadline:
`Automatize disponibilidade, agendamentos, confirmações e atendimento pelo WhatsApp sem perder o controle da operação.`

CTA primário: `Solicitar demonstração` CTA secundário:
`Ver como funciona`

Trust hints, somente se suportados: - Sem aplicativo para o paciente -
Atendimento humano quando necessário - Agenda integrada

Layout desktop: copy à esquerda e Product Demo à direita. Mobile: copy →
CTA → trust hints → demo.

Manter dark/navy como identidade, com gradient mesh e glow azul
extremamente sutis.

## 7. Hero Product Demo

Evoluir a demonstração atual para uma história conectando WhatsApp e
Agenda:

Paciente: `Tem cardiologista amanhã?` → Clinic Assistant apresenta
horários → paciente escolhe `10:30` → confirmação →
`Consulta confirmada ✓` → pequeno card da Agenda aparece:
`10:30 — Nova consulta`

Criar animação controlada de aproximadamente 10--15s: - mensagens
progressivas; - typing indicator curto; - horários; - confirmação; -
evento na agenda; - pausa e reinício suave.

Requisitos: - sem backend; - sem áudio; - sem timers frágeis; -
desmontagem limpa; - reduced motion exibe estado final estático; - opção
discreta `Reproduzir novamente`, se útil.

## 8. Problem Section

Headline sugerida:
`Sua recepção não deveria passar o dia respondendo as mesmas perguntas.`

Exemplos visuais: - "Tem horário amanhã?" - "Qual médico atende
cardiologia?" - "Quero remarcar." - "Pode cancelar minha consulta?"

Depois mostrar:
`Clinic Assistant → Paciente atendido + Agenda atualizada + Recepção acompanhando`

Evitar quatro cards genéricos iguais. Preferir composição editorial
assimétrica ou Bento.

## 9. How It Works

Preservar a lógica real:
`Paciente → WhatsApp → Clinic Assistant → Agenda / Profissionais / Disponibilidade / Atendimento humano → Recepção`

Implementar diagrama com HTML/CSS/SVG próprio: - responsivo; -
acessível; - desktop horizontal/radial; - mobile vertical; - conexões
sutis; - hover/focus destaca nó e conexão; - sem animação contínua
distrativa.

## 10. Product Showcase

Seção prioritária.

Headline: `Uma visão completa da operação.`

Supporting:
`WhatsApp, agenda e atendimento humano conectados em um único fluxo.`

Tabs: `Dashboard | Agenda | Conversas`

Cada tab deve usar screenshot real e atual do produto, com dados
fictícios e sem PII, tokens, credenciais ou URLs internas sensíveis.

Dashboard: destacar KPIs, tendências, fila, alertas e próximas
consultas. Agenda: destacar Dia, Semana, Mês, Lista, filtros, bloqueios
e disponibilidade. Conversas: destacar histórico, fila humana, handoff e
contexto.

Usar frame de browser/app elegante, imagens otimizadas e transição curta
entre tabs.

## 11. Bento Product Capabilities

Criar composição assimétrica semelhante conceitualmente a:

`WhatsApp grande | Agenda` `Fila humana     | Dashboard grande`

Cada bloco comunica uma ideia. Usar mini-UI, ícones e labels. Evitar
parágrafos longos, excesso de cor e cards todos iguais.

Não usar números decorativos que possam ser interpretados como claims
comerciais.

## 12. Benefits / Outcomes

Promover a mensagem:
`Menos tarefas repetitivas. Mais controle para a recepção.`

Avaliar:
`Menos tarefas repetitivas. Mais tempo para cuidar dos pacientes.`

Benefícios: - atendimento mais organizado; - agenda centralizada; -
menos troca de contexto; - continuidade entre automação e humano; -
histórico operacional; - visibilidade da operação.

Opcionalmente criar comparação Before/After sem desqualificar o trabalho
humano.

## 13. Controle e Segurança

Apresentar somente capacidades reais: - controle de acesso; - isolamento
por clínica; - auditoria; - histórico operacional; - atendimento humano.

Não declarar LGPD/HIPAA/ISO sem validação formal.

## 14. Público-alvo

Seção curta: `Feito para quem vive a rotina da clínica.`

-   Clínicas médicas
-   Consultórios
-   Clínicas multidisciplinares
-   Pequenas redes

Não ampliar para outros mercados nesta etapa.

## 15. FAQ

Perguntas mínimas: - O paciente precisa instalar algum aplicativo? - O
sistema funciona pelo WhatsApp? - Posso assumir uma conversa
manualmente? - Posso utilizar vários profissionais? - É possível
controlar horários e bloqueios? - Como funciona a implantação?

Accordion acessível com teclado, `aria-expanded`, focus e animação
curta.

## 16. CTA final

Headline:
`Veja o Clinic Assistant funcionando na rotina da sua clínica.`

Supporting:
`Conheça o fluxo completo, do WhatsApp à agenda e ao atendimento da recepção.`

CTA: `Solicitar demonstração`

Auditar o destino real do CTA. Se houver formulário, usar campos
mínimos: - Nome - Clínica - E-mail ou WhatsApp - Quantidade de
profissionais (opcional)

Backend, se criado, deve ter validação, rate limiting, proteção
anti-spam e tratamento de erros. Não inventar fluxo inseguro.

Avaliar CTA fixo discreto no mobile após sair do Hero, sem cobrir
conteúdo.

## 17. Design System

Preservar identidade dark navy + off-white + light blue/gray + primary
blue.

Azul primário para CTA, selected states, links e pequenos detalhes.
Evitar grandes faixas alternadas sem ritmo.

Auditar tipografia antes de trocar. Usar escala responsiva e
`clamp()`/tokens quando adequado.

Padronizar espaçamento, radius, borders e sombras.

Glassmorphism apenas em locais pontuais como Hero Demo/Navbar/Bento,
nunca em toda página.

Usar biblioteca de ícones existente e não misturar estilos.

## 18. Microinterações

Aplicar apenas em: - CTA hover; - tabs; - accordion; - Hero Demo; -
diagrama; - screenshot transition; - navbar; - Bento hover.

Durações curtas. Implementar reduced motion.

## 19. Performance

-   `next/image` ou equivalente;
-   AVIF/WebP quando suportado;
-   lazy loading abaixo da dobra;
-   evitar vídeo pesado;
-   evitar bibliotecas de animação quando CSS resolve;
-   não transformar toda landing em Client Component;
-   code splitting;
-   screenshots dimensionados;
-   minimizar CLS.

Em Next.js, manter seções estáticas como Server Components quando
possível. Client Components somente para interações.

## 20. SEO

Atualizar: - title; - description; - Open Graph; - Twitter/X metadata; -
canonical; - robots; - sitemap quando aplicável; - favicon/manifest
quando aplicável.

Landing indexável. Área autenticada não indexável.

Avaliar JSON-LD apenas se semanticamente correto. Não inventar ratings,
reviews, preços ou dados de organização.

Criar social preview sem dados de pacientes.

## 21. Acessibilidade

Meta: WCAG 2.2 AA.

Validar: - contraste; - headings; - landmarks; - teclado; - focus
visible; - accordion; - tabs; - menu mobile; - links; - CTA; - alt
text; - SVG accessibility; - touch targets; - reduced motion; - zoom
200%.

## 22. Responsividade

Validar: `375, 390, 430, 768, 1024, 1280, 1440`

Sem overflow horizontal.

## 23. Screenshots e dados fake

Screenshots devem ser ricos e plausíveis, mas usar exclusivamente dados
fictícios.

Documentar processo em: `docs/product/landing-screenshots.md`

Incluir ambiente, seed, viewport, páginas, tratamento de dados e
diretório de assets.

Organizar assets conforme convenção do projeto, por exemplo:
`public/landing/product` `public/landing/diagrams` `public/landing/og`

## 24. Analytics

Somente se analytics já estiver aprovado/configurado, instrumentar: -
landing_view - hero_demo_started - hero_demo_completed -
how_it_works_clicked - product_tab_changed - demo_cta_clicked -
login_clicked - faq_opened - lead_started - lead_submitted

Não instalar tracker externo sem necessidade. Nunca enviar PII em
analytics.

## 25. Claims

Validar no código antes de publicar.

Claims funcionais possíveis, quando confirmados: - agendamento pelo
WhatsApp; - disponibilidade; - confirmação; - reagendamento; -
cancelamento; - agenda; - fila humana; - conversas; - auditoria; -
controle de acesso; - operação WhatsApp.

Proibido sem evidência: - economia de X%; - redução de custos X%; -
aumento de faturamento X%; - conversão X%; - "zero erros"; - "IA
avançada"; - "LGPD compliant"; - "24/7" se não garantido.

## 26. Componentes sugeridos

Criar/reutilizar somente quando necessário: - LandingNavbar -
HeroSection - HeroProductDemo - ProblemSection - HowItWorksDiagram -
ProductShowcase - ProductTabs - ProductScreenshot - CapabilitiesBento -
BenefitsSection - SecuritySection - TargetAudienceSection - FaqSection -
FinalCtaSection - LandingFooter - DemoRequestForm

Seguir estrutura atual do projeto; não reorganizar o frontend inteiro.

## 27. Fases

### 9.8.1.1 Auditoria

Landing, design system, assets, CTA, SEO, performance e testes.

### 9.8.1.2 Foundation

Layout, typography, spacing, color rhythm e navbar.

### 9.8.1.3 Hero

Copy, CTA, demo, motion e responsive.

### 9.8.1.4 Storytelling

Problema, How It Works e diagrama.

### 9.8.1.5 Product Showcase

Screenshots, tabs e descriptions.

### 9.8.1.6 Bento

WhatsApp, Agenda, Dashboard e fila.

### 9.8.1.7 Conversion

Benefícios, segurança, público, FAQ, CTA final e lead flow.

### 9.8.1.8 Hardening

Mobile, accessibility, SEO, performance e analytics.

### 9.8.1.9 Tests

Unit, Playwright e visual regression se já existente.

### 9.8.1.10 Documentation

Copy, screenshots e relatório.

## 28. Ordem de entrega

Primeira entrega:
`Auditoria → Navbar → Hero → Hero Demo → Problem → How It Works → Product Showcase → responsive base`

Validar antes de avançar.

Segunda entrega:
`Bento → Benefits → Security → Target Audience → FAQ → Final CTA → Lead flow → SEO → analytics → hardening`

## 29. Testes

Unitários somente para lógica relevante: demo state, tabs, validação de
lead e helpers.

Playwright desktop: - landing; - navbar; - CTA; - Ver como funciona; -
demo chega ao estado final; - tabs Dashboard/Agenda/Conversas; - FAQ; -
CTA final; - Entrar.

Playwright mobile 390x844: - menu; - Hero; - CTA; - demo; - diagrama; -
tabs; - Bento; - FAQ; - CTA final; - sem overflow.

Se visual regression já existir, capturar Hero e landing desktop/mobile.

Quando possível, executar Lighthouse e registrar Performance,
Accessibility, Best Practices e SEO. Não perseguir nota 100
artificialmente.

## 30. Documentação

Criar/atualizar: - `docs/product/landing-copy.md` -
`docs/product/landing-screenshots.md` - documentação frontend da landing
já existente; - documentação E2E da landing já existente.

`landing-copy.md` deve centralizar headline, subheadline, benefícios,
CTAs, FAQ, claims permitidos e proibidos.

## 31. Restrições

Não: - reescrever o app; - trocar framework; - alterar backend sem
necessidade; - trocar identidade completamente; - usar vídeo pesado; -
instalar biblioteca de animação sem justificar; - copiar landing de
concorrente; - inventar testimonials/logos/métricas; - usar PII; -
apresentar demo visual como chatbot real; - quebrar login ou rotas
privadas; - remover testes; - usar `test.skip` para concluir.

## 32. Validação técnica

Usar scripts reais existentes no projeto. Quando aplicáveis:

``` bash
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1
```

Não inventar scripts inexistentes. Se algum não existir, documentar e
usar equivalente real.

## 33. Critérios de aceite

Visual: - Hero de maior impacto; - demo integrada; - menos aparência de
sequência de cards; - Product Showcase real; - Bento assimétrico; -
tipografia consistente; - CTAs claros; - mobile refinado.

Comercial: - proposta compreensível em segundos; - dor explícita; -
solução demonstrada; - produto real visível; - benefícios claros; -
humano valorizado; - segurança e público claros; - nenhum claim sem
evidência.

Técnico: - lint/build/typecheck/tests/E2E PASS conforme scripts
existentes; - login e rotas privadas sem regressão; - sem overflow; -
reduced motion; - imagens otimizadas; - SEO e acessibilidade
revisados; - screenshots sem dados reais; - documentação atualizada.

## 34. Relatório final obrigatório

Apresentar: 1. auditoria anterior; 2. problemas; 3. narrativa final; 4.
Hero; 5. demo; 6. Problem; 7. How It Works; 8. Product Showcase; 9.
screenshots; 10. Bento; 11. benefícios; 12. segurança; 13. público; 14.
FAQ; 15. CTAs; 16. lead flow; 17. analytics; 18. SEO; 19.
acessibilidade; 20. responsividade; 21. performance; 22. testes; 23.
dependências; 24. documentação; 25. pendências; 26. riscos; 27. próximos
experimentos de conversão.

## 35. Resultado esperado

A landing não deve apenas dizer: `Temos WhatsApp, Agenda e Dashboard.`

Ela deve fazer o visitante pensar:
`Isso resolve um problema real da minha recepção.`

E então: `Quero ver isso funcionando na minha clínica.`

Esse é o objetivo central da Etapa 9.8.1.
