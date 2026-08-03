BEGIN;
INSERT INTO clinic_assistant.users ("Id","TenantId","Name","Email","PasswordHash","Role","Status","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000201','00000000-0000-0000-0000-000000000101','Admin E2E','admin.e2e@fake.local',:'password_hash','ClinicAdmin','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000202','00000000-0000-0000-0000-000000000101','Manager E2E','manager.e2e@fake.local',:'password_hash','ClinicAdmin','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000203','00000000-0000-0000-0000-000000000101','Recepção E2E','reception.e2e@fake.local',:'password_hash','Receptionist','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000204','00000000-0000-0000-0000-000000000101','Operador E2E','operator.e2e@fake.local',:'password_hash','Professional','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000205','00000000-0000-0000-0000-000000000101','Operador Dois E2E','operator2.e2e@fake.local',:'password_hash','Professional','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000206','00000000-0000-0000-0000-000000000102','Visualizador E2E','viewer.e2e@fake.local',:'password_hash','Viewer','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000207','00000000-0000-0000-0000-000000000101','Platform Admin E2E','platform-admin.e2e@fake.local',:'password_hash','PlatformAdmin','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Email"=EXCLUDED."Email", "PasswordHash"=EXCLUDED."PasswordHash", "Role"=EXCLUDED."Role", "Status"=EXCLUDED."Status", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
