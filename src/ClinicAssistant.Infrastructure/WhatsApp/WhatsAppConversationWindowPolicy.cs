using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppConversationWindowPolicy : IWhatsAppConversationWindowPolicy
{
    private static readonly TimeSpan WindowDuration = TimeSpan.FromHours(24);

    public WhatsAppConversationWindowResult Evaluate(DateTimeOffset? lastInboundMessageAt, DateTimeOffset currentTime)
    {
        if (lastInboundMessageAt is null) return new(WhatsAppConversationWindowStatus.NoInboundHistory);
        return currentTime - lastInboundMessageAt.Value <= WindowDuration
            ? new(WhatsAppConversationWindowStatus.Open)
            : new(WhatsAppConversationWindowStatus.Expired);
    }
}
