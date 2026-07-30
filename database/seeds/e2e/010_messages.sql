BEGIN;
INSERT INTO clinic_assistant.conversation_messages ("Id","TenantId","ConversationId","Direction","Type","Content","ContentSanitized","Provider","ExternalMessageId","Status","ProviderStatus","ProviderErrorCode","ProviderErrorMessage","QueuedAt","AcceptedAt","SentAt","DeliveredAt","ReadAt","FailedAt","ReceivedAt","CreatedAt","UpdatedAt")
SELECT ('00000000-0000-0000-0000-' || lpad((1000+n)::text,12,'0'))::uuid,
       CASE WHEN ((n-1)%20)+1 = 20 THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE '00000000-0000-0000-0000-000000000101'::uuid END,
       ('00000000-0000-0000-0000-' || lpad((400 + ((n-1)%20+1))::text,12,'0'))::uuid,
       CASE WHEN n % 2 = 0 THEN 'Outbound' ELSE 'Inbound' END,
       CASE WHEN n % 25 = 0 THEN 'Template' WHEN n % 17 = 0 THEN 'System' ELSE 'Text' END,
       'Mensagem fictícia E2E ' || n, 'Mensagem fictícia E2E ' || n, 'Fake',
       'SM_TEST_' || lpad(n::text,30,'0'),
       (ARRAY['Pending','Queued','Accepted','Sent','Delivered','Read','Failed','Received'])[ ((n-1)%8)+1 ],
       CASE WHEN ((n-1)%8)+1 = 7 THEN 'failed' ELSE lower((ARRAY['Pending','Queued','Accepted','Sent','Delivered','Read','Failed','Received'])[ ((n-1)%8)+1 ]) END,
       CASE WHEN ((n-1)%8)+1 = 7 THEN 'TEST_DELIVERY_FAILURE' ELSE NULL END,
       CASE WHEN ((n-1)%8)+1 = 7 THEN 'Falha simulada, sem envio real.' ELSE NULL END,
       CASE WHEN ((n-1)%8)+1 >= 2 THEN '2026-08-03T12:01:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 >= 3 THEN '2026-08-03T12:02:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 >= 4 THEN '2026-08-03T12:03:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 >= 5 THEN '2026-08-03T12:04:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 = 6 THEN '2026-08-03T12:05:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 = 7 THEN '2026-08-03T12:05:00Z'::timestamptz END,
       CASE WHEN ((n-1)%8)+1 = 8 THEN '2026-08-03T12:00:00Z'::timestamptz END,
       '2026-08-03T12:00:00Z'::timestamptz, '2026-08-03T12:00:00Z'::timestamptz
FROM generate_series(1,200) AS n
ON CONFLICT ("Id") DO UPDATE SET "Status"=EXCLUDED."Status", "ProviderErrorCode"=EXCLUDED."ProviderErrorCode", "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
