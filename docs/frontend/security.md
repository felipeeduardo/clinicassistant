# Segurança do frontend

- Tokens de acesso e refresh permanecem apenas em memória; uma recarga requer novo login até existir sessão segura via cookie HttpOnly/BFF.
- O cliente encerra a sessão e limpa o cache ao receber `401`.
- A interface apresenta somente o título sanitizado de um erro HTTP; `detail`, payloads e stack traces não são exibidos.
- Telefone e e-mail de pacientes são mascarados na listagem.
- O build publica CSP, `X-Frame-Options: DENY`, `nosniff`, política de referer e permissões restritas. A origem da API e WebSocket deve ser definida por `NEXT_PUBLIC_API_URL` no momento do build.
- Cancelamentos exigem confirmação explícita no navegador.
