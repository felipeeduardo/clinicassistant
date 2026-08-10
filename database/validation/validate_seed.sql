BEGIN;
DO $$
BEGIN
  IF current_setting('test_data.profile', true) = 'e2e' AND NOT EXISTS (SELECT 1 FROM clinic_assistant.users WHERE "Email" = 'platform-admin.e2e@fake.local' AND "Role" = 'PlatformAdmin' AND "Status" = 'Active') THEN
    RAISE EXCEPTION 'E2E PlatformAdmin fixture is missing or inactive.';
  END IF;
END $$;
DO $$ BEGIN
  IF to_regclass('clinic_assistant.tenants') IS NULL OR to_regclass('clinic_assistant.human_queue_items') IS NULL THEN
    RAISE EXCEPTION 'Expected Clinic Assistant schema is missing.';
  END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.users WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102') AND "Email" !~ '@(fake\.local|example\.test|invalid\.local)$') THEN
    RAISE EXCEPTION 'External email found in test data.';
  END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.patients WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102') AND left("Phone", 7) <> '+550000') THEN
    RAISE EXCEPTION 'A patient phone is not a reserved fake number.';
  END IF;
  IF current_setting('test_data.profile', true) = 'e2e' AND EXISTS (SELECT 1 FROM clinic_assistant.whatsapp_integrations WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102') AND "Provider" = 'Twilio' AND "Status" <> 'Disabled') THEN
    RAISE EXCEPTION 'A Twilio integration is enabled in test data.';
  END IF;
  IF current_setting('test_data.profile', true) = 'minimal' AND EXISTS (SELECT 1 FROM clinic_assistant.whatsapp_integrations WHERE "TenantId" = '00000000-0000-0000-0000-000000000001' AND ("Provider"::text <> current_setting('test_data.whatsapp_provider', true) OR "Status" <> 'Connected')) THEN
    RAISE EXCEPTION 'The minimal WhatsApp integration does not match WHATSAPP_PROVIDER.';
  END IF;
END $$;
COMMIT;
