BEGIN;
DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM clinic_assistant.appointments a LEFT JOIN clinic_assistant.patients p ON p."Id"=a."PatientId" AND p."TenantId"=a."TenantId" WHERE p."Id" IS NULL) THEN RAISE EXCEPTION 'Appointment/patient reference is invalid.'; END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.conversations c LEFT JOIN clinic_assistant.patients p ON p."Id"=c."PatientId" AND p."TenantId"=c."TenantId" WHERE p."Id" IS NULL) THEN RAISE EXCEPTION 'Conversation/patient reference is invalid.'; END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.conversations c LEFT JOIN clinic_assistant.whatsapp_integrations i ON i."Id"=c."IntegrationId" AND i."TenantId"=c."TenantId" WHERE i."Id" IS NULL) THEN RAISE EXCEPTION 'Conversation/integration reference is invalid.'; END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.conversation_messages m LEFT JOIN clinic_assistant.conversations c ON c."Id"=m."ConversationId" AND c."TenantId"=m."TenantId" WHERE c."Id" IS NULL) THEN RAISE EXCEPTION 'Message/conversation reference is invalid.'; END IF;
END $$;
COMMIT;
