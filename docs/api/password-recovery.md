# Recuperação segura de senha

`POST /api/auth/forgot-password` sempre retorna uma mensagem genérica, independentemente de o e-mail existir. Para usuários ativos, é criado um token aleatório de uso único, armazenado apenas como SHA-256, com expiração configurável (`PasswordRecovery:TokenExpirationMinutes`, entre 5 e 120 minutos).

`POST /api/auth/reset-password` recebe `{ token, newPassword }`. A senha exige pelo menos 12 caracteres, maiúscula, minúscula, número e símbolo. Após o uso, o token é consumido e os refresh tokens ativos do usuário são revogados; não há login automático.

O envio é configurável via `PasswordRecovery:Provider=Smtp`, `From`, `SmtpHost`, `SmtpPort`, `SmtpUser`, `SmtpPassword` e `EnableSsl`. Em `Disabled` (padrão local), a solicitação continua genérica e não vaza o token nos logs; configure SMTP antes de habilitar em produção. Nunca registre tokens, links ou senhas.

Rotas públicas da interface: `/esqueci-minha-senha` e `/redefinir-senha?token=...`.

A tabela `password_reset_tokens` é criada pela migration `202608180002_PasswordResetTokens`. A aplicação deve ser reiniciada após configurar as variáveis SMTP para que o link seja enviado pelo domínio oficial.
