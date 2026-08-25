using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608250002_LeadPipeline")]
public partial class LeadPipeline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(name: "NextActionAt", schema: "clinic_assistant", table: "demo_leads", nullable: true);
        migrationBuilder.AddColumn<string>(name: "NextActionDescription", schema: "clinic_assistant", table: "demo_leads", type: "character varying(500)", maxLength: 500, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "NextActionAt", schema: "clinic_assistant", table: "demo_leads");
        migrationBuilder.DropColumn(name: "NextActionDescription", schema: "clinic_assistant", table: "demo_leads");
    }
}
