BEGIN;
INSERT INTO clinic_assistant.specialties ("Id","TenantId","Name","Description","Status","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-00000000013' || n)::uuid, CASE WHEN n = 8 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END, name, 'Especialidade fictícia E2E.', 'Active', '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM (VALUES (1,'Clínico Geral'),(2,'Cardiologia'),(3,'Dermatologia'),(4,'Pediatria'),(5,'Ortopedia'),(6,'Neurologia'),(7,'Ginecologia'),(8,'Oftalmologia')) AS specialties(n,name)
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
