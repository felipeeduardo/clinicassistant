# SendGrid Email transacional

O envio transacional usa a Web API v3 do SendGrid através de `IEmailSender`; o
Application não conhece o provedor. A API é chamada com `Authorization: Bearer`
e a chave nunca é persistida, retornada ao frontend ou registrada.

## Configuração

```ini
Email__Enabled=true
Email__Provider=SendGrid
Email__FromAddress=no-reply@iarecepcao.com.br
Email__FromName=IA Recepção
SendGrid__ApiKey=<secret>
SendGrid__RequestTimeoutSeconds=30
```

No SendGrid, autentique o domínio em **Settings → Sender Authentication →
Domain Authentication**, publique os registros DNS indicados e crie uma API
Key com somente a permissão **Mail Send**. O remetente deve pertencer ao domínio
autenticado. Configure as variáveis somente no secret store do Railway (API).

Com `Email__Enabled=false`, nenhum request externo é feito e o login/recuperação
continua funcional em ambiente local.
