using ClinicAssistant.Application.Conversations;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Conversations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace ClinicAssistant.UnitTests.Conversations;

public sealed class ConversationOrchestratorTests
{
    [Fact]
    public async Task ProcessingCreatesResponseOutboxAndPersistentIdempotencyRecord()
    {
        await using var dbContext = CreateDbContext();
        var fixture = await SeedAsync(dbContext);
        var orchestrator = CreateOrchestrator(dbContext, new AvailableLockManager());

        var result = await orchestrator.ProcessAsync(fixture.Command, CancellationToken.None);

        Assert.Equal(ConversationOrchestrationResult.Processed, result);
        Assert.Single(await dbContext.ConversationProcessedMessages.IgnoreQueryFilters().ToListAsync());
        Assert.Single(await dbContext.OutboxMessages.IgnoreQueryFilters().ToListAsync());
        Assert.Equal(2, await dbContext.ConversationMessages.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task ProcessingTheSameMessageTwiceDoesNotCreateAnotherResponse()
    {
        await using var dbContext = CreateDbContext();
        var fixture = await SeedAsync(dbContext);
        var orchestrator = CreateOrchestrator(dbContext, new AvailableLockManager());

        await orchestrator.ProcessAsync(fixture.Command, CancellationToken.None);
        var duplicate = await orchestrator.ProcessAsync(fixture.Command, CancellationToken.None);

        Assert.Equal(ConversationOrchestrationResult.Duplicate, duplicate);
        Assert.Single(await dbContext.OutboxMessages.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task ProcessingWithAnotherTenantIsRejected()
    {
        await using var dbContext = CreateDbContext();
        var fixture = await SeedAsync(dbContext);
        var orchestrator = CreateOrchestrator(dbContext, new AvailableLockManager());
        var command = fixture.Command with { TenantId = Guid.NewGuid() };

        var result = await orchestrator.ProcessAsync(command, CancellationToken.None);

        Assert.Equal(ConversationOrchestrationResult.Rejected, result);
    }

    [Fact]
    public async Task ProcessingWithoutLockDoesNotPersistResponse()
    {
        await using var dbContext = CreateDbContext();
        var fixture = await SeedAsync(dbContext);
        var orchestrator = CreateOrchestrator(dbContext, new UnavailableLockManager());

        var result = await orchestrator.ProcessAsync(fixture.Command, CancellationToken.None);

        Assert.Equal(ConversationOrchestrationResult.LockUnavailable, result);
        Assert.Empty(await dbContext.OutboxMessages.IgnoreQueryFilters().ToListAsync());
    }

    private static ConversationOrchestrator CreateOrchestrator(ClinicAssistantDbContext dbContext, IConversationLockManager lockManager)
    {
        var options = Options.Create(new ConversationOptions { StateExpirationMinutes = 30, MaximumInvalidAttempts = 3, MaxOptionsPerMessage = 10, MaxMessageLength = 2_000 });
        return new(dbContext, lockManager, new ConversationStateMachine(options), new InMemoryConversationResponseComposer(), options);
    }

    private static ClinicAssistantDbContext CreateDbContext() => new(new DbContextOptionsBuilder<ClinicAssistantDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options);

    private static async Task<Fixture> SeedAsync(ClinicAssistantDbContext dbContext)
    {
        var tenantId = Guid.NewGuid();
        var patient = new Patient(tenantId, "Paciente", "+5581999999999", null, null, ConsentStatus.Unknown, PatientSource.WhatsApp);
        var integration = new WhatsAppIntegration(tenantId, WhatsAppProvider.Twilio, "wha_test", "+5581888888888");
        integration.MarkConnected();
        var conversation = new Conversation(tenantId, patient.Id, integration.Id, patient.Phone);
        var message = new ConversationMessage(tenantId, conversation.Id, ConversationMessageType.Text, "Olá", WhatsAppProvider.Twilio, "SM123", DateTimeOffset.UtcNow);
        dbContext.AddRange(patient, integration, conversation, message);
        await dbContext.SaveChangesAsync();
        return new(new(tenantId, integration.Id, conversation.Id, message.Id, "test-correlation"));
    }

    private sealed record Fixture(ProcessConversationMessageCommand Command);

    private sealed class AvailableLockManager : IConversationLockManager
    {
        public Task<IConversationLockHandle?> TryAcquireAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken) => Task.FromResult<IConversationLockHandle?>(new LockHandle());
    }

    private sealed class UnavailableLockManager : IConversationLockManager
    {
        public Task<IConversationLockHandle?> TryAcquireAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken) => Task.FromResult<IConversationLockHandle?>(null);
    }

    private sealed class LockHandle : IConversationLockHandle
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
