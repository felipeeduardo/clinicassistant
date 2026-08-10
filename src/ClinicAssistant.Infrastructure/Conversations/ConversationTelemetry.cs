using System.Diagnostics;
using System.Diagnostics.Metrics;
using ClinicAssistant.Domain.Conversations;

namespace ClinicAssistant.Infrastructure.Conversations;

public static class ConversationTelemetry
{
    private static readonly Meter Meter = new("ClinicAssistant.Conversations");
    public static readonly ActivitySource ActivitySource = new("ClinicAssistant.Conversations");
    public static readonly Counter<long> Processed = Meter.CreateCounter<long>("conversation_messages_processed_total");
    public static readonly Counter<long> Duplicates = Meter.CreateCounter<long>("conversation_messages_duplicate_total");
    public static readonly Counter<long> LockUnavailable = Meter.CreateCounter<long>("conversation_lock_unavailable_total");
    public static readonly Counter<long> ConcurrencyConflicts = Meter.CreateCounter<long>("conversation_concurrency_conflict_total");
    public static readonly Counter<long> QueueAssigned = Meter.CreateCounter<long>("human_queue_assigned_total");
    public static readonly Counter<long> QueueReleased = Meter.CreateCounter<long>("human_queue_released_total");
    public static readonly Counter<long> QueueTransferred = Meter.CreateCounter<long>("human_queue_transferred_total");
    public static readonly Counter<long> QueueCompleted = Meter.CreateCounter<long>("human_queue_completed_total");
    public static readonly Counter<long> IntentDetected = Meter.CreateCounter<long>("conversation_intent_total");
    public static readonly Counter<long> UnknownIntent = Meter.CreateCounter<long>("conversation_unknown_intent_total");
    public static readonly Counter<long> InvalidInput = Meter.CreateCounter<long>("conversation_invalid_input_total");
    public static readonly Counter<long> FlowStarted = Meter.CreateCounter<long>("conversation_flow_started_total");
    public static readonly Counter<long> FlowCompleted = Meter.CreateCounter<long>("conversation_flow_completed_total");
    public static readonly Counter<long> FlowAbandoned = Meter.CreateCounter<long>("conversation_flow_abandoned_total");
    public static readonly Counter<long> FlowTimeout = Meter.CreateCounter<long>("conversation_flow_timeout_total");
    public static readonly Counter<long> Handoff = Meter.CreateCounter<long>("conversation_handoff_total");
    public static readonly Histogram<double> MessagesPerFlow = Meter.CreateHistogram<double>("conversation_messages_per_flow", unit: "messages");

    public static void RecordIntent(ConversationIntent intent, ConversationFlowState flowState)
    {
        var tags = new TagList { { "intent", intent.ToString() }, { "flow", flowState.ToString() } };
        IntentDetected.Add(1, tags);
        MessagesPerFlow.Record(1, tags);
        if (intent is ConversationIntent.Unknown or ConversationIntent.Unsupported) UnknownIntent.Add(1, tags);
    }
}
