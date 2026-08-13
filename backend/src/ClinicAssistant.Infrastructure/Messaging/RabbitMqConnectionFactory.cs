using RabbitMQ.Client;

namespace ClinicAssistant.Infrastructure.Messaging;

public sealed class RabbitMqConnectionFactory(RabbitMqOptions options)
{
    public ConnectionFactory Create(string clientName)
    {
        options.Validate();
        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = clientName
        };

        if (options.UseTls)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = options.ServerName!
            };
        }

        return factory;
    }
}
