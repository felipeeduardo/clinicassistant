# Perfis de dados de teste

`minimal` cria uma clínica, 2 usuários, 1 unidade, 2 especialidades, 2 profissionais, 5 pacientes, 10 regras de disponibilidade, 3 consultas, 2 conversas, 10 mensagens e 1 integração Fake.

`e2e` cria 2 tenants, 6 usuários, 3 unidades, 8 especialidades, 10 profissionais, 30 pacientes, 10 regras de disponibilidade ao longo do calendário configurável, 50 consultas, 20 conversas, 200 mensagens, 10 itens de fila, uma integração Fake conectada e uma Twilio desabilitada. `E2E_BASE_DATE` define a data-base e tem padrão `2026-08-03`.

Os nomes Manager, Operator e Viewer são aliases de fixture: o modelo atual só possui `ClinicAdmin`, `Receptionist` e `Professional`; veja a documentação de segurança.
