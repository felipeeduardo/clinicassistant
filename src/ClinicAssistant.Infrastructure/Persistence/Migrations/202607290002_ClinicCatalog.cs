using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAssistant.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ClinicAssistantDbContext))]
[Migration("202607290002_ClinicCatalog")]
public partial class ClinicCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE clinic_assistant.clinics ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL UNIQUE, "LegalName" varchar(200) NOT NULL, "TradeName" varchar(200) NOT NULL, "Document" varchar(32) NOT NULL, "Email" varchar(320) NOT NULL, "Phone" varchar(32) NOT NULL, "TimeZone" varchar(100) NOT NULL, "Status" varchar(32) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
            ALTER TABLE clinic_assistant.clinics ADD CONSTRAINT "FK_clinics_tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES clinic_assistant.tenants ("Id") ON DELETE RESTRICT;
            CREATE TABLE clinic_assistant.clinic_units ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ClinicId" uuid NOT NULL, "Name" varchar(200) NOT NULL, "Address" varchar(500) NOT NULL, "Phone" varchar(32) NOT NULL, "Status" varchar(32) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
            ALTER TABLE clinic_assistant.clinic_units ADD CONSTRAINT "FK_clinic_units_clinics_ClinicId" FOREIGN KEY ("ClinicId") REFERENCES clinic_assistant.clinics ("Id") ON DELETE RESTRICT;
            CREATE INDEX "IX_clinic_units_TenantId" ON clinic_assistant.clinic_units ("TenantId");
            CREATE TABLE clinic_assistant.specialties ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "Name" varchar(160) NOT NULL, "Description" varchar(1000), "Status" varchar(32) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
            CREATE UNIQUE INDEX "IX_specialties_TenantId_Name" ON clinic_assistant.specialties ("TenantId", "Name");
            CREATE TABLE clinic_assistant.professionals ("Id" uuid PRIMARY KEY, "TenantId" uuid NOT NULL, "ClinicUnitId" uuid NOT NULL, "Name" varchar(200) NOT NULL, "Email" varchar(320) NOT NULL, "Phone" varchar(32) NOT NULL, "RegistrationNumber" varchar(80) NOT NULL, "Status" varchar(32) NOT NULL, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz NOT NULL);
            ALTER TABLE clinic_assistant.professionals ADD CONSTRAINT "FK_professionals_units_ClinicUnitId" FOREIGN KEY ("ClinicUnitId") REFERENCES clinic_assistant.clinic_units ("Id") ON DELETE RESTRICT;
            CREATE INDEX "IX_professionals_TenantId" ON clinic_assistant.professionals ("TenantId"); CREATE UNIQUE INDEX "IX_professionals_TenantId_RegistrationNumber" ON clinic_assistant.professionals ("TenantId", "RegistrationNumber");
            CREATE TABLE clinic_assistant.professional_specialties ("ProfessionalId" uuid NOT NULL, "SpecialtyId" uuid NOT NULL, PRIMARY KEY ("ProfessionalId", "SpecialtyId"));
            ALTER TABLE clinic_assistant.professional_specialties ADD CONSTRAINT "FK_professional_specialties_professional" FOREIGN KEY ("ProfessionalId") REFERENCES clinic_assistant.professionals ("Id") ON DELETE CASCADE;
            ALTER TABLE clinic_assistant.professional_specialties ADD CONSTRAINT "FK_professional_specialties_specialty" FOREIGN KEY ("SpecialtyId") REFERENCES clinic_assistant.specialties ("Id") ON DELETE RESTRICT;
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP TABLE clinic_assistant.professional_specialties; DROP TABLE clinic_assistant.professionals; DROP TABLE clinic_assistant.specialties; DROP TABLE clinic_assistant.clinic_units; DROP TABLE clinic_assistant.clinics;");
}
