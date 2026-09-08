# Guidelines de design system

Agenda, Dashboard e Landing reutilizam `Button`, `Input`, `Select`, `Card`, `Drawer`, `StatusBadge`, `Skeleton` e `EmptyState`. Novos controles devem manter os tokens existentes de altura, radius, foco e estados disabled/error. A Landing usa composição Tailwind existente, sem biblioteca visual adicional.

Todos os CTAs precisam ter nome acessível, headings devem seguir hierarquia e animações futuras devem respeitar `prefers-reduced-motion`.

## Tamanhos de controles

`Input`, `Select` e `Textarea` aceitam `size="sm" | "md" | "lg"` (padrão `md`). Use `sm` em filtros compactos, `md` em formulários padrão e `lg` apenas em campos de maior destaque. O tamanho altera altura, espaçamento horizontal e tipografia de forma consistente.

## Páginas administrativas de cadastro

Pacientes, Profissionais, Especialidades e Unidades devem seguir o mesmo esqueleto visual:

- `PageHeader` com título, descrição objetiva e CTA principal à direita.
- Filtros agrupados em `FilterToolbar`, sempre com `FormField` e labels explícitos.
- Listagens em `DataTable`, com estados de `Skeleton`, `ErrorState` e `EmptyState`.
- Ações de linha agrupadas em `RowActions` e renderizadas com `RowActionButton`, usando ícone, nome acessível e tom visual por intenção.
- Ações primárias mantêm o vocabulário `Novo/Nova ...`; ações secundárias usam rótulos objetivos como `Visualizar`, `Editar`, `Ativar` e `Desativar`.
- Para manter a interface limpa, ações de linha devem exibir rótulos curtos (`Ver`, `Editar`, `Agenda`) e preservar o contexto completo no `aria-label`.

Evite criar padrões locais de botão, filtros ou cards quando os componentes compartilhados atenderem ao caso.
