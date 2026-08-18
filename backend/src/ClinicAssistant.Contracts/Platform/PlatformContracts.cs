namespace ClinicAssistant.Contracts.Platform;

public sealed record PlatformTenantResponse(Guid Id, string Name, string Slug, string Status, Guid? ClinicId, int UserCount);
public sealed record PlatformUserResponse(Guid Id, Guid TenantId, string Name, string Email, string Role, string Status);
public sealed record PlatformClinicResponse(Guid Id, Guid TenantId, string TradeName, string LegalName, string Status);
public sealed record OnboardTenantRequest(string TenantName, string TenantSlug, string ClinicLegalName, string ClinicTradeName, string ClinicDocument, string ClinicEmail, string ClinicPhone, string TimeZone, string UnitName, string UnitAddress, string UnitPhone, string AdminName, string AdminEmail, string TemporaryPassword);
public sealed record OnboardTenantResponse(Guid TenantId, Guid ClinicId, Guid UnitId, Guid AdminUserId, Guid IntegrationId, bool Replayed);
public sealed record PlatformOnboardingStatusResponse(Guid TenantId, bool ClinicConfigured, bool UnitConfigured, bool SpecialtiesConfigured, bool ProfessionalsConfigured, bool AvailabilityConfigured, bool ClinicAdminConfigured, bool WhatsAppConfigured, bool CanActivate);
public sealed record CreateClinicAdminRequest(string Name, string Email, string TemporaryPassword);
