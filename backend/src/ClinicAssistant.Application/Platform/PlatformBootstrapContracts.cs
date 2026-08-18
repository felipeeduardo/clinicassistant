namespace ClinicAssistant.Application.Platform;

public sealed class PlatformBootstrapAdmin
{
    // These properties are populated by Microsoft.Extensions.Configuration's
    // options binder from PlatformBootstrap__Admins__N__* environment variables.
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformBootstrapOptions
{
    public const string SectionName = "PlatformBootstrap";
    public bool Enabled { get; set; }
    public List<PlatformBootstrapAdmin> Admins { get; set; } = [];
}

public sealed record PlatformBootstrapResult(bool Enabled, int Created, int AlreadyExisting);

public interface IPlatformBootstrapService
{
    Task<PlatformBootstrapResult> RunAsync(CancellationToken cancellationToken);
}
