using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608180001_DemoLeads")]
public partial class DemoLeadsMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.demo_leads (
            "Id" uuid NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            "FullName" character varying(200) NOT NULL,
            "CompanyOrClinicName" character varying(200) NOT NULL,
            "Email" character varying(320) NOT NULL,
            "Phone" character varying(40) NOT NULL,
            "Description" character varying(2000),
            "Source" character varying(80) NOT NULL,
            "Status" character varying(32) NOT NULL,
            "AssignedToUserId" uuid,
            "LastContactAt" timestamp with time zone,
            "Version" integer NOT NULL DEFAULT 1,
            CONSTRAINT "PK_demo_leads" PRIMARY KEY ("Id")
        );
        CREATE INDEX "IX_demo_leads_Status_CreatedAt" ON clinic_assistant.demo_leads ("Status", "CreatedAt");
        CREATE INDEX "IX_demo_leads_Email" ON clinic_assistant.demo_leads ("Email");
        CREATE INDEX "IX_demo_leads_AssignedToUserId" ON clinic_assistant.demo_leads ("AssignedToUserId");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.demo_leads;");
}
