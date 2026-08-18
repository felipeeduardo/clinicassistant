using ClinicAssistant.Contracts.Leads;

namespace ClinicAssistant.Application.Leads;

public interface IDemoLeadService
{
    Task<bool> CreateAsync(CreateDemoLeadRequest request, CancellationToken cancellationToken);
    Task<DemoLeadPage> SearchAsync(DemoLeadListQuery query, CancellationToken cancellationToken);
    Task<DemoLeadDetail?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<DemoLeadSummary> GetSummaryAsync(CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken);
    Task AssignAsync(Guid id, Guid? userId, CancellationToken cancellationToken);
    Task AddNoteAsync(Guid id, string note, CancellationToken cancellationToken);
}
