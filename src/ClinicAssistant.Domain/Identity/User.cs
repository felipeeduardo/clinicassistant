using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Identity;

public sealed class User : Entity, ITenantEntity
{
    private User() { }
    public User(Guid tenantId, string name, string email, string passwordHash, UserRole role)
    {
        TenantId = tenantId; Name = name; Email = email; PasswordHash = passwordHash; Role = role;
    }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTimeOffset? LastLoginAt { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    public void RecordLogin() { LastLoginAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
    public void UpdateRole(UserRole role) { Role = role; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Activate() { Status = UserStatus.Active; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Suspend() { Status = UserStatus.Blocked; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Disable() { Status = UserStatus.Disabled; UpdatedAt = DateTimeOffset.UtcNow; }
}
