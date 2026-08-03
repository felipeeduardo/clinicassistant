# Autorização de cadastros administrativos

As policies de leitura e alteração são separadas por recurso. `ClinicAdmin` é o único perfil que recebe policies `*.Manage`; `Receptionist`, `Professional` e `Viewer` recebem, quando aplicável, apenas as policies `*.View`.

| Recurso | Leitura | Alteração |
| --- | --- | --- |
| Clínica | `Clinics.View` | `Clinics.Manage` |
| Unidades | `Units.View` | `Units.Manage` |
| Pacientes | `Patients.View` | `Patients.Manage` |
| Profissionais | `Professionals.View` | `Professionals.Manage` |
| Especialidades | `Specialties.View` | `Specialties.Manage` |

O frontend oculta ações administrativas, mas a API aplica as mesmas regras no servidor. O tenant continua sendo obtido das claims JWT e filtrado pelo `DbContext`.
