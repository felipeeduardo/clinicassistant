using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300004_WhatsAppTwilioFoundation")]
public partial class WhatsAppTwilioFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP INDEX clinic_assistant."IX_whatsapp_integrations_PhoneNumberId";
        ALTER TABLE clinic_assistant.whatsapp_integrations
            DROP COLUMN "PhoneNumberId",
            DROP COLUMN "Active",
            ADD COLUMN "Provider" varchar(32) NOT NULL DEFAULT 'Meta',
            ADD COLUMN "IntegrationKey" varchar(80) NOT NULL DEFAULT '',
            ADD COLUMN "AccountSidReference" varchar(80),
            ADD COLUMN "MessagingServiceSid" varchar(80),
            ADD COLUMN "WhatsAppFrom" varchar(32) NOT NULL DEFAULT '',
            ADD COLUMN "DisplayPhoneNumber" varchar(80),
            ADD COLUMN "Status" varchar(32) NOT NULL DEFAULT 'Disabled',
            ADD COLUMN "ConnectedAt" timestamptz,
            ADD COLUMN "LastValidatedAt" timestamptz,
            ADD COLUMN "LastWebhookAt" timestamptz,
            ADD COLUMN "LastSuccessfulSendAt" timestamptz,
            ADD COLUMN "LastFailureAt" timestamptz,
            ADD COLUMN "FailureReason" varchar(1000);
        UPDATE clinic_assistant.whatsapp_integrations
        SET "IntegrationKey" = 'wha_' || substring(replace("Id"::text, '-', '') from 1 for 16);
        CREATE UNIQUE INDEX "IX_whatsapp_integrations_IntegrationKey" ON clinic_assistant.whatsapp_integrations ("IntegrationKey");

        ALTER TABLE clinic_assistant.inbox_messages
            RENAME COLUMN "Payload" TO "RawPayload";
        ALTER TABLE clinic_assistant.inbox_messages
            RENAME COLUMN "LastError" TO "LastErrorMessage";
        ALTER TABLE clinic_assistant.inbox_messages
            ADD COLUMN "IntegrationId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ADD COLUMN "EventType" varchar(80) NOT NULL DEFAULT 'incoming_message',
            ADD COLUMN "ExternalEventId" varchar(200),
            ADD COLUMN "PayloadHash" varchar(64) NOT NULL DEFAULT '',
            ADD COLUMN "QueuedAt" timestamptz,
            ADD COLUMN "ProcessingStartedAt" timestamptz,
            ADD COLUMN "LastErrorCode" varchar(120),
            ADD COLUMN "CorrelationId" varchar(128);
        UPDATE clinic_assistant.inbox_messages SET "Status" = 'Received' WHERE "Status" = 'Pending';
        DROP INDEX clinic_assistant."IX_inbox_messages_Provider_ExternalMessageId";
        CREATE UNIQUE INDEX "IX_inbox_messages_Provider_IntegrationId_ExternalMessageId" ON clinic_assistant.inbox_messages ("Provider", "IntegrationId", "ExternalMessageId");

        CREATE TABLE clinic_assistant.whatsapp_templates (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "IntegrationId" uuid NOT NULL,
            "Provider" varchar(32) NOT NULL, "ExternalTemplateId" varchar(160), "ContentSid" varchar(80) NOT NULL,
            "Name" varchar(160) NOT NULL, "LanguageCode" varchar(16) NOT NULL, "Category" varchar(80),
            "Status" varchar(32) NOT NULL, "ParametersSchema" text,
            "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_whatsapp_templates_IntegrationId_ContentSid" ON clinic_assistant.whatsapp_templates ("IntegrationId", "ContentSid");
        CREATE INDEX "IX_whatsapp_templates_TenantId" ON clinic_assistant.whatsapp_templates ("TenantId");

        CREATE TABLE clinic_assistant.conversations (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "PatientId" uuid NOT NULL, "Channel" varchar(32) NOT NULL,
            "IntegrationId" uuid NOT NULL, "ExternalContactId" varchar(120), "Status" varchar(32) NOT NULL,
            "AssignedUserId" uuid, "StartedAt" timestamptz NOT NULL, "LastMessageAt" timestamptz, "ClosedAt" timestamptz,
            "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_conversations_TenantId_IntegrationId_PatientId_Status" ON clinic_assistant.conversations ("TenantId", "IntegrationId", "PatientId", "Status");

        CREATE TABLE clinic_assistant.conversation_messages (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationId" uuid NOT NULL,
            "Direction" varchar(32) NOT NULL, "Type" varchar(32) NOT NULL, "Content" text, "ContentSanitized" text,
            "Provider" varchar(32) NOT NULL, "ExternalMessageId" varchar(200), "ExternalReplyToMessageId" varchar(200),
            "Status" varchar(32) NOT NULL, "ProviderStatus" varchar(80), "ProviderErrorCode" varchar(120), "ProviderErrorMessage" varchar(2000),
            "QueuedAt" timestamptz, "AcceptedAt" timestamptz, "SentAt" timestamptz, "DeliveredAt" timestamptz,
            "ReadAt" timestamptz, "FailedAt" timestamptz, "ReceivedAt" timestamptz,
            "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_conversation_messages_ConversationId_CreatedAt" ON clinic_assistant.conversation_messages ("ConversationId", "CreatedAt");
        CREATE INDEX "IX_conversation_messages_Provider_ExternalMessageId" ON clinic_assistant.conversation_messages ("Provider", "ExternalMessageId");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE clinic_assistant.conversation_messages;
        DROP TABLE clinic_assistant.conversations;
        DROP TABLE clinic_assistant.whatsapp_templates;
        DROP INDEX clinic_assistant."IX_inbox_messages_Provider_IntegrationId_ExternalMessageId";
        ALTER TABLE clinic_assistant.inbox_messages DROP COLUMN "IntegrationId", DROP COLUMN "EventType", DROP COLUMN "ExternalEventId", DROP COLUMN "PayloadHash", DROP COLUMN "QueuedAt", DROP COLUMN "ProcessingStartedAt", DROP COLUMN "LastErrorCode", DROP COLUMN "CorrelationId";
        ALTER TABLE clinic_assistant.inbox_messages RENAME COLUMN "RawPayload" TO "Payload";
        ALTER TABLE clinic_assistant.inbox_messages RENAME COLUMN "LastErrorMessage" TO "LastError";
        CREATE UNIQUE INDEX "IX_inbox_messages_Provider_ExternalMessageId" ON clinic_assistant.inbox_messages ("Provider", "ExternalMessageId");
        DROP INDEX clinic_assistant."IX_whatsapp_integrations_IntegrationKey";
        ALTER TABLE clinic_assistant.whatsapp_integrations DROP COLUMN "Provider", DROP COLUMN "IntegrationKey", DROP COLUMN "AccountSidReference", DROP COLUMN "MessagingServiceSid", DROP COLUMN "WhatsAppFrom", DROP COLUMN "DisplayPhoneNumber", DROP COLUMN "Status", DROP COLUMN "ConnectedAt", DROP COLUMN "LastValidatedAt", DROP COLUMN "LastWebhookAt", DROP COLUMN "LastSuccessfulSendAt", DROP COLUMN "LastFailureAt", DROP COLUMN "FailureReason";
        ALTER TABLE clinic_assistant.whatsapp_integrations ADD COLUMN "PhoneNumberId" varchar(120) NOT NULL DEFAULT '', ADD COLUMN "Active" boolean NOT NULL DEFAULT false;
        CREATE UNIQUE INDEX "IX_whatsapp_integrations_PhoneNumberId" ON clinic_assistant.whatsapp_integrations ("PhoneNumberId");
        """);
}
