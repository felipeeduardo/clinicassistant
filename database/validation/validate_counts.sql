BEGIN;
DO $$
DECLARE expected_tenants integer := CASE current_setting('test_data.profile', true) WHEN 'minimal' THEN 1 ELSE 2 END;
DECLARE actual_tenants integer;
BEGIN
  SELECT count(*) INTO actual_tenants FROM clinic_assistant.tenants WHERE "Id" IN ('00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102');
  IF actual_tenants <> expected_tenants THEN RAISE EXCEPTION 'Expected % seed tenant(s), found %.', expected_tenants, actual_tenants; END IF;
  IF current_setting('test_data.profile', true) = 'e2e' THEN
    IF (SELECT count(*) FROM clinic_assistant.users WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102')) <> 6 THEN RAISE EXCEPTION 'Expected 6 E2E users.'; END IF;
    IF (SELECT count(*) FROM clinic_assistant.patients WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102')) <> 30 THEN RAISE EXCEPTION 'Expected 30 E2E patients.'; END IF;
    IF (SELECT count(*) FROM clinic_assistant.conversations WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102')) <> 20 THEN RAISE EXCEPTION 'Expected 20 E2E conversations.'; END IF;
    IF (SELECT count(*) FROM clinic_assistant.conversation_messages WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102')) <> 200 THEN RAISE EXCEPTION 'Expected 200 E2E messages.'; END IF;
    IF (SELECT count(*) FROM clinic_assistant.human_queue_items WHERE "TenantId" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102')) <> 10 THEN RAISE EXCEPTION 'Expected 10 E2E human queue items.'; END IF;
  END IF;
END $$;
COMMIT;
