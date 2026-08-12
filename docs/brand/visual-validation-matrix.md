# Matriz de validação visual — IA Recepção

## Viewports obrigatórios

| Viewport | Landing | Login | Sidebar | Showcase | Status |
|---:|---|---|---|---|---|
| 375px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 390px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 430px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 768px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 1024px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 1280px | [ ] | [ ] | [ ] | [ ] | Pendente |
| 1440px | [ ] | [ ] | [ ] | [ ] | Pendente |

## Critérios

- sem overflow horizontal ou corte de conteúdo;
- lockup legível em fundo claro e escuro;
- favicon/app icon reconhecível em 16, 32 e 64px;
- foco visível em links, tabs, botões e campos;
- contraste suficiente para texto e controles;
- sidebar expandida e recolhida preserva contexto;
- `app.iarecepcao.com.br` aparece apenas como preview quando o ambiente ainda é local.

## Como executar

1. Rode `npm run dev` dentro de `frontend`.
2. Abra `/`, `/login` e uma tela autenticada.
3. Use o DevTools para testar cada viewport.
4. Marque a matriz somente após conferir visualmente.
5. Registre screenshots aprovados no relatório de release, sem dados reais.
