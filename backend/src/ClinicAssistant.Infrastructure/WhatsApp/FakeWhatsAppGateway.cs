using System.Collections.Concurrent;
using ClinicAssistant.Application.WhatsApp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed partial class FakeWhatsAppGateway(IOptions<WhatsAppOptions> options, ILogger<FakeWhatsAppGateway> logger) : IWhatsAppGateway
{
    public WhatsAppGatewayCapabilities Capabilities { get; } = new(true, true, true);
    private readonly ConcurrentDictionary<string, SendWhatsAppMessageResult> _results = new(StringComparer.Ordinal);
    private readonly FakeWhatsAppOptions _options = options.Value.Fake;

    public Task<SendWhatsAppMessageResult> SendTextAsync(SendWhatsAppTextRequest request, CancellationToken cancellationToken) =>
        SendAsync(request.IdempotencyKey, request.IntegrationId, "text", cancellationToken);

    public Task<SendWhatsAppMessageResult> SendInteractiveAsync(SendWhatsAppInteractiveRequest request, CancellationToken cancellationToken) =>
        SendAsync(request.IdempotencyKey, request.IntegrationId, $"interactive:{request.Interaction.Type}", cancellationToken);

    public Task<SendWhatsAppMessageResult> SendTemplateAsync(SendWhatsAppTemplateRequest request, CancellationToken cancellationToken) =>
        SendAsync(request.IdempotencyKey, request.IntegrationId, "template", cancellationToken);

    public Task<SendWhatsAppMessageResult> SendMediaAsync(SendWhatsAppMediaRequest request, CancellationToken cancellationToken) =>
        SendAsync(request.IdempotencyKey, request.IntegrationId, "media", cancellationToken);

    private async Task<SendWhatsAppMessageResult> SendAsync(string idempotencyKey, Guid integrationId, string messageType, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (_results.TryGetValue(idempotencyKey, out var existing)) return existing;
        if (_options.DelayMilliseconds > 0) await Task.Delay(_options.DelayMilliseconds, cancellationToken);
        if (_options.FailureMode == FakeWhatsAppFailureMode.Timeout)
            throw new TimeoutException("The simulated WhatsApp gateway timed out.");

        var result = CreateResult();
        _results.TryAdd(idempotencyKey, result);
        LogSimulatedMessage(logger, integrationId, messageType, result.Success);
        return result;
    }

    private SendWhatsAppMessageResult CreateResult()
    {
        var failureMode = _options.FailureMode;
        if (failureMode == FakeWhatsAppFailureMode.None && _options.FailureRate > 0 && Random.Shared.NextDouble() < (double)_options.FailureRate)
            failureMode = FakeWhatsAppFailureMode.Transient;

        return failureMode switch
        {
            FakeWhatsAppFailureMode.Transient => new(false, null, "failed", new(WhatsAppFailureType.Transient, "fake_transient", "A temporary provider error occurred.", true)),
            FakeWhatsAppFailureMode.Permanent => new(false, null, "failed", new(WhatsAppFailureType.Permanent, "fake_permanent", "The provider rejected the message.", false)),
            _ => new(true, $"SM_FAKE_{Guid.NewGuid():N}", "accepted", null)
        };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Fake WhatsApp message simulated. IntegrationId: {IntegrationId}; Type: {MessageType}; Success: {Success}")]
    private static partial void LogSimulatedMessage(ILogger logger, Guid integrationId, string messageType, bool success);
}
