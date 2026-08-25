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

    public DemoLead(string fullName, string companyOrClinicName, string email, string phone, string? description, string source, string? utmSource = null, string? utmMedium = null, string? utmCampaign = null, string? utmContent = null, string? utmTerm = null, string? landingPage = null, string? referrer = null)
    {
        FullName = fullName;
        CompanyOrClinicName = companyOrClinicName;
        Email = email;
        Phone = phone;
        Description = description;
        Source = source;
        UtmSource = utmSource;
        UtmMedium = utmMedium;
        UtmCampaign = utmCampaign;
        UtmContent = utmContent;
        UtmTerm = utmTerm;
        LandingPage = landingPage;
        Referrer = referrer;
    }

    public string FullName { get; private set; } = null!;
    public string CompanyOrClinicName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Source { get; private set; } = null!;
    public string? UtmSource { get; private set; }
    public string? UtmMedium { get; private set; }
    public string? UtmCampaign { get; private set; }
    public string? UtmContent { get; private set; }
    public string? UtmTerm { get; private set; }
    public string? LandingPage { get; private set; }
    public string? Referrer { get; private set; }
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
