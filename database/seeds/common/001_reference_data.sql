BEGIN;

DO $$
BEGIN
    IF to_regclass('clinic_assistant.tenants') IS NULL
       OR to_regclass('clinic_assistant.users') IS NULL
       OR to_regclass('clinic_assistant.conversations') IS NULL THEN
        RAISE EXCEPTION 'Clinic Assistant migrations have not been applied.';
    END IF;
END $$;

COMMIT;
