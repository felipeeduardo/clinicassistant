-- Deliberately not executable by the runner. Shared environments must use the
-- tenant-scoped reset_test_data.sql so non-test tenants remain untouched.
DO $$ BEGIN RAISE EXCEPTION 'TRUNCATE is intentionally disabled for test-data scripts.'; END $$;
