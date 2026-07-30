BEGIN;
INSERT INTO clinic_assistant.availability_rules ("Id","TenantId","ProfessionalId","DayOfWeek","StartTime","EndTime","SlotDurationMinutes","Active","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((700+n)::text,12,'0'))::uuid,
       CASE WHEN n = 10 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       ('00000000-0000-0000-0000-' || lpad((140+n)::text,12,'0'))::uuid, n % 7, '08:00'::time, '17:00'::time, 30, true, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,10) AS n
ON CONFLICT ("Id") DO UPDATE SET "Active"=EXCLUDED."Active", "UpdatedAt"=EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.schedule_blocks ("Id","TenantId","ProfessionalId","StartsAt","EndsAt","Reason","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000731','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000141',(:'base_date'::date + time '12:00')::timestamp AT TIME ZONE 'America/Recife', (:'base_date'::date + time '13:00')::timestamp AT TIME ZONE 'America/Recife','Bloqueio fictício E2E','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
