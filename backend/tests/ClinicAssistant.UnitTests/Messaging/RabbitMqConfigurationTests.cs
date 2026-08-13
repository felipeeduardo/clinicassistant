using ClinicAssistant.Infrastructure.Messaging;
using RabbitMQ.Client;
using Xunit;

namespace ClinicAssistant.UnitTests.Messaging;

public sealed class RabbitMqConfigurationTests
{
    [Fact]
    public void LocalConfigurationAllowsPlainAmqpWithRootVirtualHost()
    {
        var options = new RabbitMqOptions();

        options.Validate();

        var factory = new RabbitMqConnectionFactory(options).Create("test-worker");
        Assert.Equal("localhost", factory.HostName);
        Assert.Equal(5672, factory.Port);
        Assert.Equal("/", factory.VirtualHost);
        Assert.False(factory.Ssl.Enabled);
    }

    [Fact]
    public void TlsConfigurationUsesServerNameAndVirtualHost()
    {
        var options = new RabbitMqOptions
        {
            Host = "broker.example.test",
            Port = 5671,
            Username = "app",
            Password = "secret",
            VirtualHost = "clinicassistant",
            UseTls = true,
            ServerName = "broker.example.test"
        };

        var factory = new RabbitMqConnectionFactory(options).Create("test-api");

        Assert.Equal("clinicassistant", factory.VirtualHost);
        Assert.True(factory.Ssl.Enabled);
        Assert.Equal("broker.example.test", factory.Ssl.ServerName);
    }

    [Fact]
    public void TlsRequiresServerName()
    {
        var options = new RabbitMqOptions { UseTls = true };

        var exception = Assert.Throws<InvalidOperationException>(() => options.Validate());

        Assert.Contains("ServerName", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void PortMustBeValid(int port)
    {
        var options = new RabbitMqOptions { Port = port };

        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void VirtualHostCannotBeEmpty()
    {
        var options = new RabbitMqOptions { VirtualHost = " " };

        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }
}
