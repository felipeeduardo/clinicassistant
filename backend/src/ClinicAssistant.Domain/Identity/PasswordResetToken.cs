using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Identity;

public sealed class PasswordResetToken : Entity
{
    private PasswordResetToken() { }
    public PasswordResetToken(Guid userId, string tokenHash, DateTimeOffset expiresAt) { UserId = userId; TokenHash = tokenHash; ExpiresAt = expiresAt; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public User User { get; private set; } = null!;
    public bool IsActive(DateTimeOffset now) => UsedAt is null && ExpiresAt > now;
    public void Consume() { UsedAt = DateTimeOffset.UtcNow; UpdatedAt = DateTimeOffset.UtcNow; }
}
