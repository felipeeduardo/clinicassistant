using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Identity;

public sealed class Tenant : Entity
{
    private Tenant() { }
    public Tenant(string name, string slug) { Name = name; Slug = slug; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public TenantStatus Status { get; private set; } = TenantStatus.Active;
    public void Suspend() { Status = TenantStatus.Suspended; UpdatedAt = DateTimeOffset.UtcNow; }
}
