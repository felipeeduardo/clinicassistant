BEGIN;
DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM clinic_assistant.human_queue_items q JOIN clinic_assistant.conversations c ON c."Id"=q."ConversationId" WHERE q."TenantId"<>c."TenantId") THEN RAISE EXCEPTION 'Human queue tenant isolation violation.'; END IF;
  IF EXISTS (SELECT 1 FROM clinic_assistant.conversation_states s JOIN clinic_assistant.conversations c ON c."Id"=s."ConversationId" WHERE s."TenantId"<>c."TenantId") THEN RAISE EXCEPTION 'Conversation state tenant isolation violation.'; END IF;
END $$;
COMMIT;
