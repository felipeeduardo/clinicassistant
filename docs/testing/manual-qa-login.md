# QA manual — página de login

Este roteiro substitui temporariamente a execução E2E automatizada da página de login. A pipeline valida compilação, lint, typecheck e testes unitários; o job Playwright foi removido até a retomada da infraestrutura E2E.

## Preparação

1. Suba o ambiente local:

   ```bash
   docker compose up -d --build
   ```

2. Acesse `http://localhost:3000/login`.
3. Use um usuário administrativo existente no banco local. Não registre senha em documentação ou no repositório.

## Checklist funcional

- [ ] A marca IA Recepção aparece no painel e no mobile.
- [ ] O painel apresenta contexto da operação, trust points e preview de agenda.
- [ ] Os campos E-mail e Senha aparecem com labels associados.
- [ ] O campo de e-mail aceita autocomplete de e-mail.
- [ ] O campo de senha aceita autocomplete de senha atual.
- [ ] O botão Mostrar senha alterna para texto e Ocultar senha retorna para password.
- [ ] Enter no formulário executa o login.
- [ ] Durante a requisição o botão fica desabilitado e exibe “Entrando...”.
- [ ] Credenciais válidas redirecionam para `/dashboard`.
- [ ] Credenciais inválidas exibem mensagem amigável, sem stack trace ou status HTTP.
- [ ] O link “Voltar ao site” retorna para `/`.

## Checklist responsivo

Validar em 1440×900, 1366×768, 768×1024 e 390×844:

- [ ] Nenhum conteúdo fica cortado verticalmente.
- [ ] Não existe rolagem horizontal.
- [ ] No mobile, o formulário permanece prioritário e os campos não ficam comprimidos.
- [ ] O foco por teclado é visível e a ordem segue marca, campos, toggle, botão e retorno.

## Registro do QA

Anote data, ambiente, usuário de teste, viewport e resultado. Para falhas, registre a URL, passos para reproduzir, mensagem exibida e screenshot sem dados sensíveis.
