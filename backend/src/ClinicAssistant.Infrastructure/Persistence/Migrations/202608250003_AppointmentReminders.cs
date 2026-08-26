using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861
namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608250003_AppointmentReminders")]
public partial class AppointmentReminders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "appointment_reminders", schema: "clinic_assistant", columns: t => new { Id = t.Column<Guid>(type: "uuid", nullable: false), TenantId = t.Column<Guid>(type: "uuid", nullable: false), AppointmentId = t.Column<Guid>(type: "uuid", nullable: false), WhatsAppChannelId = t.Column<Guid>(type: "uuid", nullable: true), Type = t.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false), AppointmentStartUtc = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), ScheduledAtUtc = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), Status = t.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false), RetryCount = t.Column<int>(type: "integer", nullable: false), QueuedAtUtc = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), SentAtUtc = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), FailedAtUtc = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), ProviderCode = t.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true), FailureReason = t.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true), CorrelationId = t.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false), CreatedAt = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), UpdatedAt = t.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false) }, constraints: t => t.PrimaryKey("PK_appointment_reminders", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_appointment_reminders_Status_ScheduledAtUtc", schema: "clinic_assistant", table: "appointment_reminders", columns: new[] { "Status", "ScheduledAtUtc" });
        migrationBuilder.CreateIndex(name: "IX_appointment_reminders_TenantId_AppointmentId", schema: "clinic_assistant", table: "appointment_reminders", columns: new[] { "TenantId", "AppointmentId" });
        migrationBuilder.CreateIndex(name: "IX_appointment_reminders_AppointmentId_Type_AppointmentStartUtc", schema: "clinic_assistant", table: "appointment_reminders", columns: new[] { "AppointmentId", "Type", "AppointmentStartUtc" }, unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "appointment_reminders", schema: "clinic_assistant");
}
