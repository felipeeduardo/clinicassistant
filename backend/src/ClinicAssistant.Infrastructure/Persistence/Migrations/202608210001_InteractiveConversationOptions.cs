using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608210001_InteractiveConversationOptions")]
public partial class InteractiveConversationOptionsMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE clinic_assistant.conversation_options ADD COLUMN IF NOT EXISTS \"ActionId\" varchar(128) NULL;");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE clinic_assistant.conversation_options DROP COLUMN IF EXISTS \"ActionId\";");
}
