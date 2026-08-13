using ClinicAssistant.Application.WhatsApp;
using Microsoft.Extensions.Options;
using Twilio.Security;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioWebhookSignatureValidator(IOptions<TwilioOptions> options) : ITwilioWebhookSignatureValidator
{
    private readonly TwilioOptions _options = options.Value;

    public bool IsValid(string requestUrl, IReadOnlyDictionary<string, string> parameters, string? signature)
    {
        if (string.IsNullOrWhiteSpace(_options.AuthToken) || string.IsNullOrWhiteSpace(signature)) return false;
        var validator = new RequestValidator(_options.AuthToken);
        return validator.Validate(requestUrl, parameters.ToDictionary(item => item.Key, item => item.Value), signature);
    }
}
