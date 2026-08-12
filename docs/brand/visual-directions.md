# Direções visuais — Clinic Assistant

## Auditoria

| Item | Estado atual | Problema | Proposta | Requer decisão comercial? |
|---|---|---|---|---|
| Logo | Monograma `CA` em CSS no app shell/login | Não existe símbolo aprovado nem lockup reutilizável | Avaliar cinco conceitos antes de substituir | Sim — `APPROVAL REQUIRED` |
| Favicon/app icon | SVG azul com balão e cruz interna | Funciona, mas ainda é provisório | Manter como fallback até aprovação do símbolo | Sim |
| Cores | Tokens Tailwind brand/slate/semantic | Alguns usos estão hardcoded nas telas | Consolidar tokens de marca e semânticos | Não |
| Tipografia | Sans nativa/Tailwind | Legível e rápida, sem necessidade de troca | Preservar até evidência contrária | Não |
| Navbar/login/sidebar | Identidade `CA` + wordmark textual | Falta sistema de lockups | Criar arquitetura `brand-mark`/`wordmark` | Sim para símbolo final |
| Landing/copy | Demo, showcase CSS e CTA `mailto` | Sem pricing aprovado ou lead flow | Modo comercial configurável, sem promessas | Sim |
| Analytics | Não configurado | Eventos não podem ser enviados | Documentar experimentos, não instalar plataforma | Sim |

## Cinco conceitos

### A — Conversation + Calendar

Balão abstrato conectado a uma folha de agenda e um check. Representa conversa, disponibilidade e confirmação sem copiar o WhatsApp. Forma arredondada, premium e amigável. Vantagem: relação imediata com o fluxo. Risco: pode ficar genérico em tamanhos pequenos. Funciona em 16/32/64px, fundos claro/escuro e monocromático.

### B — CA Monogram

`C` e `A` geométricos compartilhando uma única forma. Representa Clinic Assistant como produto SaaS e funciona sozinho como app icon. Vantagem: memorabilidade e compactação. Risco: exigir desenho proprietário para não parecer monograma genérico. Funciona melhor em favicon e lockup compacto.

### C — Smart Reception

Três nós conectados em sequência: mensagem, organização e confirmação. Vantagem: comunica automação + operação humana. Risco: excesso de elementos em 16px. Requer versão reduzida para favicon.

### D — Care + Connection

Duas formas curvas envolvendo um ponto central, sugerindo paciente, equipe e clínica conectados. Evita cruz médica dominante. Vantagem: proximidade e confiança. Risco: associação abstrata sem contexto em monocromático.

### E — Pulse / Flow

Linha contínua com três mudanças de ritmo, simbolizando conversa → agenda → atendimento. Vantagem: distintivo e escalável em motion/ambient graphics. Risco: leitura menos literal e possível associação com saúde clínica.

## Aplicações comuns

Cada direção deve ser desenhada em: `brand-mark`, `wordmark`, `horizontal lockup`, `compact lockup`, favicon 16/32/64px, app icon, dark, light e monocromático. A decisão final permanece **APPROVAL REQUIRED**.

## Adaptação para IA Recepção — Gate A

### A — Conversation + Calendar

Balão, agenda e confirmação em composição abstrata. Wordmark: `IA Recepção`; favicon reduzido a balão + check.

### B — IR Monogram

Monograma geométrico `IR` para IA Recepção, sem continuar o legado `CA`. Deve funcionar como mark, wordmark, lockup, favicon e app icon, com versões dark, light e monocromática.

### C — Smart Reception

Três nós conectados: mensagem, organização e confirmação. O nó triplo aparece na navbar/login; a versão reduzida usa dois nós em 16px.

### D — Care + Connection

Duas curvas envolvendo um ponto, representando paciente, equipe e clínica. Evita cruz médica e mantém leitura em dark/light.

### E — Pulse / Flow

Linha contínua em três ritmos para conversa → agenda → recepção. O favicon usa apenas o trecho central e não uma referência ECG literal.

**Decisão registrada:** direção **D — Care + Connection**, variação **1 — Abraço equilibrado**.

O símbolo aprovado para a próxima rodada usa duas curvas (azul-marinho e verde) envolvendo um ponto azul central. A primeira aplicação foi feita de forma gradual no componente `BrandLockup`, na tela de login, no cabeçalho público e no app icon. O nome legado `Clinic Assistant` permanece em textos de produto e metadados para preservar compatibilidade durante a transição.

**Próxima validação:** revisar SVG em 16/32/64px, contraste claro/escuro, wordmark e favicon antes de remover os fallbacks legados.
