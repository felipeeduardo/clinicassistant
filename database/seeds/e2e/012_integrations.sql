BEGIN;
INSERT INTO clinic_assistant.whatsapp_integrations ("Id","TenantId","Provider","IntegrationKey","AccountSidReference","MessagingServiceSid","WhatsAppFrom","DisplayPhoneNumber","Status","ConnectedAt","CreatedAt","UpdatedAt") VALUES
('00000000-0000-0000-0000-000000000601','00000000-0000-0000-0000-000000000101','Fake','fake-e2e-main',NULL,NULL,'whatsapp:+5500000000000','Fake E2E Sender','Connected','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z','2026-08-03T12:00:00Z'),
('00000000-0000-0000-0000-000000000602','00000000-0000-0000-0000-000000000102','Twilio','twilio-e2e-disabled',NULL,NULL,'whatsapp:+5500000000000','Twilio Placeholder','Disabled',NULL,'2026-08-03T12:00:00Z','2026-08-03T12:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET "Provider"=EXCLUDED."Provider", "IntegrationKey"=EXCLUDED."IntegrationKey", "Status"=EXCLUDED."Status", "AccountSidReference"=NULL, "MessagingServiceSid"=NULL, "UpdatedAt"=EXCLUDED."UpdatedAt";
COMMIT;
