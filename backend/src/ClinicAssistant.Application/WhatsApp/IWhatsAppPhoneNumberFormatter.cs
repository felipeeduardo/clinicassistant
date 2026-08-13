namespace ClinicAssistant.Application.WhatsApp;

public interface IWhatsAppPhoneNumberFormatter
{
    string FormatForProvider(string phoneNumber);
}
