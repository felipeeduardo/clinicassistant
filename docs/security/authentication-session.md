# Sessão e refresh token

O token de acesso permanece somente em memória no frontend. O refresh token é emitido em cookie `HttpOnly`, com `SameSite=Lax` e escopo `/api/auth`; ele não é serializado no JSON de login, cadastro ou refresh.

`POST /api/auth/refresh` lê o cookie, realiza rotação do refresh token e devolve somente um novo token de acesso e o perfil. A aplicação restaura a sessão automaticamente ao carregar. `POST /api/auth/logout` revoga o token presente no cookie e o remove.

Em produção, sirva a API por HTTPS para que o cookie seja marcado como `Secure`.
