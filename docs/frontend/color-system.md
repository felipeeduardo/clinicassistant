# Sistema de cores — IA Recepção

## Fonte da paleta

A tela de login refinada é a referência visual. Os valores foram extraídos dos tokens Tailwind e estilos existentes, sem criar uma paleta paralela.

| Token semântico | Valor atual | Uso |
|---|---|---|
| `--color-brand-dark` | `#0f203d` | Login, sidebar e superfícies dark principais |
| `--color-brand-dark-surface` | `#1e293b` | Hover e superfície interna dark |
| `--color-brand-primary` | `#1d4ed8` | CTA, seleção, links e foco |
| `--color-brand-primary-hover` | `#1e40af` | Hover/active de ações primárias |
| `--color-brand-primary-subtle` | `#eff6ff` | Highlights e estados selecionados suaves |
| `--color-brand-secondary` | `#2563eb` | Accent e visualizações |
| `--color-brand-accent` | `#60a5fa` | Conectores e detalhes sobre dark |
| `--color-background` | `#f8fafc` | Canvas geral do app |
| `--color-background-subtle` | `#f1f5f9` | Superfície neutra secundária |
| `--color-surface` | `#ffffff` | Cards, formulários e drawers |
| `--color-foreground` | `#0f172a` | Texto principal |
| `--color-foreground-muted` | `#475569` | Texto secundário |
| `--color-border` | `#e2e8f0` | Bordas padrão |
| `--color-border-strong` | `#cbd5e1` | Inputs e controles |
| `--color-focus` | `#1d4ed8` | Foco visível |

Os aliases Tailwind equivalentes são `canvas`, `foreground-*`, `brand-dark-*`, `brand-primary-*` e `border-system-*`. Os tokens `brand-*` e `surface-*` existentes continuam compatíveis para evitar regressões.

## Cores semânticas

Status não são substituídos por azul:

- success: verde (`emerald` / `--color-success`)
- warning: âmbar (`amber` / `--color-warning`)
- destructive: vermelho (`red` / `--color-destructive`)
- info: azul brand (`--color-info`)

Confirmações, falhas, conflitos, cancelamentos e estados de conexão devem manter seu significado. Verde do WhatsApp só aparece quando representa conexão ou sucesso.

## Componentes

`Button`, `Input`, `Select`, `Textarea`, `Card` e `AppShell` consomem os aliases semânticos. Isso centraliza primary, hover, focus, canvas, surface, borda e sidebar sem alterar regras de negócio.

## Gráficos e visualizações

Usar uma sequência curta e coerente: primary blue, secondary blue, teal/green semântico, purple muted e amber semântico. Evitar arco-íris e não usar azul para representar severidade.

## Do / Don't

**Do:** usar tokens semânticos, manter neutros como base, reservar brand para interação e preservar status.

**Don't:** criar `login-navy`, substituir status cegamente, adicionar `bg-[#...]` para conceitos existentes, transformar todos os cards em azul ou criar dark mode nesta tarefa.

## Auditoria e limitações

A auditoria encontrou valores decorativos RGB no CSS da Landing (glows, conectores e mini-visuais) e cores próprias do logo SVG. Eles não representam tokens funcionais distintos e foram preservados para não alterar ilustrações aprovadas. A próxima limpeza pode migrá-los para `color-mix()`/variáveis de opacidade sem impacto visual.
