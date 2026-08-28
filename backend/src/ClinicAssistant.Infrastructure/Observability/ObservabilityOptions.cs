namespace ClinicAssistant.Infrastructure.Observability;

public sealed record ObservabilityOptions
{
    public const string SectionName = "Observability";
    public bool Enabled { get; set; }
    public double TraceSamplingRatio { get; set; } = 1.0;
}
