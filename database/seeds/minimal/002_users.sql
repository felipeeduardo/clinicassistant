BEGIN;
INSERT INTO clinic_assistant.users ("Id", "TenantId", "Name", "Email", "PasswordHash", "Role", "Status", "CreatedAt", "UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000011', '00000000-0000-0000-0000-000000000001', 'Admin Minimal', 'admin.minimal@fake.local', :'password_hash', 'ClinicAdmin', 'Active', '2026-08-03T12:00:00Z', '2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000012', '00000000-0000-0000-0000-000000000001', 'Recepção Minimal', 'reception.minimal@fake.local', :'password_hash', 'Receptionist', 'Active', '2026-08-03T12:00:00Z', '2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name", "Email" = EXCLUDED."Email", "PasswordHash" = EXCLUDED."PasswordHash", "Role" = EXCLUDED."Role", "Status" = EXCLUDED."Status", "UpdatedAt" = EXCLUDED."UpdatedAt";
COMMIT;
