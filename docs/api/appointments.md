# Agenda administrativa

Além da listagem legada por período, a agenda oferece:

- `GET /api/appointments/search` com `page`, `pageSize`, `professionalId`, `specialtyId`, `unitId`, `patientId`, `status`, `source`, `from`, `to` e `sort=startsAt:desc`.
- `GET /api/appointments/{id}` com nomes de paciente, profissional, unidade e especialidade, além de dados de cancelamento.

`POST /api/appointments`, `POST /api/appointments/{id}/confirm`, `POST /api/appointments/{id}/cancel` e `POST /api/appointments/{id}/reschedule` exigem `Idempotency-Key`. Confirmar e cancelar recebem `expectedVersion`; reagendar recebe `startsAt`, `endsAt`, `expectedVersion` e `notes`. Reagendar executa em transação serializável, rejeita conflitos e mantém o registro original como `Rescheduled`, criando um novo agendamento. Repetir a mesma chave devolve a resposta persistida.

As rotas exigem `ClinicStaff` e mantêm o isolamento por tenant.
