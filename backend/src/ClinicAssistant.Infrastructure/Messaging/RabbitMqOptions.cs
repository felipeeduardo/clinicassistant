namespace ClinicAssistant.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public bool UseTls { get; init; }
    public string? ServerName { get; init; }
    public int OutboxBatchSize { get; init; } = 50;
    public int OutboxPollingSeconds { get; init; } = 5;
    public int MaximumRetryAttempts { get; init; } = 4;
    public ushort ConsumerPrefetchCount { get; init; } = 20;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host)) throw new InvalidOperationException("RabbitMq:Host is required.");
        if (Port is < 1 or > 65_535) throw new InvalidOperationException("RabbitMq:Port must be between 1 and 65535.");
        if (string.IsNullOrWhiteSpace(Username)) throw new InvalidOperationException("RabbitMq:Username is required.");
        if (string.IsNullOrWhiteSpace(Password)) throw new InvalidOperationException("RabbitMq:Password is required.");
        if (string.IsNullOrWhiteSpace(VirtualHost)) throw new InvalidOperationException("RabbitMq:VirtualHost is required.");
        if (UseTls && string.IsNullOrWhiteSpace(ServerName)) throw new InvalidOperationException("RabbitMq:ServerName is required when RabbitMq:UseTls=true.");
    }
}
