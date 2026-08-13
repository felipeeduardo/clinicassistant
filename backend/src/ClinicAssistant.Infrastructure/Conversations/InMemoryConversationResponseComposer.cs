using ClinicAssistant.Application.Conversations;
using System.Globalization;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class InMemoryConversationResponseComposer : IConversationResponseComposer
{
    private static readonly IReadOnlyDictionary<string, string> OptionLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["specialties"] = "Ver especialidades",
        ["professionals"] = "Ver profissionais",
        ["availability"] = "Consultar disponibilidade",
        ["schedule"] = "Agendar consulta",
        ["reschedule"] = "Reagendar consulta",
        ["cancel_appointment"] = "Cancelar consulta",
        ["confirm"] = "Confirmar consulta",
        ["human"] = "Falar com atendente"
    };

    public ConversationResponse Compose(ConversationResponseRequest request)
    {
        var text = request.CustomText ?? (ConversationMessageCatalog.Text.TryGetValue(request.ResponseKey, out var responseText) ? responseText : ConversationMessageCatalog.Text["conversation.menu"]);
        if (request.Options.Count == 0) return new(text, request.Options);

        var menu = string.Join(Environment.NewLine, request.Options
            .OrderBy(option => option.DisplayOrder)
            .Select(option => $"{option.Key} - {DisplayLabel(option.Value)}"));
        return new($"{text}{Environment.NewLine}{Environment.NewLine}{menu}", request.Options);
    }

    private static string DisplayLabel(string value)
    {
        var parts = value.Split("||", 2, StringSplitOptions.None);
        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1])) return parts[1];
        if (parts[0].StartsWith("slot:", StringComparison.Ordinal))
        {
            var slot = parts[0].Split('|', StringSplitOptions.TrimEntries);
            if (slot.Length >= 3 && DateTimeOffset.TryParse(slot[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var startsAt) && DateTimeOffset.TryParse(slot[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var endsAt))
                return $"{startsAt:HH\\:mm} às {endsAt:HH\\:mm}";
            return "Horário disponível";
        }
        return OptionLabels.GetValueOrDefault(parts[0], parts[0]);
    }
}
