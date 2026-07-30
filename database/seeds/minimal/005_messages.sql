BEGIN;
INSERT INTO clinic_assistant.conversation_messages ("Id","TenantId","ConversationId","Direction","Type","Content","ContentSanitized","Provider","ExternalMessageId","Status","ReceivedAt","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((80+n)::text,12,'0'))::uuid, '00000000-0000-0000-0000-000000000001'::uuid,
CASE WHEN n%2=0 THEN '00000000-0000-0000-0000-000000000072'::uuid ELSE '00000000-0000-0000-0000-000000000071'::uuid END,
CASE WHEN n%2=0 THEN 'Outbound' ELSE 'Inbound' END, 'Text', 'Mensagem fictícia minimal ' || n, 'Mensagem fictícia minimal ' || n, 'Fake', 'SM_TEST_MINIMAL_' || lpad(n::text,4,'0'), CASE WHEN n%2=0 THEN 'Sent' ELSE 'Received' END,
CASE WHEN n%2=1 THEN '2026-08-03T12:00:00Z'::timestamptz ELSE NULL END, '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz FROM generate_series(1,10) AS n
ON CONFLICT ("Id") DO UPDATE SET "Status"=EXCLUDED."Status", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
