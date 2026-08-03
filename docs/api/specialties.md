# Especialidades administrativas

`Specialties.View` permite consultar dependências e `Specialties.Manage` permite editar o ciclo de vida.

- `GET /api/specialties/{id}/dependencies` informa quantos profissionais e agendamentos futuros dependem da especialidade.
- `POST /api/specialties/{id}/status/Active` ou `Inactive` altera o estado lógico.
- Desativar ou excluir é bloqueado quando houver profissionais vinculados ou consultas futuras não canceladas.

As operações de criação, edição, status e exclusão geram registros de auditoria sanitizados.
