# Guidelines de design system

Agenda, Dashboard e Landing reutilizam `Button`, `Input`, `Select`, `Card`, `Drawer`, `StatusBadge`, `Skeleton` e `EmptyState`. Novos controles devem manter os tokens existentes de altura, radius, foco e estados disabled/error. A Landing usa composição Tailwind existente, sem biblioteca visual adicional.

Todos os CTAs precisam ter nome acessível, headings devem seguir hierarquia e animações futuras devem respeitar `prefers-reduced-motion`.

## Tamanhos de controles

`Input`, `Select` e `Textarea` aceitam `size="sm" | "md" | "lg"` (padrão `md`). Use `sm` em filtros compactos, `md` em formulários padrão e `lg` apenas em campos de maior destaque. O tamanho altera altura, espaçamento horizontal e tipografia de forma consistente.
