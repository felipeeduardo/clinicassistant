# Pacientes administrativos

As rotas exigem a política `Patients.View` para leitura e `Patients.Manage` para criação e edição, sempre no tenant do usuário autenticado.

- `GET /api/patients/search?page=1&pageSize=20&search=&consentStatus=` retorna uma página de pacientes. A busca cobre nome, telefone e e-mail; `pageSize` é limitado a 100.
- `GET /api/patients/{id}` retorna o cadastro, origem, datas de contato, próximos agendamentos, conversas e até 20 eventos de auditoria do paciente.
- `POST /api/patients` e `PUT /api/patients/{id}` mantêm o contrato `PatientRequest` existente. Essas ações passam a registrar um evento de auditoria.

O endpoint histórico `GET /api/patients` continua disponível para consumidores legados e retorna a lista não paginada. Clientes novos devem usar `/api/patients/search`.
