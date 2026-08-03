using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608030003_ProfessionalVacations")]
public partial class ProfessionalVacationsMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""CREATE TABLE clinic_assistant.professional_vacations ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ProfessionalId" uuid NOT NULL, "StartsAt" timestamptz NOT NULL, "EndsAt" timestamptz NOT NULL, "Reason" varchar(500), "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL); ALTER TABLE clinic_assistant.professional_vacations ADD CONSTRAINT "FK_professional_vacations_professionals_ProfessionalId" FOREIGN KEY ("ProfessionalId") REFERENCES clinic_assistant.professionals ("Id") ON DELETE CASCADE; CREATE INDEX "IX_professional_vacations_ProfessionalId_StartsAt" ON clinic_assistant.professional_vacations ("ProfessionalId", "StartsAt");""");
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.professional_vacations;");
}
