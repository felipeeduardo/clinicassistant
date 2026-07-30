using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300008_ConversationFoundation")]
public partial class ConversationFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE clinic_assistant.conversations
            ADD COLUMN "AutomationMode" varchar(32) NOT NULL DEFAULT 'Automated',
            ADD COLUMN "Priority" varchar(32) NOT NULL DEFAULT 'Normal',
            ADD COLUMN "Version" integer NOT NULL DEFAULT 1;

        CREATE TABLE clinic_assistant.conversation_states (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationId" uuid NOT NULL,
            "Status" varchar(32) NOT NULL, "FlowState" varchar(64) NOT NULL, "Intent" varchar(64) NOT NULL,
            "ContextJson" jsonb NOT NULL DEFAULT '{}'::jsonb, "InvalidAttempts" integer NOT NULL,
            "ExpiresAt" timestamptz NOT NULL, "LastInteractionAt" timestamptz, "Version" integer NOT NULL,
            "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_conversation_states_ConversationId" ON clinic_assistant.conversation_states ("ConversationId");
        CREATE INDEX "IX_conversation_states_TenantId_ExpiresAt" ON clinic_assistant.conversation_states ("TenantId", "ExpiresAt");

        CREATE TABLE clinic_assistant.conversation_processed_messages (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationId" uuid NOT NULL,
            "ConversationMessageId" uuid NOT NULL, "ResponseMessageId" uuid, "OutboxMessageId" uuid,
            "ProcessedAt" timestamptz NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_conversation_processed_messages_TenantId_ConversationMessageId" ON clinic_assistant.conversation_processed_messages ("TenantId", "ConversationMessageId");
        CREATE INDEX "IX_conversation_processed_messages_TenantId_ConversationId_ProcessedAt" ON clinic_assistant.conversation_processed_messages ("TenantId", "ConversationId", "ProcessedAt");

        CREATE TABLE clinic_assistant.conversation_options (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationStateId" uuid NOT NULL,
            "Key" varchar(64) NOT NULL, "Value" varchar(256) NOT NULL, "DisplayOrder" integer NOT NULL,
            "ExpiresAt" timestamptz NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_conversation_options_ConversationStateId_Key" ON clinic_assistant.conversation_options ("ConversationStateId", "Key");
        CREATE INDEX "IX_conversation_options_TenantId_ExpiresAt" ON clinic_assistant.conversation_options ("TenantId", "ExpiresAt");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE clinic_assistant.conversation_options;
        DROP TABLE clinic_assistant.conversation_processed_messages;
        DROP TABLE clinic_assistant.conversation_states;
        ALTER TABLE clinic_assistant.conversations DROP COLUMN "AutomationMode", DROP COLUMN "Priority", DROP COLUMN "Version";
        """);
}
