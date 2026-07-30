using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClinicAssistant.Infrastructure.Conversations;

public static class ConversationTelemetry
{
    private static readonly Meter Meter = new("ClinicAssistant.Conversations");
    public static readonly ActivitySource ActivitySource = new("ClinicAssistant.Conversations");
    public static readonly Counter<long> Processed = Meter.CreateCounter<long>("conversation_messages_processed_total");
    public static readonly Counter<long> Duplicates = Meter.CreateCounter<long>("conversation_messages_duplicate_total");
    public static readonly Counter<long> LockUnavailable = Meter.CreateCounter<long>("conversation_lock_unavailable_total");
    public static readonly Counter<long> ConcurrencyConflicts = Meter.CreateCounter<long>("conversation_concurrency_conflict_total");
}
