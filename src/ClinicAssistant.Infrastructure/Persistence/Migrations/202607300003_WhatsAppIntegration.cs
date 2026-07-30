using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300003_WhatsAppIntegration")]
public partial class WhatsAppIntegration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.whatsapp_integrations ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "PhoneNumberId" varchar(120) NOT NULL, "Active" boolean NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE UNIQUE INDEX "IX_whatsapp_integrations_PhoneNumberId" ON clinic_assistant.whatsapp_integrations ("PhoneNumberId"); CREATE INDEX "IX_whatsapp_integrations_TenantId" ON clinic_assistant.whatsapp_integrations ("TenantId");
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.whatsapp_integrations;");
}
