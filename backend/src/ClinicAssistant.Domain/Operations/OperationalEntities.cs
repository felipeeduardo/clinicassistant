using ClinicAssistant.Domain.Primitives;

namespace ClinicAssistant.Domain.Operations;

public sealed class IdempotencyRecord : Entity
{
    private IdempotencyRecord() { }
    public IdempotencyRecord(string scope, string key, string responseJson) { Scope = scope; Key = key; ResponseJson = responseJson; }
    public string Scope { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string ResponseJson { get; private set; } = null!;
}

public sealed class AuditRecord : Entity
{
    private AuditRecord() { }
    public AuditRecord(Guid? tenantId, Guid? actorUserId, string action, string resourceType, Guid? resourceId, string result, string details)
    { TenantId = tenantId; ActorUserId = actorUserId; Action = action; ResourceType = resourceType; ResourceId = resourceId; Result = result; Details = details; }
    public Guid? TenantId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string ResourceType { get; private set; } = null!;
    public Guid? ResourceId { get; private set; }
    public string Result { get; private set; } = null!;
    public string Details { get; private set; } = null!;
}
