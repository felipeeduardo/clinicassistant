using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace ClinicAssistant.Infrastructure.Persistence.Migrations;
[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300009_HumanQueue")]
public partial class HumanQueueMigration : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""CREATE TABLE clinic_assistant.human_queue_items ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ConversationId" uuid NOT NULL, "Status" varchar(32) NOT NULL, "Priority" varchar(32) NOT NULL, "AssignedUserId" uuid, "Reason" varchar(500), "Version" integer NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL); CREATE UNIQUE INDEX "IX_human_queue_items_ConversationId" ON clinic_assistant.human_queue_items ("ConversationId"); CREATE INDEX "IX_human_queue_items_TenantId_Status_CreatedAt" ON clinic_assistant.human_queue_items ("TenantId", "Status", "CreatedAt");""");
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.human_queue_items;");
}
