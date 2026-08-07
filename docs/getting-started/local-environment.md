# Ambiente local

Os serviços são iniciados pelo `docker-compose.yml`: PostgreSQL (5432), RabbitMQ (5672/15672), Redis (6379), API (8080), Worker e frontend (3000). API, RabbitMQ, Redis e PostgreSQL possuem verificações de prontidão; o frontend é validado por `/login`.

Profiles: `e2e` usa o seeder determinístico e `twilio-smoke` é um override manual com ngrok. O padrão local permanece Fake, sem custo e sem chamadas externas.
