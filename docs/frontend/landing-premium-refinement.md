# Refinamento premium da Landing Page

## Escopo

Refinamento visual da Landing Page pública da IA Recepção, sem alteração de APIs, autenticação, captação de leads, fórmulas comerciais ou área autenticada.

## O que foi aplicado

- Ambientação sutil no hero com grid e órbitas em azul marinho/azul já aprovados.
- Frame da demonstração do WhatsApp com profundidade e sequência visual de 10 segundos.
- Reveal progressivo das seções principais via `IntersectionObserver`.
- Animação única dos conectores do fluxo “Como funciona” e pulso discreto do nó IA Recepção.
- Elevação e sombra suaves no showcase do produto e nos cards operacionais.
- Transições curtas no frame do showcase, mantendo as abas Dashboard, Agenda e Conversas e a lógica existente.
- Fallback sem JavaScript e suporte a `prefers-reduced-motion`.

## Decisões preservadas

- A ordem e os textos estruturais da Landing Page permanecem os mesmos.
- Não foram adicionadas dependências visuais ou componentes de terceiros.
- Nenhum endpoint, evento de analytics, formulário ou CTA foi alterado.
- A paleta continua baseada nos tokens de marca existentes.

## Validação

Executado em `frontend/`:

```bash
npm run typecheck
npm run lint
npm run build
```

Os três comandos concluíram com sucesso. A validação visual deve ser feita nos breakpoints de 375, 390, 430, 768, 1024, 1280 e 1440 px, incluindo teclado e movimento reduzido.
