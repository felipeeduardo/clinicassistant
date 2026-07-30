using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Conversations;

public enum HumanQueueItemStatus { Waiting, Assigned, Completed }

public sealed class HumanQueueItem : Entity, ITenantEntity
{
    private HumanQueueItem() { }
    public HumanQueueItem(Guid tenantId, Guid conversationId, ConversationPriority priority, string? reason)
    { TenantId = tenantId; ConversationId = conversationId; Priority = priority; Reason = reason; Status = HumanQueueItemStatus.Waiting; Version = 1; }
    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public HumanQueueItemStatus Status { get; private set; }
    public ConversationPriority Priority { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public string? Reason { get; private set; }
    public int Version { get; private set; }
    public void Assign(Guid userId) { AssignedUserId = userId; Status = HumanQueueItemStatus.Assigned; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Release() { AssignedUserId = null; Status = HumanQueueItemStatus.Waiting; Version++; UpdatedAt = DateTimeOffset.UtcNow; }
}
