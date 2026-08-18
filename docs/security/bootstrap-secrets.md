# Segredos do bootstrap da plataforma

O bootstrap inicial dos `PlatformAdmin` é executado pela API depois das migrations.
Ele usa o `PasswordHasher` oficial da solução (PBKDF2-SHA512 com salt aleatório) e
nunca recebe senha de migration, SQL, log, documentação ou teste versionado.

## Variáveis

O binder do .NET aceita as variáveis abaixo (em Railway, marque as senhas como
**Secret**):

```text
PlatformBootstrap__Enabled=false
PlatformBootstrap__Admins__0__Email=<PLATFORM_ADMIN_EMAIL_1>
PlatformBootstrap__Admins__0__Password=<SET_IN_SECRET_STORE>
PlatformBootstrap__Admins__1__Email=<PLATFORM_ADMIN_EMAIL_2>
PlatformBootstrap__Admins__1__Password=<SET_IN_SECRET_STORE>
```

No Docker Compose local, alterar o `.env` não altera um container já criado.
Depois de configurar os secrets, recrie a API para que os valores sejam
injetados no processo:

```bash
docker compose up -d --build --force-recreate api
docker compose logs -f api
```

O `build` sozinho apenas recompila a imagem; ele não atualiza as variáveis de
ambiente de um container existente. Antes de investigar o bootstrap, confirme
que a API recebeu os três valores sem imprimir a senha:

```bash
docker compose exec api sh -c \
  'test "$PlatformBootstrap__Enabled" = true && test -n "$PlatformBootstrap__Admins__0__Email" && test -n "$PlatformBootstrap__Admins__0__Password" && echo "Platform bootstrap configuration loaded"'
```

O Compose local injeta somente o administrador de índice `0`. Isso evita que
variáveis opcionais vazias (`Admins__1__*`) sejam interpretadas pelo binder como
um segundo administrador incompleto. Para dois administradores, configure os
índices `0` e `1` diretamente no ambiente de execução (por exemplo, Railway) ou
em um override do Compose que contenha ambos os valores.

Quando `Enabled=false`, nenhuma consulta de criação é feita. Quando habilitado,
é obrigatório configurar de um a dois administradores, com e-mails distintos e
senha de pelo menos 12 caracteres contendo maiúscula, minúscula, número e símbolo.

O serviço normaliza o e-mail, verifica cada identidade individualmente e é
idempotente em reinicializações. Se um e-mail já pertencer a outra role, o
bootstrap falha sem promover o usuário. As auditorias registram apenas evento,
role/identidade e resultado; nunca registram senha.

## Rotação

Após validar o login dos administradores:

1. troque as credenciais temporárias pelo fluxo operacional aprovado;
2. defina `PlatformBootstrap__Enabled=false`;
3. remova ou rotacione as senhas no secret store;
4. reinicie a API e confirme nos logs que não há execução de bootstrap.

Valores reais devem ser inseridos somente no secret manager do ambiente, nunca no
Git ou em `.env.example`.
