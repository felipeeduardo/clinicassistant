BEGIN;
INSERT INTO clinic_assistant.tenants ("Id", "Name", "Slug", "Status", "CreatedAt", "UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000101', 'Clínica Saúde Mais E2E', 'clinica-saude-mais-e2e', 'Active', '2026-08-03T12:00:00Z', '2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000102', 'Clínica Isolada E2E', 'clinica-isolada-e2e', 'Active', '2026-08-03T12:00:00Z', '2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name", "Slug" = EXCLUDED."Slug", "Status" = EXCLUDED."Status", "UpdatedAt" = EXCLUDED."UpdatedAt";
COMMIT;
