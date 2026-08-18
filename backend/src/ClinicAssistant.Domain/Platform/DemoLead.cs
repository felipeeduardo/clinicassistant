using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Platform;

public enum DemoLeadStatus
{
    New,
    Contacted,
    Qualified,
    DemoScheduled,
    Won,
    Lost,
    Archived
}

public sealed class DemoLead : Entity
{
    private DemoLead() { }

    public DemoLead(string fullName, string companyOrClinicName, string email, string phone, string? description, string source)
    {
        FullName = fullName;
        CompanyOrClinicName = companyOrClinicName;
        Email = email;
        Phone = phone;
        Description = description;
        Source = source;
    }

    public string FullName { get; private set; } = null!;
    public string CompanyOrClinicName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Source { get; private set; } = null!;
    public DemoLeadStatus Status { get; private set; } = DemoLeadStatus.New;
    public Guid? AssignedToUserId { get; private set; }
    public DateTimeOffset? LastContactAt { get; private set; }
    public int Version { get; private set; } = 1;

    public void ChangeStatus(DemoLeadStatus status)
    {
        Status = status;
        if (status == DemoLeadStatus.Contacted) LastContactAt = DateTimeOffset.UtcNow;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Assign(Guid? userId)
    {
        AssignedToUserId = userId;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
