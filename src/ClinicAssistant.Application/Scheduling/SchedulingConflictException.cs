namespace ClinicAssistant.Application.Scheduling;

public sealed class SchedulingConflictException(string message) : InvalidOperationException(message);
