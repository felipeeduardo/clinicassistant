# Auditoria administrativa

`GET /api/audit` exige `ClinicAdmin` e só retorna eventos do tenant autenticado.

Aceita `page`, `pageSize`, `userId`, `action`, `resourceType`, `resourceId`, `result`, `from` e `to`. A resposta não expõe detalhes internos, payloads nem dados sensíveis: somente data, ator, ação, recurso e resultado.
