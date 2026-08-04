using ClinicAssistant.Application.Conversations;

namespace ClinicAssistant.Application.WhatsApp;

public sealed record WhatsAppTemplateQuery(int Page = 1, int PageSize = 25, string? Search = null, string? Status = null, string? LanguageCode = null, string? Category = null, string? Provider = null);
public sealed record WhatsAppTemplateListItem(Guid Id, string Name, string Provider, string LanguageCode, string? Category, string Status, string ContentSidMasked, DateTimeOffset UpdatedAt);
public sealed record WhatsAppTemplateDetail(Guid Id, string Name, string Provider, string LanguageCode, string? Category, string Status, string ContentSidMasked, IReadOnlyList<string> Variables, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record WhatsAppTemplateRequest(string? ContentSid, string Name, string LanguageCode, string? Category, IReadOnlyList<string>? Variables);

public interface IWhatsAppTemplateAdministrationService
{
    Task<PagedResult<WhatsAppTemplateListItem>> SearchAsync(WhatsAppTemplateQuery query, CancellationToken cancellationToken);
    Task<WhatsAppTemplateDetail?> GetAsync(Guid templateId, CancellationToken cancellationToken);
    Task<WhatsAppTemplateDetail> CreateAsync(WhatsAppTemplateRequest request, CancellationToken cancellationToken);
    Task<WhatsAppTemplateDetail?> UpdateAsync(Guid templateId, WhatsAppTemplateRequest request, CancellationToken cancellationToken);
    Task<bool> ActivateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(Guid templateId, CancellationToken cancellationToken);
    Task QueueSyncAsync(CancellationToken cancellationToken);
}

public sealed record SyncWhatsAppTemplatesCommand(Guid TenantId, Guid IntegrationId, string CorrelationId);
public interface IWhatsAppTemplateSyncProcessor { Task ProcessAsync(SyncWhatsAppTemplatesCommand command, CancellationToken cancellationToken); }
