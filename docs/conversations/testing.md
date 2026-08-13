# Testes

Os testes unitários cobrem máquina de estados, fundação da conversa, criação da resposta e Outbox, idempotência, isolamento de tenant e lock indisponível. Para validar Redis e RabbitMQ reais, execute Docker Compose e a suíte em um ambiente que permita conexões locais.

```bash
dotnet test backend/ClinicAssistant.sln --no-build --no-restore
docker compose up --build
```
