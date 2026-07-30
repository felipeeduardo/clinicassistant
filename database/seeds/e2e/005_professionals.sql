BEGIN;
INSERT INTO clinic_assistant.professionals ("Id","TenantId","ClinicUnitId","Name","Email","Phone","RegistrationNumber","Status","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((140 + n)::text,12,'0'))::uuid, CASE WHEN n = 10 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
CASE WHEN n = 10 THEN '00000000-0000-0000-0000-000000000123'::uuid WHEN n % 2 = 0 THEN '00000000-0000-0000-0000-000000000122'::uuid ELSE '00000000-0000-0000-0000-000000000121'::uuid END,
name, lower(replace(name,' ','.')) || '@fake.local', '+5500000002' || lpad(n::text,2,'0'), 'TEST-REG-' || lpad(n::text,3,'0'), 'Active', '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM (VALUES (1,'Dra. Ana Souza'),(2,'Dr. Bruno Lima'),(3,'Dra. Carla Mendes'),(4,'Dr. Daniel Rocha'),(5,'Dra. Elisa Barros'),(6,'Dr. Fabio Alves'),(7,'Dra. Gabriela Reis'),(8,'Dr. Henrique Costa'),(9,'Dra. Isabela Cruz'),(10,'Dr. João Isolado')) AS professionals(n,name)
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "UpdatedAt"=EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.professional_specialties ("ProfessionalId","SpecialtyId")
SELECT ('00000000-0000-0000-0000-' || lpad((140 + n)::text,12,'0'))::uuid, ('00000000-0000-0000-0000-' || lpad((130 + n)::text,12,'0'))::uuid FROM generate_series(1,7) AS n
UNION ALL SELECT '00000000-0000-0000-0000-000000000150'::uuid, '00000000-0000-0000-0000-000000000138'::uuid
ON CONFLICT DO NOTHING;
COMMIT;
