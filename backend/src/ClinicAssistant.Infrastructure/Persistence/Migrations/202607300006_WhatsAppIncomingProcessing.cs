using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300006_WhatsAppIncomingProcessing")]
public partial class WhatsAppIncomingProcessing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE clinic_assistant.patients ADD COLUMN "Source" varchar(32) NOT NULL DEFAULT 'Manual', ADD COLUMN "FirstContactAt" timestamptz, ADD COLUMN "LastContactAt" timestamptz;
        DROP INDEX clinic_assistant."IX_patients_TenantId_Phone";
        CREATE UNIQUE INDEX "IX_patients_TenantId_Phone" ON clinic_assistant.patients ("TenantId", "Phone");
        DROP INDEX clinic_assistant."IX_conversation_messages_Provider_ExternalMessageId";
        CREATE UNIQUE INDEX "IX_conversation_messages_TenantId_Provider_ExternalMessageId" ON clinic_assistant.conversation_messages ("TenantId", "Provider", "ExternalMessageId");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP INDEX clinic_assistant."IX_conversation_messages_TenantId_Provider_ExternalMessageId";
        CREATE INDEX "IX_conversation_messages_Provider_ExternalMessageId" ON clinic_assistant.conversation_messages ("Provider", "ExternalMessageId");
        DROP INDEX clinic_assistant."IX_patients_TenantId_Phone";
        CREATE INDEX "IX_patients_TenantId_Phone" ON clinic_assistant.patients ("TenantId", "Phone");
        ALTER TABLE clinic_assistant.patients DROP COLUMN "Source", DROP COLUMN "FirstContactAt", DROP COLUMN "LastContactAt";
        """);
}
