using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.WhatsApp;

namespace ClinicAssistant.Application.Conversations;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
public sealed record ConversationListQuery(int Page = 1, int PageSize = 25, ConversationStatus? Status = null, ConversationAutomationMode? AutomationMode = null, ConversationIntent? Intent = null, string? Search = null);
public sealed record ConversationListItem(Guid Id, Guid PatientId, string PatientName, string MaskedPhone, ConversationStatus Status, ConversationAutomationMode AutomationMode, ConversationPriority Priority, Guid? AssignedUserId, DateTimeOffset? LastMessageAt, int Version);
public sealed record ConversationDetail(Guid Id, Guid PatientId, string PatientName, ConversationStatus Status, ConversationAutomationMode AutomationMode, ConversationPriority Priority, Guid? AssignedUserId, int Version, ConversationStateSummary? State, IReadOnlyCollection<ConversationMessageItem> RecentMessages);
public sealed record ConversationStateSummary(ConversationFlowState FlowState, ConversationIntent Intent, ConversationStateStatus Status, int InvalidAttempts, DateTimeOffset ExpiresAt, int Version);
public sealed record ConversationMessageItem(Guid Id, ConversationMessageDirection Direction, ConversationMessageType Type, string? ContentSanitized, ConversationMessageStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt, string? Failure);
public sealed record ConversationOperationRequest(int ExpectedVersion, string? Reason = null);
public sealed record ConversationTransferRequest(int ExpectedVersion, Guid TargetUserId, string? Reason = null);
public sealed record HumanQueueListQuery(int Page = 1, int PageSize = 25, HumanQueueItemStatus? Status = null);
public sealed record HumanQueueListItem(Guid ConversationId, string PatientName, HumanQueueItemStatus Status, ConversationPriority Priority, Guid? AssignedUserId, string? Reason, DateTimeOffset CreatedAt, int Version);
public sealed record ConversationPriorityRequest(int ExpectedVersion, ConversationPriority Priority);
public sealed record ManualConversationMessageRequest(int ExpectedVersion, string Content);
public sealed record ConversationAppointmentItem(Guid Id, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status, string SpecialtyName, string ProfessionalName);
public sealed record AssignableUserItem(Guid Id, string Name, string Role);
public interface IConversationAdministrationService
{
    Task<PagedResult<ConversationListItem>> ListAsync(ConversationListQuery query, CancellationToken cancellationToken);
    Task<ConversationDetail?> GetAsync(Guid conversationId, CancellationToken cancellationToken);
    Task<PagedResult<ConversationMessageItem>?> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken);
    Task MarkReadAsync(Guid conversationId, Guid messageId, int expectedVersion, CancellationToken cancellationToken);
    Task AssignAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task ReleaseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task PauseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task ResumeAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task TransferAsync(Guid conversationId, ConversationTransferRequest request, CancellationToken cancellationToken);
    Task<PagedResult<HumanQueueListItem>> GetHumanQueueAsync(HumanQueueListQuery query, CancellationToken cancellationToken);
    Task CloseAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task ReopenAsync(Guid conversationId, ConversationOperationRequest request, CancellationToken cancellationToken);
    Task SetPriorityAsync(Guid conversationId, ConversationPriorityRequest request, CancellationToken cancellationToken);
    Task SendManualMessageAsync(Guid conversationId, ManualConversationMessageRequest request, string idempotencyKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationAppointmentItem>> GetAppointmentsAsync(Guid conversationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssignableUserItem>> GetAssignableUsersAsync(CancellationToken cancellationToken);
}
