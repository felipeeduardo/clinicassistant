using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace ClinicAssistant.Infrastructure.Persistence.Migrations;
[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202608180002_PasswordResetTokens")]
public partial class PasswordResetTokensMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.password_reset_tokens ("Id" uuid NOT NULL, "CreatedAt" timestamp with time zone NOT NULL, "UpdatedAt" timestamp with time zone NOT NULL, "UserId" uuid NOT NULL, "TokenHash" character varying(64) NOT NULL, "ExpiresAt" timestamp with time zone NOT NULL, "UsedAt" timestamp with time zone NULL, CONSTRAINT "PK_password_reset_tokens" PRIMARY KEY ("Id"), CONSTRAINT "FK_password_reset_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES clinic_assistant.users ("Id") ON DELETE CASCADE);
        CREATE UNIQUE INDEX "IX_password_reset_tokens_TokenHash" ON clinic_assistant.password_reset_tokens ("TokenHash");
        CREATE INDEX "IX_password_reset_tokens_UserId_ExpiresAt" ON clinic_assistant.password_reset_tokens ("UserId", "ExpiresAt");
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.password_reset_tokens;");
}
