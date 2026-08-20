# Checklist de publicação do Dashboard PlatformAdmin

## Railway (API)

1. Publique o serviço apontando para o Dockerfile da API em `backend/src/ClinicAssistant.Api` conforme a configuração existente do projeto.
2. Confirme `ConnectionStrings__Default`, `Redis__ConnectionString`, `RabbitMq__Uri` e `Jwt__Secret` no ambiente de produção.
3. Aguarde `/health/ready` retornar HTTP 200.
4. Abra `/swagger/v1/swagger.json` e confirme a operação `GET /api/platform/dashboard`.
5. Verifique no log os checks `worker-outbox` e `signalr` sem expor valores de configuração.

## Vercel (frontend)

1. Configure `NEXT_PUBLIC_API_URL=https://api.iarecepcao.com.br`.
2. Publique o frontend e entre com um usuário `PlatformAdmin`.
3. Abra `/dashboard`, alterne 7/30/90 dias e use **Atualizar**.
4. Confirme que usuários de clínica continuam vendo o dashboard operacional, não os indicadores globais.
5. Faça uma verificação visual em desktop e viewport estreito; os cards devem reflowar sem rolagem horizontal.

Não são necessários novos secrets para o dashboard: ele reutiliza a autenticação e as conexões já configuradas.
