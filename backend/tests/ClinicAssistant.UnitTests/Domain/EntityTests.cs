using ClinicAssistant.Domain.Primitives;
using Xunit;

namespace ClinicAssistant.UnitTests.Domain;

public sealed class EntityTests
{
    [Fact]
    public void NewEntityHasAnIdentifierAndUtcTimestamps()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(TimeSpan.Zero, entity.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, entity.UpdatedAt.Offset);
    }

    private sealed class TestEntity : Entity;
}
