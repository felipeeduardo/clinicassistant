BEGIN;
INSERT INTO clinic_assistant.tenants ("Id", "Name", "Slug", "Status", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000000001', 'Clínica Demonstração Minimal', 'clinica-minimal', 'Active', '2026-08-03T12:00:00Z', '2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name", "Slug" = EXCLUDED."Slug", "Status" = EXCLUDED."Status", "UpdatedAt" = EXCLUDED."UpdatedAt";
COMMIT;
