using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608230002_HumanQueueNotifications")]
public partial class HumanQueueNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>("WaitingSince", "conversations", schema: "clinic_assistant", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>("HumanQueueReminderSentAt", "conversations", schema: "clinic_assistant", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>("HumanQueueSlaExceededAt", "conversations", schema: "clinic_assistant", nullable: true);
        migrationBuilder.CreateTable(name: "operational_notifications", schema: "clinic_assistant", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            TenantId = table.Column<Guid>(type: "uuid", nullable: false),
            ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
            Type = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
            Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
            Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
            CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
            CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_operational_notifications", x => x.Id));
        migrationBuilder.CreateIndex("IX_operational_notifications_TenantId_ConversationId_Type", "operational_notifications", new[] { "TenantId", "ConversationId", "Type" }, "clinic_assistant", unique: true);
        migrationBuilder.CreateIndex("IX_operational_notifications_TenantId_Status_CreatedAt", "operational_notifications", new[] { "TenantId", "Status", "CreatedAt" }, "clinic_assistant");
        migrationBuilder.CreateIndex("IX_conversations_TenantId_Status_WaitingSince", "conversations", new[] { "TenantId", "Status", "WaitingSince" }, "clinic_assistant");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("operational_notifications", "clinic_assistant");
        migrationBuilder.DropIndex("IX_conversations_TenantId_Status_WaitingSince", "conversations", "clinic_assistant");
        migrationBuilder.DropColumn("WaitingSince", "conversations", "clinic_assistant");
        migrationBuilder.DropColumn("HumanQueueReminderSentAt", "conversations", "clinic_assistant");
        migrationBuilder.DropColumn("HumanQueueSlaExceededAt", "conversations", "clinic_assistant");
    }
}
