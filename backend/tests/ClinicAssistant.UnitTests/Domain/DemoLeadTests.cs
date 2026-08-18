using ClinicAssistant.Domain.Platform;
using Xunit;

namespace ClinicAssistant.UnitTests.Domain;

public sealed class DemoLeadTests
{
    [Fact]
    public void NewLeadStartsAsNewAndUsesCommercialSource()
    {
        var lead = new DemoLead("Ana", "Clínica Exemplo", "ana@example.com", "+5581999999999", null, "LandingDemoForm");

        Assert.Equal(DemoLeadStatus.New, lead.Status);
        Assert.Equal("LandingDemoForm", lead.Source);
    }

    [Fact]
    public void ContactedLeadRecordsLastContactAndAssignmentCanBeCleared()
    {
        var lead = new DemoLead("Ana", "Clínica Exemplo", "ana@example.com", "+5581999999999", null, "LandingDemoForm");
        lead.Assign(Guid.NewGuid());
        lead.ChangeStatus(DemoLeadStatus.Contacted);
        lead.Assign(null);

        Assert.Equal(DemoLeadStatus.Contacted, lead.Status);
        Assert.NotNull(lead.LastContactAt);
        Assert.Null(lead.AssignedToUserId);
    }
}
