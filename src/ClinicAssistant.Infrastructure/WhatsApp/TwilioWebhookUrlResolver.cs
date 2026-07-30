using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioWebhookUrlResolver(IOptions<TwilioOptions> options)
{
    private readonly TwilioOptions _options = options.Value;

    public string Resolve(HttpRequest request)
        => Resolve(request, _options.IncomingWebhookBaseUrl);

    public string ResolveStatusCallback(HttpRequest request)
        => Resolve(request, _options.StatusCallbackBaseUrl);

    private static string Resolve(HttpRequest request, string? configuredBaseUrl)
    {
        var pathAndQuery = $"{request.PathBase}{request.Path}{request.QueryString}";
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
            return $"{configuredBaseUrl.TrimEnd('/')}{pathAndQuery}";
        return $"{request.Scheme}://{request.Host}{pathAndQuery}";
    }
}
