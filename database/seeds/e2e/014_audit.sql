BEGIN;
-- The current schema has no audit table. Keeping this versioned no-op makes the
-- profile order explicit without creating data outside EF Core migrations.
DO $$ BEGIN RAISE NOTICE 'Audit fixtures are unavailable until the audit module is migrated.'; END $$;
COMMIT;
