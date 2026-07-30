# Plataforma de dados de teste

Os scripts em `database/` complementam as migrations EF Core: migrations criam o schema; seeds inserem somente dados fictícios e determinísticos. O runner é `scripts/test-data` e executa SQL com `psql` em transações separadas por arquivo.

O hash de senha nunca é montado em SQL. Antes do seed, o runner chama `ClinicAssistant.TestDataHash`, que usa o `PasswordHasher` oficial (PBKDF2-SHA512) e entrega apenas o hash ao `psql`.

Não há envio real: telefones usam a faixa `+550000…`, emails usam domínios reservados e toda integração Twilio permanece `Disabled`, sem SID, token ou sender real.
