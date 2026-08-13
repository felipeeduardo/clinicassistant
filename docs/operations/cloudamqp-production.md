# Runbook CloudAMQP em produção

## Pré-requisitos

Crie um usuário CloudAMQP com permissões mínimas somente no VirtualHost usado pela
aplicação. Não coloque a URL AMQP completa em código, documentação, tickets,
screenshots ou logs.

## Dados a obter no painel CloudAMQP

1. Host DNS do broker.
2. Porta AMQPS/TLS fornecida pela instância; não assumir `5671`.
3. Username e password, cadastrados somente no Railway.
4. VirtualHost configurado.
5. Hostname esperado pelo certificado para SNI (`ServerName`).

## Railway API e Worker

Configure nos dois serviços:

```text
RabbitMq__Host=<host do broker>
RabbitMq__Port=<porta TLS fornecida>
RabbitMq__Username=<usuário>
RabbitMq__Password=<secret>
RabbitMq__VirtualHost=<vhost>
RabbitMq__UseTls=true
RabbitMq__ServerName=<hostname do certificado>
```

Depois de revisar as variáveis, faça redeploy da API e do Worker. O `/health/ready`
usa a mesma factory e as mesmas opções da conexão do Worker; ele não faz um teste
TCP paralelo com `localhost`.

## Validação

- [ ] DNS do host resolve.
- [ ] TLS negocia com o ServerName configurado.
- [ ] Certificado é validado pelo runtime; não há bypass.
- [ ] Usuário autentica no VirtualHost correto.
- [ ] API `/health/ready` retorna RabbitMQ healthy.
- [ ] Worker declara topology existente sem alterar nomes.
- [ ] Smoke controlado publica e consome uma mensagem.
- [ ] Logs não mostram password ou URI com credenciais.

Se TLS ou o VirtualHost falhar, a conexão deve falhar; nunca desabilite TLS como
fallback. Para rollback de configuração, retorne somente no ambiente local a
`RabbitMq__UseTls=false` e `RabbitMq__VirtualHost=/`.
