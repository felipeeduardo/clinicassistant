BEGIN;
INSERT INTO clinic_assistant.patients ("Id","TenantId","Name","Phone","Email","BirthDate","ConsentStatus","Source","FirstContactAt","LastContactAt","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000301','00000000-0000-0000-0000-000000000101','Paciente E2E Principal','+550000000301','paciente.principal@fake.local','1990-01-15','Granted','Manual','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000302','00000000-0000-0000-0000-000000000101','Paciente E2E Secundário','+550000000302','paciente.secundario@fake.local','1987-05-22','Granted','WhatsApp','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000303','00000000-0000-0000-0000-000000000102','Paciente Outro Tenant','+550000000303','paciente.outro@fake.local','1993-09-11','Granted','Manual','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Phone"=EXCLUDED."Phone", "Email"=EXCLUDED."Email", "UpdatedAt"=EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.patients ("Id","TenantId","Name","Phone","Email","BirthDate","ConsentStatus","Source","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((303 + n)::text,12,'0'))::uuid, '00000000-0000-0000-0000-000000000101'::uuid, 'Paciente E2E Fictício ' || lpad(n::text,2,'0'), '+550000000' || lpad((303+n)::text,3,'0'), 'paciente.' || lpad(n::text,2,'0') || '@example.test', ('1980-01-01'::date + n), 'Granted', 'Manual', '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,27) AS n
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
