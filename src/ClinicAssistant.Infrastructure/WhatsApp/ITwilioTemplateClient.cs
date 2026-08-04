namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed record TwilioRemoteTemplate(string ContentSid, string Name, string LanguageCode, IReadOnlyList<string> Variables, DateTimeOffset? UpdatedAt);
public interface ITwilioTemplateClient
{
    Task<IReadOnlyList<TwilioRemoteTemplate>> ListAsync(CancellationToken cancellationToken);
}
