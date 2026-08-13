using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607290001_InitialIdentity")]
public partial class InitialIdentity : Migration
{
    private static readonly string[] RefreshTokenUserExpiryColumns = ["UserId", "ExpiresAt"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "clinic_assistant");
        migrationBuilder.CreateTable(
            name: "tenants", schema: "clinic_assistant",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_tenants", x => x.Id));
        migrationBuilder.CreateTable(
            name: "users", schema: "clinic_assistant",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false), TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.Id);
                table.ForeignKey(
                    name: "FK_users_tenants_TenantId",
                    column: x => x.TenantId,
                    principalSchema: "clinic_assistant",
                    principalTable: "tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateTable(
            name: "refresh_tokens", schema: "clinic_assistant",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false), TenantId = table.Column<Guid>(type: "uuid", nullable: false), UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false), ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), ReplacedByTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_refresh_tokens_users_UserId",
                    column: x => x.UserId,
                    principalSchema: "clinic_assistant",
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_tenants_Slug", schema: "clinic_assistant", table: "tenants", column: "Slug", unique: true);
        migrationBuilder.CreateIndex(name: "IX_users_Email", schema: "clinic_assistant", table: "users", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_users_TenantId", schema: "clinic_assistant", table: "users", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_refresh_tokens_TokenHash", schema: "clinic_assistant", table: "refresh_tokens", column: "TokenHash", unique: true);
        migrationBuilder.CreateIndex(name: "IX_refresh_tokens_TenantId", schema: "clinic_assistant", table: "refresh_tokens", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_refresh_tokens_UserId_ExpiresAt", schema: "clinic_assistant", table: "refresh_tokens", columns: RefreshTokenUserExpiryColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "refresh_tokens", schema: "clinic_assistant");
        migrationBuilder.DropTable(name: "users", schema: "clinic_assistant");
        migrationBuilder.DropTable(name: "tenants", schema: "clinic_assistant");
    }
}
