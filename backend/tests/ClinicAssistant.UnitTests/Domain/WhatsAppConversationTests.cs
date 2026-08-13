using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.WhatsApp;
using Xunit;

namespace ClinicAssistant.UnitTests.Domain;

public sealed class WhatsAppConversationTests
{
    [Fact]
    public void WhatsAppPatientRegistersFirstAndLastContact()
    {
        var patient = new Patient(Guid.NewGuid(), "Paciente", "+5581999999999", null, null, ConsentStatus.Unknown, PatientSource.WhatsApp);
        var firstContact = DateTimeOffset.UtcNow.AddMinutes(-5);

        patient.RegisterContact(firstContact);
        patient.RegisterContact(DateTimeOffset.UtcNow);

        Assert.Equal(firstContact, patient.FirstContactAt);
        Assert.NotNull(patient.LastContactAt);
        Assert.Equal(PatientSource.WhatsApp, patient.Source);
    }

    [Fact]
    public void IncomingConversationMessageHasReceivedStatus()
    {
        var message = new ConversationMessage(Guid.NewGuid(), Guid.NewGuid(), ConversationMessageType.Text, "Olá", WhatsAppProvider.Twilio, "SM123", DateTimeOffset.UtcNow);

        Assert.Equal(ConversationMessageDirection.Inbound, message.Direction);
        Assert.Equal(ConversationMessageStatus.Received, message.Status);
        Assert.Equal("SM123", message.ExternalMessageId);
    }

    [Fact]
    public void OutgoingConversationMessageStoresProviderAcceptance()
    {
        var message = new ConversationMessage(Guid.NewGuid(), Guid.NewGuid(), ConversationMessageType.Text, "Confirmação", WhatsAppProvider.Twilio);

        message.MarkAccepted("SM456", "accepted");

        Assert.Equal(ConversationMessageStatus.Accepted, message.Status);
        Assert.Equal("SM456", message.ExternalMessageId);
        Assert.Equal("accepted", message.ProviderStatus);
    }
}
