BEGIN;
INSERT INTO clinic_assistant.conversations ("Id","TenantId","PatientId","Channel","IntegrationId","ExternalContactId","Status","AssignedUserId","StartedAt","LastMessageAt","ClosedAt","AutomationMode","Priority","Version","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((400+n)::text,12,'0'))::uuid,
       CASE WHEN n = 20 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       CASE WHEN n = 20 THEN '00000000-0000-0000-0000-000000000303'::uuid
            WHEN ((n-1)%29)+1 <= 2 THEN ('00000000-0000-0000-0000-' || lpad((300 + ((n-1)%29+1))::text,12,'0'))::uuid
            ELSE ('00000000-0000-0000-0000-' || lpad((301 + ((n-1)%29+1))::text,12,'0'))::uuid END,
       'WhatsApp',CASE WHEN n = 20 THEN '00000000-0000-0000-0000-000000000602'::uuid ELSE '00000000-0000-0000-0000-000000000601'::uuid END, '+550000000' || lpad((300+n)::text,3,'0'),
       CASE n WHEN 1 THEN 'WaitingHuman' WHEN 2 THEN 'Human' WHEN 4 THEN 'Closed' ELSE 'Bot' END,
       CASE WHEN n=2 THEN '00000000-0000-0000-0000-000000000204'::uuid ELSE NULL END,
       '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:15:00Z'::timestamptz, CASE WHEN n=4 THEN '2026-08-03T12:20:00Z'::timestamptz ELSE NULL END,
       CASE WHEN n IN (1,2) THEN 'Human' WHEN n IN (3,4) THEN 'Paused' ELSE 'Automated' END,
       CASE WHEN n=1 THEN 'Urgent' WHEN n=2 THEN 'High' ELSE 'Normal' END, 1, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,20) AS n
ON CONFLICT ("Id") DO UPDATE SET "TenantId"=EXCLUDED."TenantId", "PatientId"=EXCLUDED."PatientId", "IntegrationId"=EXCLUDED."IntegrationId", "Status"=EXCLUDED."Status", "AutomationMode"=EXCLUDED."AutomationMode", "AssignedUserId"=EXCLUDED."AssignedUserId", "UpdatedAt"=EXCLUDED."UpdatedAt";
INSERT INTO clinic_assistant.conversation_states ("Id","TenantId","ConversationId","Status","FlowState","Intent","ContextJson","InvalidAttempts","ExpiresAt","LastInteractionAt","Version","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((800+n)::text,12,'0'))::uuid,
       CASE WHEN n=20 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       ('00000000-0000-0000-0000-' || lpad((400+n)::text,12,'0'))::uuid,
       CASE WHEN n=1 THEN 'HandedOff' WHEN n=4 THEN 'Completed' ELSE 'Active' END,
       CASE WHEN n=1 THEN 'HandedOff' WHEN n=4 THEN 'Closed' ELSE 'Menu' END,
       CASE WHEN n=1 THEN 'TalkToHuman' WHEN n=5 THEN 'RescheduleAppointment' ELSE 'Greeting' END,
       '{}'::jsonb, 0, '2026-08-04T12:00:00Z'::timestamptz, '2026-08-03T12:15:00Z'::timestamptz, 1, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,20) AS n
ON CONFLICT ("Id") DO UPDATE SET "Status"=EXCLUDED."Status", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
