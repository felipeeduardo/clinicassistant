# Runbook de domínio — IA Recepção

## Topologia pública pretendida

| Hostname | Destino | Estado |
|---|---|---|
| `iarecepcao.com.br` | Landing pública | Preparação |
| `www.iarecepcao.com.br` | Redirect para domínio canônico | Preparação |
| `app.iarecepcao.com.br` | Frontend autenticado | Preparação |
| `api.iarecepcao.com.br` | API e webhooks | Preparação |

O nome técnico `ClinicAssistant` permanece em namespaces, solution,
assemblies, banco, migrations, filas e identificadores persistentes.

## Ações manuais obrigatórias

1. Confirmar no provedor DNS qual plataforma hospedará landing, app e API.
2. Criar os registros CNAME/A conforme os valores fornecidos pela plataforma.
3. Configurar certificado TLS gerenciado para cada hostname.
4. Configurar redirect 301 de `www` para `iarecepcao.com.br`.
5. Definir `NEXT_PUBLIC_SITE_URL`, `NEXT_PUBLIC_APP_URL` e a origem de API por ambiente.
6. Atualizar a allowlist de CORS apenas com as origens de Pilot/Production.
7. Configurar os webhooks Twilio usando `api.iarecepcao.com.br` somente após o HTTPS estar válido.

Não adicionar registros A/CNAME sem os destinos oficiais do provedor. Cada mudança deve ser registrada com TTL, data, responsável e plano de rollback.

## Validação e rollback

- Validar resolução DNS com `dig` e certificado com `curl -I https://...`.
- Confirmar `/health/ready` na API e carregamento de `/login` no app.
- Confirmar callback Twilio e assinatura antes do piloto.
- Em caso de falha, restaurar o hostname anterior e as variáveis do ambiente; localhost, ngrok e FakeWhatsAppGateway continuam sendo os caminhos de desenvolvimento.
