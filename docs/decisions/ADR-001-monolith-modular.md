# ADR-001 — Monólito modular

## Decisão

Usar um monólito modular com projetos de camadas separados.

## Motivo

O produto está no MVP e precisa de implantação simples, consistência transacional e fronteiras claras, sem o custo operacional de microsserviços. As portas na Application permitem extrair componentes futuramente quando houver necessidade comprovada.
