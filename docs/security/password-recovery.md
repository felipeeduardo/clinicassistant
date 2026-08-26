# Segurança da recuperação de senha

`POST /api/auth/forgot-password` sempre retorna a mesma mensagem, exista ou não
uma conta. O usuário é localizado no identity store (sem `TenantId` fornecido
pelo cliente), o token é aleatório, armazenado apenas como SHA-256, expira entre
5 e 120 minutos e é consumido uma única vez. Após o reset, refresh tokens ativos
são revogados.

O link usa `PasswordRecovery__FrontendBaseUrl`; não há URL hardcoded. Nunca
registramos token, link completo, HTML ou chave SendGrid. O endpoint usa o
limitador público existente; novas tentativas devem continuar genéricas para não
permitir enumeração.
