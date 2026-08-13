using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608030002_UnitBusinessHours")]
public partial class UnitBusinessHoursMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""CREATE TABLE clinic_assistant.unit_business_hours ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ClinicUnitId" uuid NOT NULL, "DayOfWeek" integer NOT NULL, "OpensAt" time NOT NULL, "ClosesAt" time NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL); ALTER TABLE clinic_assistant.unit_business_hours ADD CONSTRAINT "FK_unit_business_hours_clinic_units_ClinicUnitId" FOREIGN KEY ("ClinicUnitId") REFERENCES clinic_assistant.clinic_units ("Id") ON DELETE CASCADE; CREATE UNIQUE INDEX "IX_unit_business_hours_ClinicUnitId_DayOfWeek" ON clinic_assistant.unit_business_hours ("ClinicUnitId", "DayOfWeek");""");
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.unit_business_hours;");
}
