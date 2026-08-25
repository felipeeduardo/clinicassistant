using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

public partial class DemoLeadAttribution : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "LandingPage", schema: "clinic_assistant", table: "demo_leads", type: "character varying(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Referrer", schema: "clinic_assistant", table: "demo_leads", type: "character varying(1000)", maxLength: 1000, nullable: true);
        migrationBuilder.AddColumn<string>(name: "UtmCampaign", schema: "clinic_assistant", table: "demo_leads", type: "character varying(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>(name: "UtmContent", schema: "clinic_assistant", table: "demo_leads", type: "character varying(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>(name: "UtmMedium", schema: "clinic_assistant", table: "demo_leads", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "UtmSource", schema: "clinic_assistant", table: "demo_leads", type: "character varying(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "UtmTerm", schema: "clinic_assistant", table: "demo_leads", type: "character varying(200)", maxLength: 200, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var column in new[] { "LandingPage", "Referrer", "UtmCampaign", "UtmContent", "UtmMedium", "UtmSource", "UtmTerm" })
            migrationBuilder.DropColumn(name: column, schema: "clinic_assistant", table: "demo_leads");
    }
}
