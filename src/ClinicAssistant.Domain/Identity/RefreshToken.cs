using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Identity;

public sealed class RefreshToken : Entity, ITenantEntity
{
    private RefreshToken() { }
    public RefreshToken(Guid tenantId, Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        TenantId = tenantId; UserId = userId; TokenHash = tokenHash; ExpiresAt = expiresAt;
    }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public User User { get; private set; } = null!;
    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
    public void Revoke(string? replacedByTokenHash = null) { RevokedAt = DateTimeOffset.UtcNow; ReplacedByTokenHash = replacedByTokenHash; UpdatedAt = DateTimeOffset.UtcNow; }
}
