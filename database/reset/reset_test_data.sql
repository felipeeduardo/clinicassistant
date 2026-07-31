BEGIN;
CREATE TEMP TABLE seed_tenants ("Id" uuid PRIMARY KEY) ON COMMIT DROP;
INSERT INTO seed_tenants ("Id")
SELECT "Id" FROM clinic_assistant.tenants
WHERE (:'profile' = 'minimal' AND "Id" = '00000000-0000-0000-0000-000000000001')
   OR (:'profile' = 'e2e' AND "Id" IN ('00000000-0000-0000-0000-000000000101','00000000-0000-0000-0000-000000000102'))
   OR (:'profile' = 'tenant' AND "Id" = :'tenant_id'::uuid);
-- A newly migrated test database has no fixture tenants yet. In that case the
-- scoped deletes below intentionally become no-ops, keeping reset idempotent.

DELETE FROM clinic_assistant.whatsapp_media m USING clinic_assistant.conversation_messages cm, seed_tenants t WHERE m."ConversationMessageId" = cm."Id" AND cm."TenantId" = t."Id";
DELETE FROM clinic_assistant.conversation_options o USING clinic_assistant.conversation_states s, seed_tenants t WHERE o."ConversationStateId" = s."Id" AND s."TenantId" = t."Id";
DELETE FROM clinic_assistant.conversation_processed_messages p USING seed_tenants t WHERE p."TenantId" = t."Id";
DELETE FROM clinic_assistant.human_queue_items q USING seed_tenants t WHERE q."TenantId" = t."Id";
DELETE FROM clinic_assistant.conversation_states s USING seed_tenants t WHERE s."TenantId" = t."Id";
DELETE FROM clinic_assistant.conversation_messages m USING seed_tenants t WHERE m."TenantId" = t."Id";
DELETE FROM clinic_assistant.conversations c USING seed_tenants t WHERE c."TenantId" = t."Id";
DELETE FROM clinic_assistant.whatsapp_templates w USING seed_tenants t WHERE w."TenantId" = t."Id";
DELETE FROM clinic_assistant.inbox_messages i USING seed_tenants t WHERE i."TenantId" = t."Id";
DELETE FROM clinic_assistant.outbox_messages o USING seed_tenants t WHERE o."TenantId" = t."Id";
DELETE FROM clinic_assistant.whatsapp_integrations w USING seed_tenants t WHERE w."TenantId" = t."Id";
DELETE FROM clinic_assistant.appointments a USING seed_tenants t WHERE a."TenantId" = t."Id";
DELETE FROM clinic_assistant.schedule_blocks b USING seed_tenants t WHERE b."TenantId" = t."Id";
DELETE FROM clinic_assistant.availability_rules r USING seed_tenants t WHERE r."TenantId" = t."Id";
DELETE FROM clinic_assistant.professional_specialties ps USING clinic_assistant.professionals p, seed_tenants t WHERE ps."ProfessionalId" = p."Id" AND p."TenantId" = t."Id";
DELETE FROM clinic_assistant.professionals p USING seed_tenants t WHERE p."TenantId" = t."Id";
DELETE FROM clinic_assistant.specialties s USING seed_tenants t WHERE s."TenantId" = t."Id";
DELETE FROM clinic_assistant.clinic_units u USING seed_tenants t WHERE u."TenantId" = t."Id";
DELETE FROM clinic_assistant.clinics c USING seed_tenants t WHERE c."TenantId" = t."Id";
DELETE FROM clinic_assistant.refresh_tokens r USING seed_tenants t WHERE r."TenantId" = t."Id";
DELETE FROM clinic_assistant.users u USING seed_tenants t WHERE u."TenantId" = t."Id";
DELETE FROM clinic_assistant.patients p USING seed_tenants t WHERE p."TenantId" = t."Id";
DELETE FROM clinic_assistant.tenants t USING seed_tenants s WHERE t."Id" = s."Id";
COMMIT;
