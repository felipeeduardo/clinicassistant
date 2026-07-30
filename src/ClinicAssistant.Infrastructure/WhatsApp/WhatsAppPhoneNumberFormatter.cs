using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppPhoneNumberFormatter : IWhatsAppPhoneNumberFormatter
{
    public string FormatForProvider(string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        var normalized = phoneNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
            ? phoneNumber["whatsapp:".Length..]
            : phoneNumber;

        if (!normalized.StartsWith('+') || normalized.Length < 8 || !normalized[1..].All(char.IsDigit))
            throw new ArgumentException("The phone number must use E.164 format.", nameof(phoneNumber));

        return $"whatsapp:{normalized}";
    }
}
