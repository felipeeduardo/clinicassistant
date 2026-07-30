BEGIN;
-- Valid application roles: PlatformAdmin, ClinicAdmin, Receptionist, Professional.
-- This assertion makes accidental schema drift fail before profile data is inserted.
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'clinic_assistant' AND table_name = 'users' AND column_name = 'Role') THEN
        RAISE EXCEPTION 'The users.Role column is required for test data.';
    END IF;
END $$;
COMMIT;
