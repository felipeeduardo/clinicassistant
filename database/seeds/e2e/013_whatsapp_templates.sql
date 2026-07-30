BEGIN;
INSERT INTO clinic_assistant.whatsapp_templates ("Id","TenantId","IntegrationId","Provider","ContentSid","Name","LanguageCode","Category","Status","ParametersSchema","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((650+n)::text,12,'0'))::uuid, '00000000-0000-0000-0000-000000000101'::uuid, '00000000-0000-0000-0000-000000000601'::uuid, 'Fake',
       'HX_TEST_' || lpad(n::text,30,'0'), name, 'pt-BR', 'utility', CASE WHEN n=7 THEN 'Disabled' WHEN n=6 THEN 'Rejected' WHEN n=5 THEN 'Approved' ELSE 'Draft' END, '{}'::text,
       '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM (VALUES (1,'welcome'),(2,'main_menu'),(3,'appointment_confirmation'),(4,'appointment_reschedule'),(5,'appointment_cancellation'),(6,'human_handoff'),(7,'automation_resumed')) AS templates(n,name)
ON CONFLICT ("Id") DO UPDATE SET "Status"=EXCLUDED."Status", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
