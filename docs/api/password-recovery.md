# Recuperação segura de senha

`POST /api/auth/forgot-password` sempre retorna uma mensagem genérica, independentemente de o e-mail existir. Para usuários ativos, é criado um token aleatório de uso único, armazenado apenas como SHA-256, com expiração configurável (`PasswordRecovery:TokenExpirationMinutes`, entre 5 e 120 minutos).

`POST /api/auth/reset-password` recebe `{ token, newPassword }`. A senha exige pelo menos 12 caracteres, maiúscula, minúscula, número e símbolo. Após o uso, o token é consumido e os refresh tokens ativos do usuário são revogados; não há login automático.

O envio é configurável via `Email:Provider=SendGrid`, `Email:FromAddress`, `Email:FromName` e `SendGrid:ApiKey` (Web API v3). Em `Email:Enabled=false` (padrão local), nenhum provedor é chamado e a solicitação continua genérica. Nunca registre tokens, links ou senhas.

Rotas públicas da interface: `/esqueci-minha-senha` e `/redefinir-senha?token=...`.

A tabela `password_reset_tokens` é criada pela migration `202608180002_PasswordResetTokens`. A aplicação deve ser reiniciada após configurar as variáveis SendGrid para que o link seja enviado pelo domínio oficial.
