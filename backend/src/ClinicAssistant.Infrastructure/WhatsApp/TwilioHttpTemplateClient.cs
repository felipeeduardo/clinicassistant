using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioHttpTemplateClient(HttpClient httpClient, IOptions<TwilioOptions> options) : ITwilioTemplateClient
{
    private readonly TwilioOptions _options = options.Value;

    public async Task<IReadOnlyList<TwilioRemoteTemplate>> ListAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.AccountSid) || string.IsNullOrWhiteSpace(_options.AuthToken)) throw new InvalidOperationException("Twilio credentials are not configured.");
        using var request = new HttpRequestMessage(HttpMethod.Get, "v1/Content?PageSize=500");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.AccountSid}:{_options.AuthToken}")));
        using var response = await httpClient.SendAsync(request, cancellationToken); response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken); using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("contents", out var contents) || contents.ValueKind != JsonValueKind.Array) return [];
        return contents.EnumerateArray().Select(Read).Where(item => item is not null).Cast<TwilioRemoteTemplate>().ToArray();
    }

    private static TwilioRemoteTemplate? Read(JsonElement item)
    {
        var sid = item.TryGetProperty("sid", out var sidValue) ? sidValue.GetString() : null; var name = item.TryGetProperty("friendly_name", out var nameValue) ? nameValue.GetString() : null; var language = item.TryGetProperty("language", out var languageValue) ? languageValue.GetString() : null;
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(language)) return null;
        var variables = item.TryGetProperty("variables", out var variableValues) && variableValues.ValueKind == JsonValueKind.Object ? variableValues.EnumerateObject().Select(property => property.Name).ToArray() : [];
        DateTimeOffset? updated = item.TryGetProperty("date_updated", out var updatedValue) && DateTimeOffset.TryParse(updatedValue.GetString(), out var parsed) ? parsed : null;
        return new(sid, name, language, variables, updated);
    }
}
