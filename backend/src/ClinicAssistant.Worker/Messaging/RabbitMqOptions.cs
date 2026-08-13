namespace ClinicAssistant.Worker.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public int OutboxBatchSize { get; init; } = 50;
    public int OutboxPollingSeconds { get; init; } = 5;
    public int MaximumRetryAttempts { get; init; } = 4;
    public ushort ConsumerPrefetchCount { get; init; } = 20;
}
