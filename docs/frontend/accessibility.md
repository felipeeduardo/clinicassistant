# Acessibilidade do frontend

O frontend adota HTML semântico, labels associados, estados ARIA, drawers modais e navegação responsiva. A meta é WCAG 2.2 AA nas telas operacionais.

## Cobertura automatizada

- botões carregando são desabilitados e informam `aria-busy`;
- campos possuem label, ajuda/erro associado por `aria-describedby` e `aria-invalid` em erro;
- drawers e menu móvel possuem papel modal, nome acessível e fecham por `Escape`;
- tabelas reutilizáveis emitem cabeçalhos de coluna semânticos;
- a suíte de componentes valida permissões de menu e controles principais.

## Checklist manual de release

- [ ] Percorrer login, dashboard, pacientes, agenda, conversas, WhatsApp e configurações usando somente teclado.
- [ ] Confirmar ordem de foco, foco visível e fechamento por `Escape` em drawer e menu móvel.
- [ ] Verificar contraste de textos, badges, botões, erro e foco com ferramenta de contraste.
- [ ] Conferir zoom de 200% e viewport móvel, sem perda de conteúdo ou operação.
- [ ] Validar nomes acessíveis de botões de ícone, labels de formulário e cabeçalhos de tabela.
- [ ] Validar mensagens de erro e carregamento com leitor de tela.

Registre o resultado da validação manual na evidência de release; testes automatizados não substituem a inspeção com navegador e leitor de tela.
