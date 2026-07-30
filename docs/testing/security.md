# Segurança dos dados de teste

Todos os dados são inventados. O runner bloqueia `ASPNETCORE_ENVIRONMENT=Production`, exige nome de banco contendo `test`, `e2e` ou `dev` — ou uma lista explícita em `TEST_DATA_ALLOWED_DATABASES` — e exige `ALLOW_TEST_DATA_RESET=true` para qualquer reset. Fora de CI exige confirmação `RESET`, ou `TEST_DATA_CONFIRM=YES`.

O schema atual não possui tabelas de permissões granulares nem de auditoria. Os roles são os da aplicação: `ClinicAdmin`, `Receptionist` e `Professional`; o fixture `viewer` usa o menor papel existente (`Receptionist`). A criação de roles/permissões granulares, auditoria persistida, flag `IsTestData`, locale do tenant e slots persistidos requer migrations e fica fora desta etapa para não criar schema paralelo.
