BEGIN;
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
  IF EXISTS (SELECT 1 FROM clinic_assistant.whatsapp_integrations WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102') AND "Provider" = 'Twilio' AND "Status" <> 'Disabled') THEN
    RAISE EXCEPTION 'A Twilio integration is enabled in test data.';
  END IF;
END $$;
COMMIT;
