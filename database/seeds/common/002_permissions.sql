-- Authorization is role-based in the current application.  The effective roles
-- are seeded with users in each profile; there is no permissions table yet.
BEGIN;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uuid') THEN
        RAISE EXCEPTION 'PostgreSQL UUID support is required.';
    END IF;
END $$;
COMMIT;
