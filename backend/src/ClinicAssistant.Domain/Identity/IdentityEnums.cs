namespace ClinicAssistant.Domain.Identity;

public enum TenantStatus { Provisioning, Active, Suspended, Blocked }
public enum UserRole { PlatformAdmin, ClinicAdmin, Receptionist, Professional, Viewer }
public enum UserStatus { Active, Blocked, Disabled }
