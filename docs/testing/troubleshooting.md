# Solução de problemas

- **Migrations ausentes:** suba a API ou aplique migrations antes do seed; o runner exige `202607300009_HumanQueue`.
- **Banco bloqueado:** não reduza a proteção. Defina `TEST_DATA_ALLOWED_DATABASES` somente para um banco local de teste conhecido.
- **`psql` não encontrado:** instale o cliente PostgreSQL. O serviço Docker já o inclui.
- **Hash de senha falhou:** execute `dotnet restore` para restaurar o projeto auxiliar e defina `E2E_DEFAULT_PASSWORD`.
- **Validação de contagem falhou:** execute reset + seed do mesmo perfil. Não misture `minimal` e `e2e` no mesmo banco sem reset.
