using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608230001_AssistantDisplayName")]
public partial class AssistantDisplayNameMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE clinic_assistant.clinics ADD COLUMN IF NOT EXISTS \"AssistantDisplayName\" varchar(60) NOT NULL DEFAULT 'IA Recepção';");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE clinic_assistant.clinics DROP COLUMN IF EXISTS \"AssistantDisplayName\";");
}
