using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace ClinicAssistant.Infrastructure.Persistence.Migrations;
[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608030001_PlatformAdministration")]
public partial class PlatformAdministrationMigration : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""CREATE TABLE clinic_assistant.idempotency_records ("Id" uuid PRIMARY KEY, "Scope" varchar(120) NOT NULL, "Key" varchar(200) NOT NULL, "ResponseJson" varchar(8000) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL); CREATE UNIQUE INDEX "IX_idempotency_records_Scope_Key" ON clinic_assistant.idempotency_records ("Scope", "Key"); CREATE TABLE clinic_assistant.audit_records ("Id" uuid PRIMARY KEY, "TenantId" uuid NULL, "ActorUserId" uuid NULL, "Action" varchar(120) NOT NULL, "ResourceType" varchar(120) NOT NULL, "ResourceId" uuid NULL, "Result" varchar(32) NOT NULL, "Details" varchar(2000) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL); CREATE INDEX "IX_audit_records_TenantId_CreatedAt" ON clinic_assistant.audit_records ("TenantId", "CreatedAt");""");
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.audit_records; DROP TABLE clinic_assistant.idempotency_records;");
}
