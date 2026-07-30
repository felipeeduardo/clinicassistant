using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationAdministrationService(ClinicAssistantDbContext dbContext, TenantAccessGuard accessGuard, IPhoneMasker phoneMasker, ITenantContext tenantContext) : IConversationAdministrationService
{
    public async Task<PagedResult<ConversationListItem>> ListAsync(ConversationListQuery query, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); var page = Math.Max(1, query.Page); var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var source = dbContext.Conversations.Where(item => item.TenantId == tenantId).Join(dbContext.Patients, conversation => conversation.PatientId, patient => patient.Id, (conversation, patient) => new { conversation, patient });
        if (query.Status.HasValue) source = source.Where(item => item.conversation.Status == query.Status.Value);
        if (query.AutomationMode.HasValue) source = source.Where(item => item.conversation.AutomationMode == query.AutomationMode.Value);
        if (!string.IsNullOrWhiteSpace(query.Search)) { var term = query.Search.Trim(); source = source.Where(item => item.patient.Name.Contains(term) || item.patient.Phone.Contains(term)); }
        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderByDescending(item => item.conversation.LastMessageAt).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new ConversationListItem(item.conversation.Id, item.patient.Id, item.patient.Name, item.patient.Phone, item.conversation.Status, item.conversation.AutomationMode, item.conversation.Priority, item.conversation.AssignedUserId, item.conversation.LastMessageAt, item.conversation.Version)).ToListAsync(cancellationToken);
        return new(items.Select(item => item with { MaskedPhone = phoneMasker.Mask(item.MaskedPhone) }).ToArray(), page, pageSize, total);
    }

    public async Task<ConversationDetail?> GetAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId();
        var data = await dbContext.Conversations.Where(item => item.Id == conversationId && item.TenantId == tenantId).Join(dbContext.Patients, conversation => conversation.PatientId, patient => patient.Id, (conversation, patient) => new { conversation, patient }).SingleOrDefaultAsync(cancellationToken);
        if (data is null) return null;
        var state = await dbContext.ConversationStates.SingleOrDefaultAsync(item => item.ConversationId == conversationId && item.TenantId == tenantId, cancellationToken);
        var messages = await MessagesQuery(conversationId, tenantId).OrderByDescending(item => item.CreatedAt).Take(20).ToListAsync(cancellationToken);
        return new(data.conversation.Id, data.patient.Id, data.patient.Name, data.conversation.Status, data.conversation.AutomationMode, data.conversation.Priority, data.conversation.AssignedUserId, data.conversation.Version, state is null ? null : new(state.FlowState, state.Intent, state.Status, state.InvalidAttempts, state.ExpiresAt, state.Version), messages);
    }

    public async Task<PagedResult<ConversationMessageItem>?> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); if (!await dbContext.Conversations.AnyAsync(item => item.Id == conversationId && item.TenantId == tenantId, cancellationToken)) return null;
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100); var source = MessagesQuery(conversationId, tenantId); var total = await source.CountAsync(cancellationToken); var items = await source.OrderByDescending(item => item.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken); return new(items, page, pageSize, total);
    }

    public Task MarkReadAsync(Guid conversationId, Guid messageId, int expectedVersion, CancellationToken cancellationToken) => MutateAsync(conversationId, expectedVersion, conversation => { }, async () => { var message = await dbContext.ConversationMessages.SingleOrDefaultAsync(item => item.Id == messageId && item.ConversationId == conversationId && item.TenantId == accessGuard.RequireTenantId(), cancellationToken) ?? throw new KeyNotFoundException(); message.MarkReadByOperator(); }, cancellationToken);
    public Task AssignAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) { var userId = tenantContext.UserId ?? throw new UnauthorizedAccessException(); return MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Assign(userId), async () => { var item = await QueueItemAsync(conversationId, cancellationToken); item.Assign(userId); }, cancellationToken); }
    public Task ReleaseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) => MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Release(), async () => { var item = await QueueItemAsync(conversationId, cancellationToken); item.Release(); }, cancellationToken);
    public Task PauseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) => MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.PauseAutomation(), null, cancellationToken);
    public Task ResumeAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) => MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.ResumeAutomation(), null, cancellationToken);

    private async Task MutateAsync(Guid conversationId, int expectedVersion, Action<Conversation> mutation, Func<Task>? nestedMutation, CancellationToken cancellationToken)
    { var tenantId = accessGuard.RequireTenantId(); var conversation = await dbContext.Conversations.SingleOrDefaultAsync(item => item.Id == conversationId && item.TenantId == tenantId, cancellationToken) ?? throw new KeyNotFoundException(); if (conversation.Version != expectedVersion) throw new DbUpdateConcurrencyException(); mutation(conversation); if (nestedMutation is not null) await nestedMutation(); await dbContext.SaveChangesAsync(cancellationToken); }
    private IQueryable<ConversationMessageItem> MessagesQuery(Guid conversationId, Guid tenantId) => dbContext.ConversationMessages.Where(item => item.ConversationId == conversationId && item.TenantId == tenantId).Select(item => new ConversationMessageItem(item.Id, item.Direction, item.Type, item.ContentSanitized, item.Status, item.CreatedAt, item.ReadAt, item.ProviderErrorMessage));
    private async Task<HumanQueueItem> QueueItemAsync(Guid conversationId, CancellationToken cancellationToken) => await dbContext.HumanQueueItems.SingleOrDefaultAsync(item => item.ConversationId == conversationId && item.TenantId == accessGuard.RequireTenantId(), cancellationToken) ?? throw new InvalidOperationException("The conversation is not in the human queue.");
}
