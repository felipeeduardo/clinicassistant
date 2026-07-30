# Desenvolvimento local

Pré-requisitos: .NET SDK 10 e Docker Compose.

1. Copie `.env.example` para `.env` se precisar alterar portas ou credenciais locais.
2. Execute `docker compose up --build`.
3. Acesse Swagger em `http://localhost:8080/swagger`.
4. Consulte `http://localhost:8080/health/live` e `http://localhost:8080/health/ready`.

Para desenvolvimento fora de containers, inicie as dependências com `docker compose up postgres rabbitmq redis -d`, então execute `dotnet restore`, `dotnet build` e `dotnet test` na raiz.

O painel RabbitMQ estará em `http://localhost:15672` com as credenciais definidas no `.env`.
