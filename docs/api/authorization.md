# Autenticação e autorização da API

O contrato HTTP completo é o [OpenAPI em execução](openapi.md). Esta página registra as regras de acesso que todos os consumidores devem respeitar.

- `POST /api/auth/register`, `/login`, `/refresh` e `/logout` compõem a sessão.
- `GET /api/auth/me` exige sessão autenticada.
- O access token é enviado em `Authorization: Bearer <token>`; o refresh token permanece exclusivamente em cookie `HttpOnly`.
- Os dados de clínica são sempre filtrados pelo tenant nas claims JWT. O cliente nunca envia um tenant como parâmetro de confiança.
- `ClinicAdmin` possui permissões de alteração de catálogo e operações humanas. Demais perfis recebem somente as permissões explicitamente atribuídas.
- `/api/platform/*` exige `PlatformAdmin`.

Consulte também [sessão e refresh token](../security/authentication-session.md) e [policies de catálogo](../security/authorization.md).
