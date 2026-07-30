using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300007_WhatsAppMedia")]
public partial class WhatsAppMedia : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.whatsapp_media (
            "Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationMessageId" uuid NOT NULL,
            "SourceUrl" varchar(2048) NOT NULL, "ContentType" varchar(120), "ContentLength" bigint,
            "Index" integer NOT NULL, "Status" varchar(32) NOT NULL, "SafeReason" varchar(500),
            "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_whatsapp_media_ConversationMessageId_Index" ON clinic_assistant.whatsapp_media ("ConversationMessageId", "Index");
        CREATE INDEX "IX_whatsapp_media_TenantId" ON clinic_assistant.whatsapp_media ("TenantId");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE clinic_assistant.whatsapp_media;
        """);
}
