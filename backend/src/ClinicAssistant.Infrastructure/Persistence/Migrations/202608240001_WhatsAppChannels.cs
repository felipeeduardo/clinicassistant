using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608240001_WhatsAppChannels")]
public partial class WhatsAppChannels : Migration
{
    private static readonly string[] TenantDefaultColumns = ["TenantId", "IsDefault"];
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "whatsapp_channels",
            schema: "clinic_assistant",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                IntegrationId = table.Column<Guid>(type: "uuid", nullable: true),
                Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                NormalizedPhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                DisplayPhoneNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                ProviderSenderId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastValidationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastInboundAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastOutboundAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastFailureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_whatsapp_channels", x => x.Id); });
        migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_whatsapp_channels_NormalizedPhoneNumber\" ON clinic_assistant.whatsapp_channels (\"NormalizedPhoneNumber\") WHERE \"Status\" = 'Active';");
        migrationBuilder.CreateIndex("IX_whatsapp_channels_TenantId", "whatsapp_channels", "TenantId", "clinic_assistant");
        migrationBuilder.CreateIndex("IX_whatsapp_channels_Tenant_Default", "whatsapp_channels", TenantDefaultColumns, "clinic_assistant");
        migrationBuilder.AddColumn<Guid>("WhatsAppChannelId", "conversations", schema: "clinic_assistant", nullable: true);
        migrationBuilder.AddColumn<Guid>("WhatsAppChannelId", "outbox_messages", schema: "clinic_assistant", nullable: true);
        migrationBuilder.CreateIndex("IX_conversations_WhatsAppChannelId", "conversations", "WhatsAppChannelId", "clinic_assistant");
        migrationBuilder.CreateIndex("IX_outbox_messages_WhatsAppChannelId", "outbox_messages", "WhatsAppChannelId", "clinic_assistant");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_conversations_WhatsAppChannelId", "conversations", "clinic_assistant");
        migrationBuilder.DropIndex("IX_outbox_messages_WhatsAppChannelId", "outbox_messages", "clinic_assistant");
        migrationBuilder.DropColumn("WhatsAppChannelId", "conversations", "clinic_assistant");
        migrationBuilder.DropColumn("WhatsAppChannelId", "outbox_messages", "clinic_assistant");
        migrationBuilder.DropTable("whatsapp_channels", "clinic_assistant");
    }
}
