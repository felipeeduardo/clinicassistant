using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class PhoneMasker : IPhoneMasker
{
    public string Mask(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return string.Empty;
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length <= 5) return "***";
        var prefixLength = Math.Min(2, digits.Length - 3);
        return $"+{digits[..prefixLength]}******{digits[^3..]}";
    }
}
