using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300002_Messaging")]
public partial class Messaging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.inbox_messages ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "Provider" varchar(80) NOT NULL, "ExternalMessageId" varchar(200) NOT NULL, "Payload" text NOT NULL, "Status" varchar(32) NOT NULL, "ReceivedAt" timestamptz NOT NULL, "ProcessedAt" timestamptz, "RetryCount" integer NOT NULL, "LastError" varchar(2000), "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_inbox_messages_Provider_ExternalMessageId" ON clinic_assistant.inbox_messages ("Provider", "ExternalMessageId"); CREATE INDEX "IX_inbox_messages_Status_CreatedAt" ON clinic_assistant.inbox_messages ("Status", "CreatedAt");
        CREATE TABLE clinic_assistant.outbox_messages ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "Type" varchar(120) NOT NULL, "Payload" text NOT NULL, "Status" varchar(32) NOT NULL, "ProcessedAt" timestamptz, "RetryCount" integer NOT NULL, "LastError" varchar(2000), "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_outbox_messages_Status_CreatedAt" ON clinic_assistant.outbox_messages ("Status", "CreatedAt");
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.outbox_messages; DROP TABLE clinic_assistant.inbox_messages;");
}
