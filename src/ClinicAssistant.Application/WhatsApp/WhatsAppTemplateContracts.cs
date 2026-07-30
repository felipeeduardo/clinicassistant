namespace ClinicAssistant.Application.WhatsApp;

public interface IWhatsAppConversationWindowPolicy
{
    WhatsAppConversationWindowResult Evaluate(DateTimeOffset? lastInboundMessageAt, DateTimeOffset currentTime);
}

public sealed record WhatsAppConversationWindowResult(WhatsAppConversationWindowStatus Status)
{
    public bool AllowsFreeFormText => Status == WhatsAppConversationWindowStatus.Open;
    public bool RequiresTemplate => Status is WhatsAppConversationWindowStatus.Expired or WhatsAppConversationWindowStatus.NoInboundHistory;
}

public enum WhatsAppConversationWindowStatus { NoInboundHistory, Open, Expired }

public interface IWhatsAppTemplateVariableValidator
{
    bool IsValid(string? parametersSchema, IReadOnlyDictionary<string, string> variables);
}
