namespace ClinicAssistant.Application.Platform;

public sealed class ClinicNotReadyForActivationException(Guid tenantId, IReadOnlyCollection<string> missingSteps)
    : InvalidOperationException("The clinic is not ready for activation. Complete the required onboarding steps first.")
{
    public Guid TenantId { get; } = tenantId;
    public IReadOnlyCollection<string> MissingSteps { get; } = missingSteps;
}
