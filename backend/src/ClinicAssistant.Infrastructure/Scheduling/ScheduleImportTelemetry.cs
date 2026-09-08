using System.Diagnostics;

namespace ClinicAssistant.Infrastructure.Scheduling;

internal static class ScheduleImportTelemetry
{
    public static readonly ActivitySource ActivitySource = new("ClinicAssistant.ScheduleImport");
}
