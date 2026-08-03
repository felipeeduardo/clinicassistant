# Unidades administrativas

As rotas usam `Units.View` para leitura e `Units.Manage` para alterações.

- `GET /api/units/{id}/details` traz unidade, fuso da clínica, horários, profissionais vinculados e os 20 eventos recentes de auditoria.
- `POST /api/units/{id}/status/Active` ou `Inactive` altera o estado operacional.
- `PUT /api/units/{id}/business-hours` substitui todos os horários, com uma entrada por dia (`dayOfWeek`, `opensAt`, `closesAt`). Horários repetidos ou inválidos são rejeitados.
- A exclusão é bloqueada enquanto houver profissionais vinculados.
