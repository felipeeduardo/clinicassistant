using System.Text.Json;
using ClinicAssistant.Application.WhatsApp;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppTemplateVariableValidator : IWhatsAppTemplateVariableValidator
{
    public bool IsValid(string? parametersSchema, IReadOnlyDictionary<string, string> variables)
    {
        if (variables.Any(variable => string.IsNullOrWhiteSpace(variable.Key) || string.IsNullOrWhiteSpace(variable.Value))) return false;
        if (string.IsNullOrWhiteSpace(parametersSchema)) return variables.Count == 0;
        try
        {
            var expectedKeys = JsonSerializer.Deserialize<string[]>(parametersSchema);
            return expectedKeys is not null && expectedKeys.Length == variables.Count && expectedKeys.All(variables.ContainsKey);
        }
        catch (JsonException) { return false; }
    }
}
