using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300005_OutboxRetryScheduling")]
public partial class OutboxRetryScheduling : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE clinic_assistant.outbox_messages ADD COLUMN "FirstFailureAt" timestamptz, ADD COLUMN "NextAttemptAt" timestamptz;
        DROP INDEX clinic_assistant."IX_outbox_messages_Status_CreatedAt";
        CREATE INDEX "IX_outbox_messages_Status_NextAttemptAt_CreatedAt" ON clinic_assistant.outbox_messages ("Status", "NextAttemptAt", "CreatedAt");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP INDEX clinic_assistant."IX_outbox_messages_Status_NextAttemptAt_CreatedAt";
        CREATE INDEX "IX_outbox_messages_Status_CreatedAt" ON clinic_assistant.outbox_messages ("Status", "CreatedAt");
        ALTER TABLE clinic_assistant.outbox_messages DROP COLUMN "FirstFailureAt", DROP COLUMN "NextAttemptAt";
        """);
}
