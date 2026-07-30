using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class TwilioHttpMessageClient(HttpClient httpClient, IOptions<TwilioOptions> options) : ITwilioMessageClient
{
    private readonly TwilioOptions _options = options.Value;

    public Task<TwilioMessageResult> SendTextAsync(TwilioSendTextRequest request, CancellationToken cancellationToken) =>
        SendAsync(CreateContent(request.To, request.From, request.MessagingServiceSid, [new("Body", request.Body)]), cancellationToken);

    public Task<TwilioMessageResult> SendTemplateAsync(TwilioSendTemplateRequest request, CancellationToken cancellationToken)
    {
        var variables = JsonSerializer.Serialize(request.Variables);
        return SendAsync(CreateContent(request.To, request.From, request.MessagingServiceSid, [new("ContentSid", request.ContentSid), new("ContentVariables", variables)]), cancellationToken);
    }

    public Task<TwilioMessageResult> SendMediaAsync(TwilioSendMediaRequest request, CancellationToken cancellationToken)
    {
        var fields = new List<KeyValuePair<string, string>> { new("MediaUrl", request.MediaUrl) };
        if (!string.IsNullOrWhiteSpace(request.Caption)) fields.Add(new("Body", request.Caption));
        return SendAsync(CreateContent(request.To, request.From, request.MessagingServiceSid, fields), cancellationToken);
    }

    private static FormUrlEncodedContent CreateContent(string to, string from, string? messagingServiceSid, IEnumerable<KeyValuePair<string, string>> fields)
    {
        var values = fields.ToList();
        values.Add(new("To", to));
        if (!string.IsNullOrWhiteSpace(messagingServiceSid)) values.Add(new("MessagingServiceSid", messagingServiceSid));
        else values.Add(new("From", from));
        return new FormUrlEncodedContent(values);
    }

    private async Task<TwilioMessageResult> SendAsync(FormUrlEncodedContent content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.AccountSid) || string.IsNullOrWhiteSpace(_options.AuthToken))
            return new(false, null, null, new("missing_credentials", "Twilio credentials are not configured.", null));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"2010-04-01/Accounts/{Uri.EscapeDataString(_options.AccountSid)}/Messages.json") { Content = content };
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.AccountSid}:{_options.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var payload = JsonDocument.Parse(responseBody);
        if (response.IsSuccessStatusCode)
        {
            var root = payload.RootElement;
            return new(true, root.GetProperty("sid").GetString(), root.GetProperty("status").GetString(), null);
        }

        var code = payload.RootElement.TryGetProperty("code", out var providerCode) ? providerCode.ToString() : null;
        return new(false, null, "failed", new(code, "Twilio rejected the message.", (int)response.StatusCode));
    }
}
