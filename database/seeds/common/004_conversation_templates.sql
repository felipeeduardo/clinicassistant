BEGIN;
DO $$ BEGIN
    IF to_regclass('clinic_assistant.whatsapp_templates') IS NULL THEN
        RAISE EXCEPTION 'The WhatsApp template schema is required for test data.';
    END IF;
END $$;
COMMIT;
