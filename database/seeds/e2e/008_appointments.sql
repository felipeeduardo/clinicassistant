BEGIN;
INSERT INTO clinic_assistant.appointments ("Id","TenantId","ClinicUnitId","ProfessionalId","SpecialtyId","PatientId","StartsAt","EndsAt","Status","Source","Notes","CancelledAt","CancellationReason","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((500+n)::text,12,'0'))::uuid,
       CASE WHEN n = 50 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       CASE WHEN n = 50 THEN '00000000-0000-0000-0000-000000000123'::uuid ELSE '00000000-0000-0000-0000-000000000121'::uuid END,
       CASE WHEN n = 50 THEN '00000000-0000-0000-0000-000000000150'::uuid ELSE ('00000000-0000-0000-0000-' || lpad((140 + ((n-1)%9+1))::text,12,'0'))::uuid END,
       CASE WHEN n = 50 THEN '00000000-0000-0000-0000-000000000138'::uuid ELSE ('00000000-0000-0000-0000-' || lpad((130 + ((n-1)%7+1))::text,12,'0'))::uuid END,
       CASE WHEN n = 50 THEN '00000000-0000-0000-0000-000000000303'::uuid
            WHEN ((n-1)%29)+1 <= 2 THEN ('00000000-0000-0000-0000-' || lpad((300 + ((n-1)%29+1))::text,12,'0'))::uuid
            ELSE ('00000000-0000-0000-0000-' || lpad((301 + ((n-1)%29+1))::text,12,'0'))::uuid END,
       (:'base_date'::date + ((n-1) % 30) + time '09:00')::timestamp AT TIME ZONE 'America/Recife', (:'base_date'::date + ((n-1) % 30) + time '09:30')::timestamp AT TIME ZONE 'America/Recife',
       (ARRAY['Pending','Confirmed','Rescheduled','Cancelled','Completed','NoShow'])[ ((n-1)%6)+1 ], 'Dashboard', 'Consulta fictícia E2E ' || n,
       CASE WHEN ((n-1)%6)+1 = 4 THEN (:'base_date'::date + ((n-1)%30))::timestamp AT TIME ZONE 'America/Recife' ELSE NULL END,
       CASE WHEN ((n-1)%6)+1 = 4 THEN 'Cancelamento fictício E2E' ELSE NULL END, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,50) AS n
ON CONFLICT ("Id") DO UPDATE SET "TenantId"=EXCLUDED."TenantId", "PatientId"=EXCLUDED."PatientId", "ProfessionalId"=EXCLUDED."ProfessionalId", "SpecialtyId"=EXCLUDED."SpecialtyId", "Status"=EXCLUDED."Status", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
