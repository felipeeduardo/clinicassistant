using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Domain.Messaging;
using System.Text.Json;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Infrastructure.WhatsApp;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class ConversationAdministrationService(ClinicAssistantDbContext dbContext, TenantAccessGuard accessGuard, IPhoneMasker phoneMasker, ITenantContext tenantContext, IOperationalEventPublisher events) : IConversationAdministrationService
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
        var messages = await MessagesQuery(conversationId, tenantId).OrderByDescending(item => item.CreatedAt).Take(20).Select(item => new ConversationMessageItem(item.Id, item.Direction, item.Type, item.ContentSanitized, item.Status, item.CreatedAt, item.ReadAt, item.ProviderErrorMessage)).ToListAsync(cancellationToken);
        return new(data.conversation.Id, data.patient.Id, data.patient.Name, data.conversation.Status, data.conversation.AutomationMode, data.conversation.Priority, data.conversation.AssignedUserId, data.conversation.Version, state is null ? null : new(state.FlowState, state.Intent, state.Status, state.InvalidAttempts, state.ExpiresAt, state.Version), messages);
    }

    public async Task<PagedResult<ConversationMessageItem>?> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); if (!await dbContext.Conversations.AnyAsync(item => item.Id == conversationId && item.TenantId == tenantId, cancellationToken)) return null;
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100); var source = MessagesQuery(conversationId, tenantId); var total = await source.CountAsync(cancellationToken); var items = await source.OrderByDescending(item => item.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new ConversationMessageItem(item.Id, item.Direction, item.Type, item.ContentSanitized, item.Status, item.CreatedAt, item.ReadAt, item.ProviderErrorMessage)).ToListAsync(cancellationToken); return new(items, page, pageSize, total);
    }

    public Task MarkReadAsync(Guid conversationId, Guid messageId, int expectedVersion, CancellationToken cancellationToken) => MutateAsync(conversationId, expectedVersion, conversation => { }, async () => { var message = await dbContext.ConversationMessages.SingleOrDefaultAsync(item => item.Id == messageId && item.ConversationId == conversationId && item.TenantId == accessGuard.RequireTenantId(), cancellationToken) ?? throw new KeyNotFoundException(); message.MarkReadByOperator(); }, cancellationToken);
    public async Task AssignAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) { var userId = tenantContext.UserId ?? throw new UnauthorizedAccessException(); await MutateAsync(conversationId, request.ExpectedVersion, conversation => { if (conversation.Status != ConversationStatus.WaitingHuman || conversation.AssignedUserId.HasValue) throw new InvalidOperationException("This conversation is no longer waiting for an operator."); conversation.Assign(userId); }, async () => { var item = await QueueItemAsync(conversationId, cancellationToken); if (item.Status != HumanQueueItemStatus.Waiting) throw new InvalidOperationException("This conversation is no longer waiting in the human queue."); item.Assign(userId); }, cancellationToken); ConversationTelemetry.QueueAssigned.Add(1); await events.PublishAsync(accessGuard.RequireTenantId(), "queue.item.assigned", new { ConversationId = conversationId, UserId = userId }, cancellationToken); }
    public async Task ReleaseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) { await MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Release(), async () => { var item = await QueueItemAsync(conversationId, cancellationToken); item.Release(); }, cancellationToken); ConversationTelemetry.QueueReleased.Add(1); await events.PublishAsync(accessGuard.RequireTenantId(), "queue.item.released", new { ConversationId = conversationId }, cancellationToken); }
    public Task PauseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) => MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.PauseAutomation(), null, cancellationToken);
    public Task ResumeAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) => MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.ResumeAutomation(), null, cancellationToken);
    public async Task TransferAsync(Guid conversationId, ConversationTransferRequest request, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); if (!await dbContext.Users.AnyAsync(user => user.Id == request.TargetUserId && user.TenantId == tenantId && user.Status == UserStatus.Active, cancellationToken)) throw new InvalidOperationException("Target user is not active in this tenant.");
        await MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Assign(request.TargetUserId), async () => { var item = await QueueItemAsync(conversationId, cancellationToken); item.Assign(request.TargetUserId); }, cancellationToken);
        dbContext.AuditRecords.Add(new AuditRecord(tenantId, tenantContext.UserId, "conversation.transferred", "Conversation", conversationId, "Succeeded", request.Reason ?? "Conversation transferred.")); await dbContext.SaveChangesAsync(cancellationToken);
        ConversationTelemetry.QueueTransferred.Add(1);
        await events.PublishAsync(tenantId, "queue.item.transferred", new { ConversationId = conversationId, UserId = request.TargetUserId }, cancellationToken);
    }
    public async Task<PagedResult<HumanQueueListItem>> GetHumanQueueAsync(HumanQueueListQuery query, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); var page = Math.Max(1, query.Page); var pageSize = Math.Clamp(query.PageSize, 1, 100); var source = from item in dbContext.HumanQueueItems where item.TenantId == tenantId join conversation in dbContext.Conversations on item.ConversationId equals conversation.Id join patient in dbContext.Patients on conversation.PatientId equals patient.Id select new { item, patient.Name };
        if (query.Status.HasValue) source = source.Where(item => item.item.Status == query.Status.Value); var total = await source.CountAsync(cancellationToken); var items = await source.OrderByDescending(item => item.item.Priority).ThenBy(item => item.item.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new HumanQueueListItem(item.item.ConversationId, item.Name, item.item.Status, item.item.Priority, item.item.AssignedUserId, item.item.Reason, item.item.CreatedAt, item.item.Version)).ToListAsync(cancellationToken); return new(items, page, pageSize, total);
    }
    public async Task CloseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) { await MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Close(), async () => { var queue = await dbContext.HumanQueueItems.SingleOrDefaultAsync(item => item.ConversationId == conversationId && item.TenantId == accessGuard.RequireTenantId(), cancellationToken); queue?.Complete(); }, cancellationToken); await AuditAsync(conversationId, "conversation.closed", request.Reason ?? "Conversation closed.", cancellationToken); ConversationTelemetry.QueueCompleted.Add(1); await events.PublishAsync(accessGuard.RequireTenantId(), "queue.item.completed", new { ConversationId = conversationId }, cancellationToken); }
    public async Task ReopenAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken) { await MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.Reopen(), null, cancellationToken); await AuditAsync(conversationId, "conversation.reopened", request.Reason ?? "Conversation reopened.", cancellationToken); }
    public async Task SetPriorityAsync(Guid conversationId, ConversationPriorityRequest request, CancellationToken cancellationToken) { await MutateAsync(conversationId, request.ExpectedVersion, conversation => conversation.SetPriority(request.Priority), null, cancellationToken); await AuditAsync(conversationId, "conversation.priority_changed", $"Priority set to {request.Priority}.", cancellationToken); }
    public async Task SendManualMessageAsync(Guid conversationId, ManualConversationMessageRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || string.IsNullOrWhiteSpace(request.Content)) throw new InvalidOperationException("Message content and Idempotency-Key are required."); var tenantId = accessGuard.RequireTenantId(); var scope = $"conversation.manual-message:{conversationId}"; if (await dbContext.IdempotencyRecords.AnyAsync(item => item.Scope == scope && item.Key == idempotencyKey, cancellationToken)) return;
        var conversation = await dbContext.Conversations.SingleOrDefaultAsync(item => item.Id == conversationId && item.TenantId == tenantId, cancellationToken) ?? throw new KeyNotFoundException(); if (conversation.Version != request.ExpectedVersion) throw new DbUpdateConcurrencyException(); if (conversation.Status == ConversationStatus.Closed) throw new InvalidOperationException("A closed conversation cannot receive a manual message."); if (conversation.AutomationMode != ConversationAutomationMode.Human || conversation.AssignedUserId != tenantContext.UserId) throw new InvalidOperationException("Only the assigned human operator can send a manual message.");
        var patient = await dbContext.Patients.SingleAsync(item => item.Id == conversation.PatientId && item.TenantId == tenantId, cancellationToken); var integration = await dbContext.WhatsAppIntegrations.SingleAsync(item => item.Id == conversation.IntegrationId && item.TenantId == tenantId, cancellationToken); var message = new ConversationMessage(tenantId, conversationId, ConversationMessageType.Text, request.Content.Trim(), integration.Provider); var command = new SendWhatsAppMessageCommand(tenantId, integration.Id, conversationId, message.Id, WhatsAppOutgoingMessageType.Text, patient.Phone, message.Content, null, null, null, $"manual:{message.Id:N}", idempotencyKey);
        dbContext.AddRange(message, new OutboxMessage(tenantId, nameof(SendWhatsAppMessageCommand), JsonSerializer.Serialize(command)), new IdempotencyRecord(scope, idempotencyKey, "{}"), new AuditRecord(tenantId, tenantContext.UserId, "conversation.manual_message", "Conversation", conversationId, "Succeeded", "Manual message queued.")); await dbContext.SaveChangesAsync(cancellationToken); OperationalTelemetry.ManualMessages.Add(1); await events.PublishAsync(tenantId, "conversation.updated", new { conversation.Id, conversation.Version }, cancellationToken);
    }
    public async Task<IReadOnlyList<ConversationAppointmentItem>> GetAppointmentsAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var tenantId = accessGuard.RequireTenantId(); var patientId = await dbContext.Conversations.Where(item => item.Id == conversationId && item.TenantId == tenantId).Select(item => (Guid?)item.PatientId).SingleOrDefaultAsync(cancellationToken) ?? throw new KeyNotFoundException();
        return await (from appointment in dbContext.Appointments where appointment.PatientId == patientId join specialty in dbContext.Specialties on appointment.SpecialtyId equals specialty.Id join professional in dbContext.Professionals on appointment.ProfessionalId equals professional.Id orderby appointment.StartsAt descending select new ConversationAppointmentItem(appointment.Id, appointment.StartsAt, appointment.EndsAt, appointment.Status.ToString(), specialty.Name, professional.Name)).Take(30).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<AssignableUserItem>> GetAssignableUsersAsync(CancellationToken cancellationToken) { var tenantId = accessGuard.RequireTenantId(); return await dbContext.Users.Where(user => user.TenantId == tenantId && user.Status == UserStatus.Active && (user.Role == UserRole.ClinicAdmin || user.Role == UserRole.Receptionist)).OrderBy(user => user.Name).Select(user => new AssignableUserItem(user.Id, user.Name, user.Role.ToString())).ToListAsync(cancellationToken); }

    private async Task MutateAsync(Guid conversationId, int expectedVersion, Action<Conversation> mutation, Func<Task>? nestedMutation, CancellationToken cancellationToken)
    { var tenantId = accessGuard.RequireTenantId(); var conversation = await dbContext.Conversations.SingleOrDefaultAsync(item => item.Id == conversationId && item.TenantId == tenantId, cancellationToken) ?? throw new KeyNotFoundException(); if (conversation.Version != expectedVersion) throw new DbUpdateConcurrencyException(); mutation(conversation); if (nestedMutation is not null) await nestedMutation(); await dbContext.SaveChangesAsync(cancellationToken); await events.PublishAsync(tenantId, "conversation.updated", new { conversation.Id, conversation.Version }, cancellationToken); await events.PublishAsync(tenantId, "dashboard.invalidated", new { }, cancellationToken); }
    private IQueryable<ConversationMessage> MessagesQuery(Guid conversationId, Guid tenantId) => dbContext.ConversationMessages.Where(item => item.ConversationId == conversationId && item.TenantId == tenantId);
    private async Task<HumanQueueItem> QueueItemAsync(Guid conversationId, CancellationToken cancellationToken) => await dbContext.HumanQueueItems.SingleOrDefaultAsync(item => item.ConversationId == conversationId && item.TenantId == accessGuard.RequireTenantId(), cancellationToken) ?? throw new InvalidOperationException("The conversation is not in the human queue.");
    private async Task AuditAsync(Guid conversationId, string action, string details, CancellationToken cancellationToken) { var tenantId = accessGuard.RequireTenantId(); dbContext.AuditRecords.Add(new AuditRecord(tenantId, tenantContext.UserId, action, "Conversation", conversationId, "Succeeded", details)); await dbContext.SaveChangesAsync(cancellationToken); await events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = "Conversation", ResourceId = conversationId, Result = "Succeeded" }, cancellationToken); }
}
