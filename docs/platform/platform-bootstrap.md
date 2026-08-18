# Bootstrap seguro da plataforma

## Ordem de inicialização

```text
PostgreSQL disponível → EF Migrations → PlatformBootstrapService → API pronta
```

O serviço é registrado pela infraestrutura e chamado uma vez no startup da API.
Health checks não executam bootstrap. A migration contém apenas schema e dados
estruturais; as identidades são criadas em runtime com secrets externos.

## Resultado e segurança

Para cada e-mail configurado, o serviço cria um `User` com `PlatformAdmin` e hash
gerado pelo mecanismo oficial. Reinícios não duplicam usuários. Um usuário
existente com role incompatível interrompe o processo e exige correção manual.
Conflitos de índice são tratados como criação concorrente somente quando a
identidade concorrente já é `PlatformAdmin`.

Eventos de auditoria: `PlatformAdminCreated`, `PlatformAdminAlreadyExists`, além
dos logs de início/conclusão/falha. Nenhum evento contém password.

## Railway

**MANUAL ACTION REQUIRED:** cadastrar as cinco variáveis de bootstrap no serviço
API durante o primeiro deploy, validar o login, desabilitar a flag e remover as
senhas. Não aplicar esses valores automaticamente.
