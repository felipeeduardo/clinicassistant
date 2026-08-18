# Leads comerciais e demonstração

O formulário público `/demonstracao` envia `POST /api/leads/demo-requests`. Os campos obrigatórios são `fullName`, `companyOrClinicName`, `email` e `phone`; `description` é opcional e `website` é um honeypot anti-spam. O endpoint aceita a requisição sem criar tenant ou usuário e nunca deve receber dados de pacientes.

Leads são persistidos em `clinic_assistant.demo_leads` com status `New`, `Contacted`, `Qualified`, `DemoScheduled`, `Won`, `Lost` ou `Archived`. O endpoint público tem limite de 5 requisições por minuto por IP e payload máximo de 16 KiB.

Somente `PlatformAdmin` pode usar:

- `GET /api/platform/leads` — paginação, busca, status, responsável e intervalo de datas;
- `GET /api/platform/leads/summary` — contadores operacionais;
- `GET /api/platform/leads/{id}` — detalhe, histórico e observações;
- `POST /api/platform/leads/{id}/status` — altera o status;
- `POST /api/platform/leads/{id}/assignment` — atribui a outro PlatformAdmin;
- `POST /api/platform/leads/{id}/notes` — registra observação auditada.

Todos os eventos administrativos são registrados em `audit_records`. A criação do lead não dispara onboarding automático.

A migração `202608180001_DemoLeads` é aplicada pelo startup da API junto das demais migrations. Em produção, valide o histórico `__EFMigrationsHistory` antes de abrir o formulário público.
