using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607300001_Scheduling")]
public partial class Scheduling : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE clinic_assistant.patients ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "Name" varchar(200) NOT NULL, "Phone" varchar(32) NOT NULL, "Email" varchar(320), "BirthDate" date, "ConsentStatus" varchar(32) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_patients_TenantId_Phone" ON clinic_assistant.patients ("TenantId", "Phone"); CREATE INDEX "IX_patients_TenantId_Email" ON clinic_assistant.patients ("TenantId", "Email");
        CREATE TABLE clinic_assistant.availability_rules ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ProfessionalId" uuid NOT NULL REFERENCES clinic_assistant.professionals ("Id") ON DELETE CASCADE, "DayOfWeek" integer NOT NULL, "StartTime" time NOT NULL, "EndTime" time NOT NULL, "SlotDurationMinutes" integer NOT NULL, "Active" boolean NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_availability_rules_ProfessionalId_DayOfWeek" ON clinic_assistant.availability_rules ("ProfessionalId", "DayOfWeek");
        CREATE TABLE clinic_assistant.schedule_blocks ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ProfessionalId" uuid NOT NULL REFERENCES clinic_assistant.professionals ("Id") ON DELETE CASCADE, "StartsAt" timestamptz NOT NULL, "EndsAt" timestamptz NOT NULL, "Reason" varchar(500), "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_schedule_blocks_ProfessionalId_StartsAt" ON clinic_assistant.schedule_blocks ("ProfessionalId", "StartsAt");
        CREATE TABLE clinic_assistant.appointments ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ClinicUnitId" uuid NOT NULL, "ProfessionalId" uuid NOT NULL REFERENCES clinic_assistant.professionals ("Id") ON DELETE RESTRICT, "SpecialtyId" uuid NOT NULL, "PatientId" uuid NOT NULL REFERENCES clinic_assistant.patients ("Id") ON DELETE RESTRICT, "StartsAt" timestamptz NOT NULL, "EndsAt" timestamptz NOT NULL, "Status" varchar(32) NOT NULL, "Source" varchar(32) NOT NULL, "Notes" varchar(1000), "CancelledAt" timestamptz, "CancellationReason" varchar(500), "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
        CREATE INDEX "IX_appointments_ProfessionalId_StartsAt" ON clinic_assistant.appointments ("ProfessionalId", "StartsAt"); CREATE INDEX "IX_appointments_PatientId_StartsAt" ON clinic_assistant.appointments ("PatientId", "StartsAt"); CREATE INDEX "IX_appointments_Status_CreatedAt" ON clinic_assistant.appointments ("Status", "CreatedAt");
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.appointments; DROP TABLE clinic_assistant.schedule_blocks; DROP TABLE clinic_assistant.availability_rules; DROP TABLE clinic_assistant.patients;");
}
