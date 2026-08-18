# Testes de bootstrap e onboarding

Use somente e-mails e senhas fake de teste, fornecidos por variáveis do ambiente
de teste. Nunca use credenciais de produção.

Casos mínimos do serviço: flag desabilitada, criação inicial, reinício idempotente,
apenas um admin existente, ambos existentes, e-mail inválido, senha ausente,
senha fora da política e e-mail já usado por outra role. Verifique também que os
logs não contêm senha e que migrations não contêm hash bootstrap.

O E2E completo do wizard depende da evolução incremental das APIs de catálogo,
disponibilidade, ClinicAdmin, revisão e ativação.
