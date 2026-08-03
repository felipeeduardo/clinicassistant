# Agenda administrativa de profissionais

Leitura usa `Professionals.View`; alterações usam `Professionals.Manage`. Todas as operações recusam profissional inativo ou de outro tenant.

- Disponibilidade: `GET /api/professionals/{id}/availability/rules` e `PUT` na mesma rota para substituir as regras.
- Bloqueios: `GET/POST /api/professionals/{id}/blocks` e `DELETE /api/professionals/{id}/blocks/{blockId}`.
- Férias: `GET/POST /api/professionals/{id}/vacations` e `DELETE /api/professionals/{id}/vacations/{vacationId}`.
- Agenda: `GET /api/professionals/{id}/schedule?startsAt=&endsAt=` retorna consultas, bloqueios e férias no intervalo.

Bloqueios e férias sobrepostos são rejeitados. Férias que coincidam com consultas não canceladas também são rejeitadas; consultas não podem ser criadas durante férias.
