namespace ClinicAssistant.Domain.Identity;

public enum TenantStatus { Active, Suspended, Blocked }
public enum UserRole { PlatformAdmin, ClinicAdmin, Receptionist, Professional, Viewer }
public enum UserStatus { Active, Blocked, Disabled }
