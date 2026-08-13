# Testes e validação local

Execute `dotnet build backend/ClinicAssistant.sln --no-restore` e `dotnet test backend/ClinicAssistant.sln --no-build --no-restore`. Para validar integração local, execute `docker compose up --build` e confirme `/health/ready`.

Use `WHATSAPP__PROVIDER=Fake` para testes sem rede. Não utilize AuthToken ou credenciais reais em fixtures, logs ou arquivos versionados.
