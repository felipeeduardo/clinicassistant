# Auditoria e refinamento da página de login

## Escopo

Refinamento exclusivo da rota pública `/login` da IA Recepção. Autenticação, API, JWT, sessão, permissões e redirect foram preservados.

## Diagnóstico anterior

| Área | Estado anterior | Problema | Correção |
|---|---|---|---|
| Branding | Painel navy com muito espaço vazio | Pouco contexto operacional | Trust points e mini preview abstrato |
| Card | Formulário funcional isolado | Hierarquia e conexão visual limitadas | Card com superfície, borda e sombra sutis |
| Senha | Campo simples | Sem controle de visibilidade | Mostrar/ocultar senha com botão acessível |
| Navegação | Sem retorno explícito | Saída para o site pouco clara | Link “Voltar ao site” |
| Mobile | Branding não priorizado | Poderia ocupar espaço antes do formulário | Painel reduzido a marca e rodapé em telas pequenas |

## Alterações

- Headline do painel: “Atendimento, agenda e conversas conectados à operação da clínica.”
- Supporting copy orientada à rotina da equipe.
- Trust points para agenda, atendimento humano e histórico operacional.
- Mini preview abstrato com horários e estados operacionais, sem métricas comerciais.
- Rodapé do painel com copyright.
- Card de login com copy “Entre para acompanhar a operação da sua clínica.”
- Placeholders não sensíveis para e-mail e senha.
- `autocomplete="email"` e `autocomplete="current-password"` mantidos explícitos.
- Toggle de senha com `aria-label`, foco visível e teclado.
- Loading preservado e explicitado como “Entrando...”, sem submit duplicado.
- Mensagem de autenticação amigável, sem status HTTP ou detalhes técnicos.
- Microcopy de acesso restrito ao ambiente autorizado da clínica.
- Link de retorno à Landing; nenhum link de recuperação foi criado porque não há fluxo real implementado.

## Tokens e consistência visual

Foram reutilizados tokens e componentes existentes: `brand-*`, `slate-*`, `surface`, `rounded-control`, `rounded-panel`, `shadow-panel`, `BrandLockup`, `Button`, `Input`, `FormField` e `Icon`. O navy do painel usa a mesma referência visual já presente na Landing (`#0f203d`); não foi criado tema paralelo de login nem dependência nova.

Os estilos novos usam apenas variações de transparência da paleta existente para bordas, glow e preview. Não foram adicionadas cores de status, claims de segurança ou imagens genéricas.

## Responsividade e acessibilidade

- Desktop: composição equilibrada em duas colunas.
- Tablet: painel de marca reduzido e formulário preservado sem compressão.
- Mobile: apenas marca e copyright no painel; formulário é a prioridade.
- Labels permanecem associados aos campos.
- Erros usam `FormErrorSummary`, `aria-invalid` e `aria-describedby`.
- Toggle de senha não entra como elemento decorativo e possui nome acessível.
- Enter continua submetendo o formulário.
- Foco visível segue o estilo global.
- Não há `overflow-x-hidden` específico para mascarar problemas.

## Auditoria de findings

- **CRITICAL corrigidos:** ausência de contexto no painel, falta de toggle de senha, navegação de retorno pouco clara.
- **MINOR corrigidos:** hierarquia do card, espaçamento, preview, microcopy, loading visível e consistência de marca.
- **FUTURE:** recuperação de senha, Política de Privacidade e Termos, screenshots de QA visual automatizado.

## Validação

- `npm run lint`
- `npm run typecheck`
- `npm run test -- --run`
- `npm run build`
- QA manual conforme [manual-qa-login.md](../testing/manual-qa-login.md); o E2E Playwright está temporariamente fora da pipeline.

O smoke E2E deve ser executado com a infraestrutura local disponível. Ele cobre marca, campos, autocomplete, toggle de senha, CTA e viewport mobile sem overflow.
