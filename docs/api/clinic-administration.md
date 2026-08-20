# Administração da clínica

As APIs operacionais usam o contexto do tenant autenticado e são destinadas ao `ClinicAdmin` (ou à policy de leitura específica quando aplicável). O PlatformAdmin não participa dessas operações.

Principais grupos:

- `/api/clinics/current` e `/api/clinics/current/setup`;
- `/api/units`;
- `/api/specialties`;
- `/api/professionals` e disponibilidade;
- `/api/appointments`;
- `/api/whatsapp/integration` e `/api/whatsapp/templates`;
- `/api/audit`.

O endpoint `/api/clinics/current/setup` deriva o progresso dos registros atuais e não persiste uma etapa artificial. O backend aplica isolamento por tenant e retorna `403` para papéis globais quando a operação é clínica.
