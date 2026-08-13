using System.Globalization;
using System.Text;
using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Domain.Conversations;

namespace ClinicAssistant.Infrastructure.Conversations;

/// <summary>Deterministic, provider-independent intent matching for WhatsApp messages.</summary>
public sealed class ConversationIntentResolver : IConversationIntentResolver
{
    public ConversationIntentResolution Resolve(string? message, ConversationContext context, IReadOnlyCollection<ConversationOptionDefinition> options)
    {
        var text = Normalize(message);
        if (text.Length == 0) return new(ConversationIntent.Unknown, text);
        if (text is "menu" or "inicio" or "voltar ao menu" or "comecar de novo") return new(ConversationIntent.MainMenu, text);
        if (text is "voltar" or "anterior" or "retornar") return new(ConversationIntent.GoBack, text);
        if (text is "cancelar operacao" or "cancelar fluxo" or "cancelar atendimento" or "desistir" or "sair") return new(ConversationIntent.CancelCurrentFlow, text);
        if (text is "atendente" or "humano" or "recepcao" or "falar com alguem" or "falar com uma pessoa") return new(ConversationIntent.HumanHandoff, text);
        if (text is "ajuda" or "o que posso fazer") return new(ConversationIntent.Help, text);
        if (text is "repetir" or "repete" or "novamente") return new(ConversationIntent.Repeat, text);
        if (context.PendingConfirmation && text is "sim" or "confirmar" or "pode" or "ok") return new(context.CurrentIntent is ConversationIntent.CancelAppointment ? ConversationIntent.CancelAppointment : context.CurrentIntent is ConversationIntent.RescheduleAppointment ? ConversationIntent.RescheduleAppointment : ConversationIntent.ConfirmAppointment, text);
        if (context.CurrentIntent is ConversationIntent.ScheduleAppointment or ConversationIntent.RescheduleAppointment && IsNaturalDate(text)) return new(context.CurrentIntent, text);

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) && options.Any(option => option.Key == text))
        {
            var option = options.Single(item => item.Key == text);
            return new(OptionIntent(option.Value, context.CurrentIntent), text, true);
        }

        return new(IdentifyIntent(text, context.CurrentIntent), text);
    }

    private static ConversationIntent OptionIntent(string value, ConversationIntent currentIntent) => value switch
    {
        "specialties" => ConversationIntent.ViewSpecialties,
        "professionals" => ConversationIntent.ViewProfessionals,
        "availability" => ConversationIntent.CheckAvailability,
        "schedule" => ConversationIntent.ScheduleAppointment,
        "reschedule" => ConversationIntent.RescheduleAppointment,
        "cancel_appointment" => ConversationIntent.CancelAppointment,
        "confirm" => ConversationIntent.ConfirmAppointment,
        "human" => ConversationIntent.HumanHandoff,
        _ when value.StartsWith("specialty:", StringComparison.Ordinal) => currentIntent == ConversationIntent.ListSpecialties ? ConversationIntent.ViewProfessionals : currentIntent,
        _ when value.StartsWith("professional:", StringComparison.Ordinal) => ConversationIntent.CheckAvailability,
        _ when value.StartsWith("slot:", StringComparison.Ordinal) || value.StartsWith("appointment:", StringComparison.Ordinal) => currentIntent,
        _ => currentIntent
    };

    private static ConversationIntent IdentifyIntent(string text, ConversationIntent currentIntent) => text switch
    {
        "1" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.ViewSpecialties,
        "2" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.ViewProfessionals,
        "3" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.CheckAvailability,
        "4" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.ScheduleAppointment,
        "5" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.RescheduleAppointment,
        "6" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.CancelAppointment,
        "7" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.ConfirmAppointment,
        "8" when currentIntent == ConversationIntent.Unknown || currentIntent == ConversationIntent.Greeting => ConversationIntent.HumanHandoff,
        _ when ContainsAny(text, "humano", "atendente", "pessoa", "recepcao", "falar com alguem", "falar com uma pessoa") => ConversationIntent.HumanHandoff,
        _ when ContainsAny(text, "diagnostico", "diagnosticar", "estou com dor", "sintoma", "sintomas", "prescricao", "receita medica") => ConversationIntent.HumanHandoff,
        _ when IsGreeting(text) => ConversationIntent.Greeting,
        _ when ContainsAny(text, "disponibilidade", "horario livre", "horarios disponiveis", "tem horario", "tem vaga", "tem cardiologista", "tem dermatologista") => ConversationIntent.CheckAvailability,
        _ when ContainsAny(text, "especialidade", "especialidades") => ConversationIntent.ViewSpecialties,
        _ when ContainsAny(text, "profissional", "profissionais", "medico", "medicos", "doutor", "doutora", "quem atende") => ConversationIntent.ViewProfessionals,
        _ when ContainsAny(text, "reagendar", "remarcar", "mudar meu horario") => ConversationIntent.RescheduleAppointment,
        _ when ContainsAny(text, "cancelar consulta", "desmarcar", "nao vou conseguir ir") => ConversationIntent.CancelAppointment,
        _ when ContainsAny(text, "confirmar", "confirmacao", "vou comparecer", "confirmar horario") => ConversationIntent.ConfirmAppointment,
        _ when ContainsAny(text, "agendar", "marcar consulta", "marcar") => ConversationIntent.ScheduleAppointment,
        _ when ContainsAny(text, "tchau", "adeus", "obrigado", "obrigada") => ConversationIntent.Farewell,
        _ => ConversationIntent.Unknown
    };

    private static bool ContainsAny(string text, params string[] values) => values.Any(text.Contains);
    private static bool IsGreeting(string text) => text is "oi" or "ola" || text.StartsWith("bom dia", StringComparison.Ordinal) || text.StartsWith("boa tarde", StringComparison.Ordinal) || text.StartsWith("boa noite", StringComparison.Ordinal);
    private static bool IsNaturalDate(string text) => text.Contains("hoje", StringComparison.Ordinal) || text.Contains("amanha", StringComparison.Ordinal) || text.Contains("segunda", StringComparison.Ordinal) || text.Contains("terca", StringComparison.Ordinal) || text.Contains("quarta", StringComparison.Ordinal) || text.Contains("quinta", StringComparison.Ordinal) || text.Contains("sexta", StringComparison.Ordinal) || text.Contains("sabado", StringComparison.Ordinal) || text.Contains("domingo", StringComparison.Ordinal);

    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Trim().ToLower(CultureInfo.InvariantCulture).Normalize(NormalizationForm.FormD);
        return string.Concat(normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark));
    }
}
