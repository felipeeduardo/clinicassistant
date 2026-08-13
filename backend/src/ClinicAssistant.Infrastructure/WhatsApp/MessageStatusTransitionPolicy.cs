using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class MessageStatusTransitionPolicy : IMessageStatusTransitionPolicy
{
    public bool CanTransition(ConversationMessageStatus current, ConversationMessageStatus targetStatus)
    {
        if (current == targetStatus) return true;
        if (current is ConversationMessageStatus.Received or ConversationMessageStatus.Failed) return false;
        if (targetStatus == ConversationMessageStatus.Failed) return current is not ConversationMessageStatus.Delivered and not ConversationMessageStatus.Read;
        return GetPrecedence(targetStatus) > GetPrecedence(current);
    }

    private static int GetPrecedence(ConversationMessageStatus status) => status switch
    {
        ConversationMessageStatus.Pending => 0,
        ConversationMessageStatus.Queued => 1,
        ConversationMessageStatus.Accepted => 2,
        ConversationMessageStatus.Sent => 3,
        ConversationMessageStatus.Delivered => 4,
        ConversationMessageStatus.Read => 5,
        _ => -1
    };
}
