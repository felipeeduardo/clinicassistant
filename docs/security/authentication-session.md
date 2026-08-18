# Sessão e refresh token

O token de acesso permanece somente em memória no frontend. O refresh token é emitido em cookie `HttpOnly`, com `SameSite=Lax` e escopo `/api/auth`; ele não é serializado no JSON de login, cadastro ou refresh.

`POST /api/auth/refresh` lê o cookie, realiza rotação do refresh token e devolve somente um novo token de acesso e o perfil. A aplicação restaura a sessão automaticamente ao carregar. `POST /api/auth/logout` revoga o token presente no cookie e o remove.

Em produção, sirva a API por HTTPS para que o cookie seja marcado como `Secure`.

Uma chamada de `POST /api/auth/refresh` sem cookie (primeiro acesso, logout ou
sessão expirada) responde `401 Unauthorized` sem stack trace; isso é um estado
normal e leva o frontend à tela de login. Se o login funcionar mas o refresh
falhar repetidamente, confirme no navegador que o cookie foi aceito, que a API
está em HTTPS e que o domínio do frontend e da API está configurado para permitir
credenciais. Em produção, prefira `app.<domínio>` e `api.<domínio>` sob o mesmo
domínio registrável; para domínios distintos, a política SameSite/CORS precisa
ser revisada antes do deploy.
