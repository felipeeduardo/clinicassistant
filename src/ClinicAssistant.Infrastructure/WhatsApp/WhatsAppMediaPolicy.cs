using ClinicAssistant.Application.WhatsApp;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppMediaPolicy(IOptions<WhatsAppOptions> options) : IWhatsAppMediaPolicy
{
    private readonly WhatsAppMediaOptions _options = options.Value.Media;
    private readonly HashSet<string> _allowedTypes = options.Value.Media.AllowedTypes
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public WhatsAppMediaPolicyResult Evaluate(string? contentType, long? contentLength)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !_allowedTypes.Contains(contentType.Trim()))
            return new(WhatsAppMediaDisposition.RequiresHuman, false, "Unsupported media type.");
        if (contentLength is < 0 || contentLength > _options.MaxFileSizeBytes)
            return new(WhatsAppMediaDisposition.RequiresHuman, false, "Media exceeds the configured size limit.");
        return new(WhatsAppMediaDisposition.Accepted, contentLength is null, null);
    }
}
