BEGIN;
INSERT INTO clinic_assistant.human_queue_items ("Id","TenantId","ConversationId","Status","Priority","AssignedUserId","Reason","Version","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((900+n)::text,12,'0'))::uuid,
       CASE WHEN n=10 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       ('00000000-0000-0000-0000-' || lpad((400+n)::text,12,'0'))::uuid,
       CASE WHEN n=2 THEN 'Assigned' WHEN n=3 THEN 'Completed' ELSE 'Waiting' END,
       CASE WHEN n=1 THEN 'Urgent' WHEN n=2 THEN 'High' ELSE 'Normal' END,
       CASE WHEN n=2 THEN '00000000-0000-0000-0000-000000000204'::uuid ELSE NULL END,
       'Atendimento humano fictício E2E ' || n, CASE WHEN n=2 THEN 2 ELSE 1 END, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,10) AS n
ON CONFLICT ("Id") DO UPDATE SET "Status"=EXCLUDED."Status", "AssignedUserId"=EXCLUDED."AssignedUserId", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
