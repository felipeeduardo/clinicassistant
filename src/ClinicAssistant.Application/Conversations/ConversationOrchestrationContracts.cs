using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Application.Conversations;

public interface IConversationOrchestrator
{
    Task<ConversationOrchestrationResult> ProcessAsync(ProcessConversationMessageCommand command, CancellationToken cancellationToken);
}

public interface IConversationLockManager
{
    Task<IConversationLockHandle?> TryAcquireAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken);
}

public interface IConversationLockHandle : IAsyncDisposable { }

public sealed record ProcessConversationMessageCommand(
    Guid TenantId,
    Guid IntegrationId,
    Guid ConversationId,
    Guid ConversationMessageId,
    string CorrelationId);

public enum ConversationOrchestrationResult { Processed, Duplicate, LockUnavailable, Rejected, ConcurrencyConflict }
