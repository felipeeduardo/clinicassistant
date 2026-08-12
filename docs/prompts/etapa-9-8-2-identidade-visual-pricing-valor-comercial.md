# Etapa 9.8.2 --- Identidade Visual, Pricing, Valor Comercial e Preparação para Validação Real

## Contexto

A Etapa 9.8.1 foi concluída. A 9.8.2 prepara o Clinic Assistant para
demonstrações comerciais e testes reais com clínicas, com duas frentes:
identidade visual própria e comunicação clara de investimento/valor na
landing.

## 1. Objetivos

Ao final, entregar: identidade visual consistente; pelo menos 5
conceitos de símbolo/ícone comparáveis; especificação de
logo/favicon/app icon; assets e guidelines; pricing configurável;
comunicação de valor; simulador de impacto baseado em premissas
explícitas; CTAs para demo/piloto; testes e documentação.

Princípio obrigatório: não publicar hipótese como resultado comprovado,
não inventar clientes, depoimentos, métricas, certificações ou ROI.

## 2. Auditoria obrigatória

Antes de alterar código, auditar logo, favicon, ícones, nome Clinic
Assistant, cores, tipografia, assets, Open Graph, manifest, navbar,
login, landing, sidebar, copy, CTA, lead flow, pricing existente,
analytics, planos/tenancy, limites reais, Twilio/WhatsApp e custos
operacionais conhecidos.

Produzir:

  --------------------------------------------------------------------------
  Item           Estado atual   Problema       Proposta       Requer decisão
                                                              comercial?
  -------------- -------------- -------------- -------------- --------------

  --------------------------------------------------------------------------

## 3. Identidade visual --- direções

Criar no mínimo 5 conceitos distintos, sem escolher silenciosamente um
vencedor. Para cada um: racional, símbolo, relação com o produto, forma,
sensação, vantagens, riscos, 16/32/64px, fundo claro/escuro e
monocromático.

### A --- Conversation + Calendar

Combinação abstrata de balão, agenda/check e fluxo. Não copiar WhatsApp.

### B --- CA Monogram

Monograma geométrico C+A, minimalista, SaaS e reconhecível como app
icon.

### C --- Smart Reception

Símbolo abstrato de mensagem → organização → confirmação.

### D --- Care + Connection

Cuidado, conexão, paciente e clínica, evitando cruz médica genérica
dominante.

### E --- Pulse / Flow

Linha contínua representando conversa → agenda → atendimento, com
referência sutil a fluxo/pulso.

Não incorporar logos do WhatsApp, Twilio, concorrentes ou símbolos
proprietários na marca principal.

## 4. Exploração visual

Criar `docs/brand/visual-directions.md`, descrevendo moodboard por
atributos: minimal, geometric, friendly, premium B2B SaaS, health-tech,
conversational, operational.

Se o ambiente não puder gerar imagens, não inventar PNGs. Criar
wireframes SVG simples e `docs/brand/icon-generation-prompts.md` com um
prompt por conceito, solicitando símbolo vetorial, flat, minimalista,
sem texto, sem 3D, sem cruz médica dominante, sem copiar WhatsApp e
legível como favicon.

Criar `docs/brand/icon-evaluation-matrix.md` com escala 1--5 e pesos:
reconhecimento 20%, simplicidade 15%, relação com produto 15%,
diferenciação 15%, escalabilidade 10%, favicon/app icon 10%, dark/light
5%, monocromático 5%, longevidade 5%. A escolha final deve ficar
`APPROVAL REQUIRED`.

## 5. Sistema de marca

Preparar arquitetura para brand-mark, wordmark, horizontal lockup,
compact lockup, favicon, app icon, monochrome, dark-background e
light-background. Não substituir definitivamente o logo antes de
aprovação humana.

Auditar e consolidar tokens: primary, accent, background, surface,
foreground, muted, border, success, warning, destructive, focus e
gradients. Evitar cores hardcoded.

Avaliar tipografia por legibilidade, português, performance, headings,
UI e números. Não trocar apenas por novidade.

Estrutura sugerida: `public/brand/logo`, `icon`, `favicon`, `social`,
`illustrations`, adaptada ao projeto.

## 6. Linguagem de imagens

Definir três famílias: Conversational UI (mensagens/agendamento),
Product UI (Dashboard/Agenda/Conversas reais) e Abstract Brand Graphics
(fluxo, agenda, conexão, organização). Produto real deve ter prioridade
sobre fotos genéricas.

## 7. Objetivo comercial da landing

A landing deve responder: quanto custa, o que inclui, para qual tamanho
de clínica serve, que problema econômico/operacional resolve, como
estimar impacto, se há implantação/piloto e qual próximo passo.

## 8. Pricing como hipótese validável

Verificar se já existe decisão comercial. Se não houver, implementar
pricing por configuração central e marcar como experimental. Nunca
espalhar valores hardcoded.

Documentar em `docs/product/pricing-strategy.md` pelo menos: -
mensalidade fixa; - faixa por profissionais; - base + consumo; - piloto
para clínicas parceiras.

Para cada modelo: vantagens, riscos, impacto em margem e recomendação
para MVP. Exemplos conceituais não são preço final.

## 9. Custos variáveis

Reconhecer e validar antes de publicar: WhatsApp/Twilio, infraestrutura,
e-mail, observabilidade, storage e IA futura. Não prometer ilimitado sem
análise.

## 10. Seção Pricing

Evitar 4--5 planos artificiais. Preferir 1 plano MVP ou no máximo 2
níveis. Exibir preço aprovado, periodicidade, inclusões, limites,
implantação se houver, custos variáveis quando aplicável e CTA.

Se preço não estiver aprovado, usar `Piloto para clínicas parceiras` +
`Solicitar proposta`, sem inventar preço.

Suportar modos comerciais configuráveis: - `demo` → Solicitar
demonstração; - `pilot` → Participar do piloto; - `publicPricing` →
mostrar preços aprovados.

Criar uma única implementação com configuração central para
mostrar/ocultar pricing, calculator, pilot badge, CTA, preço,
implantação e observações.

## 11. Copy comercial

Revisar a landing removendo jargão técnico. Direções possíveis: - Hero:
`Sua recepção mais organizada, do WhatsApp à agenda.` - Valor:
`Automatize o trabalho repetitivo sem tirar sua equipe do controle.` -
Investimento:
`Um investimento previsível para organizar uma das principais portas de entrada da sua clínica.` -
ROI:
`Calcule quanto tempo da sua equipe hoje é consumido por tarefas repetitivas.`

Não publicar políticas comerciais inexistentes.

## 12. Onde o Clinic Assistant gera valor

Explicar potencial de valor em: tempo da recepção, capacidade de
resposta, agenda, continuidade automação/humano e gestão. Usar linguagem
de potencial, não garantia.

## 13. Simulador de impacto/ROI

Criar calculadora transparente, client-side quando suficiente, com
inputs como: atendentes, custo mensal médio da equipe, solicitações/dia,
minutos por solicitação repetitiva, dias úteis, percentual estimado
automatizável e mensalidade do Clinic Assistant.

Documentar em `docs/product/value-calculator.md`.

Fórmulas conceituais:

    minutos_repetitivos_mes = solicitacoes_dia × minutos_por_solicitacao × dias_uteis
    minutos_potencialmente_automatizaveis = minutos_repetitivos_mes × percentual_automatizavel
    horas_potencialmente_liberadas = minutos_potencialmente_automatizaveis / 60
    custo_hora_estimado = custo_mensal_equipe / horas_mensais_trabalho
    valor_tempo_potencial = horas_potencialmente_liberadas × custo_hora_estimado
    impacto_estimado = valor_tempo_potencial - investimento_saas

As fórmulas são estimativas, não promessa de economia.

Resultado preferido: horas potencialmente liberadas/mês, valor
equivalente de tempo, investimento e diferença estimada. Não chamar
automaticamente de lucro.

Disclaimer obrigatório:
`Estimativa baseada nas informações fornecidas. Resultados reais dependem da operação, adesão, volume de atendimentos e configuração da clínica.`

Opcional: cenários Conservador, Moderado e Personalizado, com premissas
documentadas e nunca tratados como benchmarks comprovados.

## 14. Valor não financeiro

Comunicar organização, rastreabilidade, menor troca de contexto,
experiência do paciente, visibilidade, handoff humano e padronização sem
monetização fictícia.

## 15. CTA e piloto

Após pricing/calculadora: `Quer validar esse cenário na sua clínica?`
com `Solicitar demonstração` ou `Quero participar do piloto`, conforme
modo comercial.

Se houver formulário, avaliar Nome, Clínica, Contato, Número de
profissionais, Volume aproximado e Principal dificuldade; nem tudo
precisa ser obrigatório e nunca coletar dados de pacientes.

## 16. Analytics

Somente se analytics já estiver aprovado: pricing_viewed,
pricing_cta_clicked, roi_calculator_started/completed,
pilot_cta_clicked, demo_cta_clicked, lead_submitted. Não enviar PII.

Criar `docs/product/commercial-experiments.md` para futuros testes de
headline, CTA, pricing visível vs proposta, piloto vs demo e calculator
presente vs ausente. Não instalar plataforma A/B nesta etapa.

## 17. UI

Pricing deve manter o padrão premium da 9.8.1, com 1--2 cards,
comparação clara e detalhes expansíveis. Calculator: desktop inputs à
esquerda e resultado à direita; mobile inputs → resultado. Formatar
BRL/pt-BR.

Criar documentação/showcase interno de logo, ícones, cores, tipografia,
buttons, dark/light e usos incorretos. Não expor rota interna em
produção se inadequado.

## 18. Testes

Unit: fórmulas, arredondamento, zero, percentuais, pricing config e
commercialMode.

E2E: pricing, calculator, inputs, resultado, disclaimer, CTA,
pilot/publicPricing, mobile, overflow e integridade de navbar/footer.

Segurança: nenhum secret, PII ou credencial Twilio na landing/analytics.

## 19. Documentação obrigatória

Criar/atualizar: - `docs/brand/visual-directions.md` -
`docs/brand/icon-generation-prompts.md` -
`docs/brand/icon-evaluation-matrix.md` -
`docs/brand/brand-guidelines.md` - `docs/product/pricing-strategy.md` -
`docs/product/value-calculator.md` -
`docs/product/commercial-experiments.md` -
`docs/product/landing-copy.md`

## 20. Aprovação humana obrigatória

Marcar `APPROVAL REQUIRED`: ícone/logo final, nome definitivo se
discutido, preço público, implantação, limites, cobrança por consumo,
piloto grátis/pago, claims quantitativos, publicação da calculadora e
uso de marcas de parceiros.

## 21. Ordem

9.8.2.1 Auditoria → 9.8.2.2 cinco conceitos visuais → 9.8.2.3 brand
foundation → 9.8.2.4 commercial copy → 9.8.2.5 pricing architecture →
9.8.2.6 pricing UI → 9.8.2.7 value calculator → 9.8.2.8
conversion/analytics → 9.8.2.9 tests → 9.8.2.10 documentation.

## 22. Restrições

Não substituir logo sem aprovação; inventar
preço/ROI/clientes/depoimentos; usar logo WhatsApp como identidade;
publicar claims jurídicos; quebrar landing 9.8.1/login/rotas privadas;
alterar cobrança real ou criar checkout sem requisito explícito;
adicionar dependência pesada sem justificativa; usar `test.skip` para
concluir.

## 23. Validação

Usar scripts reais. Quando aplicáveis:

``` bash
npm run lint
npm run typecheck
npm run test
npm run build
npm run test:e2e -- --workers=1
```

Se backend mudar:

``` bash
dotnet restore
dotnet build
dotnet test
```

## 24. Critérios de aceite

Identidade: 5 conceitos, prompts, matriz, favicon/app
icon/dark/light/mono considerados e escolha não imposta.

Comercial: copy intuitiva, pricing configurável, modos
demo/pilot/publicPricing, custos/limites transparentes e nenhum preço
fictício como definitivo.

Valor: calculator com fórmulas testadas, premissas explícitas e
disclaimer; sem lucro garantido.

Qualidade: responsivo, acessível, analytics seguro quando existente,
testes verdes, docs atualizadas e sem regressão da 9.8.1.

## 25. Relatório final

Apresentar auditoria, conceitos, recomendação, itens aguardando
aprovação, assets, tokens, copy, pricing, configuração comercial,
hipóteses, custos variáveis, calculator/fórmulas/disclaimers, CTAs,
analytics, testes, documentação, riscos e recomendação para primeiro
piloto real.

## Resultado esperado

A landing deve permitir ao potencial cliente compreender o que o Clinic
Assistant resolve, como entra na rotina, quanto pode custar, como
avaliar seu impacto e como solicitar demo/piloto --- sem transformar
hipóteses do MVP em promessas comerciais.
