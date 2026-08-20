using System.Text.Json;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppTemplateAdministrationService(ClinicAssistantDbContext dbContext, ITenantContext tenantContext, IOperationalEventPublisher events) : IWhatsAppTemplateAdministrationService
{
    public async Task<PagedResult<WhatsAppTemplateListItem>> SearchAsync(WhatsAppTemplateQuery query, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException();
        var page = Math.Max(1, query.Page); var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var templates = dbContext.WhatsAppTemplates.Where(item => item.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim(); templates = templates.Where(item => item.Name.Contains(search) || item.ContentSid.Contains(search)); }
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<WhatsAppTemplateStatus>(query.Status, true, out var status)) templates = templates.Where(item => item.Status == status);
        if (!string.IsNullOrWhiteSpace(query.LanguageCode)) templates = templates.Where(item => item.LanguageCode == query.LanguageCode);
        if (!string.IsNullOrWhiteSpace(query.Category)) templates = templates.Where(item => item.Category == query.Category);
        var total = await templates.CountAsync(cancellationToken);
        var items = await templates.OrderByDescending(item => item.UpdatedAt).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new WhatsAppTemplateListItem(item.Id, item.Name, item.LanguageCode, item.Category, item.Status.ToString(), MaskContentSid(item.ContentSid), item.UpdatedAt)).ToListAsync(cancellationToken);
        return new(items, page, pageSize, total);
    }

    public async Task<WhatsAppTemplateDetail?> GetAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException();
        var item = await dbContext.WhatsAppTemplates.SingleOrDefaultAsync(template => template.Id == templateId && template.TenantId == tenantId, cancellationToken);
        if (item is null) return null;
        return new(item.Id, item.Name, item.LanguageCode, item.Category, item.Status.ToString(), MaskContentSid(item.ContentSid), ReadVariables(item.ParametersSchema), item.CreatedAt, item.UpdatedAt);
    }
    public async Task<WhatsAppTemplateDetail> CreateAsync(WhatsAppTemplateRequest request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException(); var normalized = Normalize(request); if (string.IsNullOrWhiteSpace(normalized.ContentSid)) throw new ArgumentException("ContentSid is required.");
        var integration = await dbContext.WhatsAppIntegrations.Where(item => item.TenantId == tenantId).OrderByDescending(item => item.UpdatedAt).FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("WhatsApp integration not found.");
        if (await dbContext.WhatsAppTemplates.AnyAsync(item => item.IntegrationId == integration.Id && item.ContentSid == normalized.ContentSid, cancellationToken)) throw new InvalidOperationException("A template with this ContentSid already exists.");
        var template = new WhatsAppTemplate(tenantId, integration.Id, integration.Provider, normalized.ContentSid, normalized.Name, normalized.LanguageCode, SerializeVariables(normalized.Variables)); template.Update(normalized.Name, normalized.LanguageCode, normalized.Category, SerializeVariables(normalized.Variables));
        dbContext.AddRange(template, new AuditRecord(tenantId, tenantContext.UserId, "whatsapp.template.created", "WhatsAppTemplate", template.Id, "Succeeded", "Template created.")); await dbContext.SaveChangesAsync(cancellationToken); await events.PublishAsync(tenantId, "whatsapp.template.created", new { template.Id, Status = template.Status.ToString() }, cancellationToken); await PublishAuditAsync(tenantId, "whatsapp.template.created", "WhatsAppTemplate", template.Id, cancellationToken); return ToDetail(template);
    }
    public async Task<WhatsAppTemplateDetail?> UpdateAsync(Guid templateId, WhatsAppTemplateRequest request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException(); var template = await dbContext.WhatsAppTemplates.SingleOrDefaultAsync(item => item.Id == templateId && item.TenantId == tenantId, cancellationToken); if (template is null) return null; var normalized = Normalize(request);
        if (!string.IsNullOrWhiteSpace(normalized.ContentSid) && !string.Equals(template.ContentSid, normalized.ContentSid, StringComparison.Ordinal)) throw new InvalidOperationException("ContentSid cannot be changed after creation.");
        template.Update(normalized.Name, normalized.LanguageCode, normalized.Category, SerializeVariables(normalized.Variables)); dbContext.AuditRecords.Add(new AuditRecord(tenantId, tenantContext.UserId, "whatsapp.template.updated", "WhatsAppTemplate", template.Id, "Succeeded", "Template updated.")); await dbContext.SaveChangesAsync(cancellationToken); await events.PublishAsync(tenantId, "whatsapp.template.updated", new { template.Id, Status = template.Status.ToString() }, cancellationToken); await PublishAuditAsync(tenantId, "whatsapp.template.updated", "WhatsAppTemplate", template.Id, cancellationToken); return ToDetail(template);
    }
    public Task<bool> ActivateAsync(Guid templateId, CancellationToken cancellationToken) => ChangeStatusAsync(templateId, true, cancellationToken);
    public Task<bool> DeactivateAsync(Guid templateId, CancellationToken cancellationToken) => ChangeStatusAsync(templateId, false, cancellationToken);
    public async Task QueueSyncAsync(CancellationToken cancellationToken) { var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException(); var integration = await dbContext.WhatsAppIntegrations.Where(item => item.TenantId == tenantId).OrderByDescending(item => item.UpdatedAt).FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("WhatsApp integration not found."); var command = new SyncWhatsAppTemplatesCommand(tenantId, integration.Id, Guid.NewGuid().ToString("N")); dbContext.AddRange(new OutboxMessage(tenantId, nameof(SyncWhatsAppTemplatesCommand), JsonSerializer.Serialize(command)), new AuditRecord(tenantId, tenantContext.UserId, "whatsapp.template.sync_requested", "WhatsAppIntegration", integration.Id, "Succeeded", "Template synchronization queued.")); await dbContext.SaveChangesAsync(cancellationToken); await PublishAuditAsync(tenantId, "whatsapp.template.sync_requested", "WhatsAppIntegration", integration.Id, cancellationToken); }
    private async Task<bool> ChangeStatusAsync(Guid templateId, bool active, CancellationToken ct) { var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException(); var template = await dbContext.WhatsAppTemplates.SingleOrDefaultAsync(item => item.Id == templateId && item.TenantId == tenantId, ct); if (template is null) return false; var action = active ? "whatsapp.template.activated" : "whatsapp.template.deactivated"; if (active) template.Activate(); else template.Deactivate(); dbContext.AuditRecords.Add(new AuditRecord(tenantId, tenantContext.UserId, action, "WhatsAppTemplate", template.Id, "Succeeded", "Template status changed.")); await dbContext.SaveChangesAsync(ct); await events.PublishAsync(tenantId, action, new { template.Id, Status = template.Status.ToString() }, ct); await PublishAuditAsync(tenantId, action, "WhatsAppTemplate", template.Id, ct); return true; }
    private static WhatsAppTemplateRequest Normalize(WhatsAppTemplateRequest request) { if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.LanguageCode)) throw new ArgumentException("Name and language are required."); var variables = request.Variables?.Select(item => item.Trim()).Where(item => item.Length > 0).Distinct(StringComparer.Ordinal).ToArray() ?? []; if (variables.Any(item => item.Length > 80)) throw new ArgumentException("Template variables are invalid."); return request with { ContentSid = string.IsNullOrWhiteSpace(request.ContentSid) ? null : request.ContentSid.Trim(), Name = request.Name.Trim(), LanguageCode = request.LanguageCode.Trim(), Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(), Variables = variables }; }
    private static string? SerializeVariables(IReadOnlyList<string>? variables) => variables is { Count: > 0 } ? JsonSerializer.Serialize(variables) : null;
    private static WhatsAppTemplateDetail ToDetail(WhatsAppTemplate template) => new(template.Id, template.Name, template.LanguageCode, template.Category, template.Status.ToString(), MaskContentSid(template.ContentSid), ReadVariables(template.ParametersSchema), template.CreatedAt, template.UpdatedAt);

    private static string[] ReadVariables(string? schema) { try { return string.IsNullOrWhiteSpace(schema) ? [] : JsonSerializer.Deserialize<string[]>(schema) ?? []; } catch (JsonException) { return []; } }
    private static string MaskContentSid(string contentSid) => contentSid.Length <= 6 ? "••••••" : $"{contentSid[..2]}••••••{contentSid[^4..]}";
    private Task PublishAuditAsync(Guid tenantId, string action, string resourceType, Guid resourceId, CancellationToken ct) => events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = resourceType, ResourceId = resourceId, Result = "Succeeded" }, ct);
}

public sealed class WhatsAppTemplateSyncProcessor(ClinicAssistantDbContext dbContext, ITwilioTemplateClient client, IOperationalEventPublisher events) : IWhatsAppTemplateSyncProcessor
{
    public async Task ProcessAsync(SyncWhatsAppTemplatesCommand command, CancellationToken cancellationToken)
    {
        var integration = await dbContext.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == command.IntegrationId && item.TenantId == command.TenantId && item.Provider == WhatsAppProvider.Twilio, cancellationToken) ?? throw new InvalidOperationException("Twilio integration not found.");
        var remoteTemplates = await client.ListAsync(cancellationToken); var existing = await dbContext.WhatsAppTemplates.IgnoreQueryFilters().Where(item => item.TenantId == command.TenantId && item.IntegrationId == command.IntegrationId).ToDictionaryAsync(item => item.ContentSid, cancellationToken);
        var changed = 0;
        foreach (var remote in remoteTemplates)
        {
            var schema = remote.Variables.Count == 0 ? null : JsonSerializer.Serialize(remote.Variables);
            if (existing.TryGetValue(remote.ContentSid, out var local)) { local.Update(remote.Name, remote.LanguageCode, null, schema); changed++; }
            else { dbContext.WhatsAppTemplates.Add(new WhatsAppTemplate(command.TenantId, integration.Id, WhatsAppProvider.Twilio, remote.ContentSid, remote.Name, remote.LanguageCode, schema)); changed++; }
        }
        dbContext.AuditRecords.Add(new AuditRecord(command.TenantId, null, "whatsapp.template.synced", "WhatsAppIntegration", integration.Id, "Succeeded", $"{changed} template(s) synchronized.")); await dbContext.SaveChangesAsync(cancellationToken); WhatsAppTelemetry.TemplateSynchronizations.Add(1); WhatsAppTelemetry.TemplatesSynchronized.Add(changed); await events.PublishAsync(command.TenantId, "whatsapp.template.synced", new { integration.Id, Changed = changed }, cancellationToken); await events.PublishAsync(command.TenantId, "audit.created", new { Action = "whatsapp.template.synced", ResourceType = "WhatsAppIntegration", ResourceId = integration.Id, Result = "Succeeded" }, cancellationToken);
    }
}
