# Matriz de URLs públicas por ambiente

As variáveis abaixo são lidas no build do frontend e devem ser definidas por ambiente. Não colocar tokens, senhas ou URLs de webhook com secrets nelas.

| Variável | Development | Pilot/Staging | Production |
|---|---|---|---|
| `NEXT_PUBLIC_SITE_URL` | `http://localhost:3000` | URL pública de landing do piloto | `https://iarecepcao.com.br` |
| `NEXT_PUBLIC_APP_URL` | `http://localhost:3000` | URL pública do app piloto | `https://app.iarecepcao.com.br` |
| `NEXT_PUBLIC_API_URL` | `http://localhost:8080` | URL HTTPS da API piloto | `https://api.iarecepcao.com.br` |
| `NEXT_PUBLIC_BRAND_DOMAIN` | `iarecepcao.com.br` | `iarecepcao.com.br` | `iarecepcao.com.br` |
| `NEXT_PUBLIC_SUPPORT_EMAIL` | e-mail local | e-mail de suporte do piloto | e-mail oficial de suporte |

## Regras

- Development permanece em localhost e não depende de DNS externo.
- Pilot/Staging deve ser preenchido com os hostnames fornecidos pelo provedor de hospedagem.
- Production só pode ser ativado após DNS, TLS, CORS e health checks validados.
- Alterar `NEXT_PUBLIC_*` exige rebuild do frontend; reiniciar o container não recompila valores embutidos no bundle.

## Validação local concluída

`docker compose config --quiet` foi executado com sucesso. A configuração resolvida confirmou que as quatro URLs públicas são propagadas tanto como argumentos de build quanto como variáveis de runtime do serviço `frontend`.
