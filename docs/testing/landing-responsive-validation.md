# Validação responsiva da Landing — Etapa 9.8.1

O Playwright da Landing foi mantido fora da execução conforme decisão da etapa. Use esta matriz para a validação visual manual:

| Viewport | Pontos de verificação |
|---|---|
| 375 × 812 | Menu, CTA, demo, tabs, FAQ e ausência de scroll horizontal |
| 390 × 844 | Hero, diagrama vertical, showcase e CTA final |
| 430 × 932 | Quebra de títulos, cards de benefício e touch targets |
| 768 × 1024 | Transição tablet e navegação pública |
| 1024 × 768 | Grid do showcase, diagrama e espaçamento |
| 1280 × 800 | Hero assimétrico, tabs e bento/benefícios |
| 1440 × 900 | Largura máxima, contraste, ritmo vertical e alinhamentos |

Critérios comuns: nenhum conteúdo cortado, nenhum overflow horizontal, foco visível por teclado, contraste legível e CTA acessível. A propriedade global `overflow-x-clip` evita que elementos decorativos criem rolagem lateral sem esconder conteúdo funcional.
