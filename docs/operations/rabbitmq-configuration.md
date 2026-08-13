# Configuração RabbitMQ

A aplicação usa uma configuração AMQP genérica. Ela não diferencia CloudAMQP de
RabbitMQ local.

| Variável | Obrigatória | Secret | Development | Production |
|---|---:|---:|---|---|
| `RabbitMq__Host` | Sim | Não | `localhost`/`rabbitmq` | Host do broker |
| `RabbitMq__Port` | Sim | Não | `5672` | Porta fornecida pelo broker |
| `RabbitMq__Username` | Sim | Sim | usuário local | usuário com acesso mínimo |
| `RabbitMq__Password` | Sim | Sim | senha local | secret manager |
| `RabbitMq__VirtualHost` | Sim | Não | `/` | vhost da instância |
| `RabbitMq__UseTls` | Sim | Não | `false` | `true` |
| `RabbitMq__ServerName` | Quando TLS | Não | vazio | hostname TLS/SNI |

`UseTls` é explícito. Quando `false`, a conexão usa AMQP normal; quando `true`,
o cliente habilita TLS e usa `ServerName`. Não existe fallback automático de TLS
para conexão sem criptografia e a validação padrão de certificado permanece ativa.

As opções de retry, topology recovery, exchanges, filas, bindings, payloads e
Outbox permanecem inalteradas.
