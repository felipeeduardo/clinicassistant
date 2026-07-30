using ClinicAssistant.Domain.Messaging;
using Xunit;

namespace ClinicAssistant.UnitTests.Domain;

public sealed class OutboxMessageTests
{
    [Fact]
    public void FirstFailureSchedulesTheConfiguredBackoff()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "WhatsAppIncomingMessageReceived", "{}");
        var beforeFailure = DateTimeOffset.UtcNow;

        message.MarkFailure("PublishException", 4);

        Assert.Equal(MessageStatus.Pending, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.NotNull(message.NextAttemptAt);
        Assert.InRange(message.NextAttemptAt!.Value, beforeFailure.AddSeconds(29), beforeFailure.AddSeconds(31));
    }

    [Fact]
    public void FinalFailureMarksTheMessageAsDeadLettered()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "WhatsAppIncomingMessageReceived", "{}");

        for (var index = 0; index < 4; index++) message.MarkFailure("PublishException", 4);

        Assert.Equal(MessageStatus.DeadLettered, message.Status);
        Assert.Null(message.NextAttemptAt);
    }
}
