using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608240002_WhatsAppOnboardingAssessment")]
public partial class WhatsAppOnboardingAssessment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("NumberOrigin", "whatsapp_channels", schema: "clinic_assistant", type: "character varying(48)", nullable: false, defaultValue: "ExistingClinicNumber");
        migrationBuilder.AddColumn<string>("CurrentUsage", "whatsapp_channels", schema: "clinic_assistant", type: "character varying(64)", nullable: false, defaultValue: "Unknown");
        migrationBuilder.AddColumn<string>("OnboardingStatus", "whatsapp_channels", schema: "clinic_assistant", type: "character varying(48)", nullable: false, defaultValue: "NeedsAssessment");
        migrationBuilder.AddColumn<string>("ValidationMessage", "whatsapp_channels", schema: "clinic_assistant", type: "character varying(1000)", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("NumberOrigin", "whatsapp_channels", schema: "clinic_assistant");
        migrationBuilder.DropColumn("CurrentUsage", "whatsapp_channels", schema: "clinic_assistant");
        migrationBuilder.DropColumn("OnboardingStatus", "whatsapp_channels", schema: "clinic_assistant");
        migrationBuilder.DropColumn("ValidationMessage", "whatsapp_channels", schema: "clinic_assistant");
    }
}
