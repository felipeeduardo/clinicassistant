BEGIN;
INSERT INTO clinic_assistant.clinics ("Id","TenantId","LegalName","TradeName","Document","Email","Phone","TimeZone","Status","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000021','00000000-0000-0000-0000-000000000001','Clínica Demonstração Minimal LTDA','Clínica Minimal','TEST-MINIMAL','contato.minimal@fake.local','+550000000001','America/Recife','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt" = EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.clinic_units ("Id","TenantId","ClinicId","Name","Address","Phone","Status","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000022','00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000021','Unidade Minimal','Rua Fictícia, 100, Recife','+550000000002','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt" = EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.specialties ("Id","TenantId","Name","Description","Status","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000023','00000000-0000-0000-0000-000000000001','Clínico Geral','Especialidade fictícia para smoke tests.','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000024','00000000-0000-0000-0000-000000000001','Pediatria','Especialidade fictícia para smoke tests.','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt" = EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.professionals ("Id","TenantId","ClinicUnitId","Name","Email","Phone","RegistrationNumber","Status","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000025','00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000022','Dra. Ana Minimal','ana.minimal@fake.local','+550000000003','TEST-CRM-001','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000026','00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000022','Dr. Bruno Minimal','bruno.minimal@fake.local','+550000000004','TEST-CRM-002','Active','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt" = EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.professional_specialties ("ProfessionalId","SpecialtyId") VALUES
('00000000-0000-0000-0000-000000000025','00000000-0000-0000-0000-000000000023'),
('00000000-0000-0000-0000-000000000026','00000000-0000-0000-0000-000000000024') ON CONFLICT DO NOTHING;
COMMIT;
