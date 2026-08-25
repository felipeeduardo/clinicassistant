namespace ClinicAssistant.Contracts.Leads;

public sealed record CreateDemoLeadRequest(
    string FullName,
    string CompanyOrClinicName,
    string Email,
    string Phone,
    string? Description = null,
    string? Website = null,
    string? UtmSource = null,
    string? UtmMedium = null,
    string? UtmCampaign = null,
    string? UtmContent = null,
    string? UtmTerm = null,
    string? LandingPage = null,
    string? Referrer = null);

public sealed record DemoLeadListQuery(
    int Page = 1,
    int PageSize = 25,
    string? Status = null,
    string? Search = null,
    Guid? AssignedToUserId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? UtmSource = null);

public sealed record DemoLeadListItem(
    Guid Id,
    string FullName,
    string CompanyOrClinicName,
    string Email,
    string Phone,
    string Status,
    string Source,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastContactAt,
    string? UtmSource = null,
    string? UtmMedium = null,
    string? UtmCampaign = null,
    string? UtmContent = null,
    string? UtmTerm = null,
    string? LandingPage = null,
    string? Referrer = null);

public sealed record DemoLeadNote(DateTimeOffset CreatedAt, Guid? ActorUserId, string? ActorName, string Note);

public sealed record DemoLeadDetail(
    DemoLeadListItem Lead,
    string? Description,
    IReadOnlyList<DemoLeadNote> Notes,
    IReadOnlyList<DemoLeadNote> History);

public sealed record DemoLeadPage(IReadOnlyList<DemoLeadListItem> Items, int Page, int PageSize, int TotalCount);
public sealed record DemoLeadSummary(int New, int Contacted, int Qualified, int DemoScheduled, int Won, int Lost, int Archived, int Total);
public sealed record UpdateDemoLeadStatusRequest(string Status);
public sealed record AssignDemoLeadRequest(Guid? UserId);
public sealed record AddDemoLeadNoteRequest(string Note);
