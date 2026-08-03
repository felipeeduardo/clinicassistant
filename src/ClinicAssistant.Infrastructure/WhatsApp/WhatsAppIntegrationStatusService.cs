using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.WhatsApp;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ClinicAssistant.Application.Realtime;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppIntegrationStatusService(ClinicAssistantDbContext dbContext, ITenantContext tenantContext, IPhoneMasker phoneMasker, IOptions<WhatsAppOptions> options, IOperationalEventPublisher events) : IWhatsAppIntegrationStatusService
{
    private readonly WhatsAppOptions _options = options.Value;
    public async Task<WhatsAppIntegrationOperationalStatus?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue) return null;
        var integration = await dbContext.WhatsAppIntegrations
            .Where(item => item.TenantId == tenantContext.TenantId.Value)
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (integration is null) return null;

        return new(integration.Provider.ToString(), integration.Status.ToString(), phoneMasker.Mask(integration.DisplayPhoneNumber ?? integration.WhatsAppFrom),
            integration.LastWebhookAt, integration.LastSuccessfulSendAt, integration.LastFailureAt, integration.FailureReason);
    }
    public async Task ValidateCurrentAsync(CancellationToken cancellationToken) { var integration = await CurrentAsync(cancellationToken); if (string.IsNullOrWhiteSpace(integration.WhatsAppFrom) || integration.Provider == WhatsAppProvider.Meta) throw new InvalidOperationException("The integration configuration is incomplete."); integration.MarkValidated(); dbContext.AuditRecords.Add(new AuditRecord(integration.TenantId, tenantContext.UserId, "whatsapp.integration.validated", "WhatsAppIntegration", integration.Id, "Succeeded", "Local configuration validation completed.")); await dbContext.SaveChangesAsync(cancellationToken); await PublishUpdatedAsync(integration.TenantId, cancellationToken); }
    public async Task EnableCurrentAsync(CancellationToken cancellationToken) { var integration = await CurrentAsync(cancellationToken); integration.MarkConnected(); dbContext.AuditRecords.Add(new AuditRecord(integration.TenantId, tenantContext.UserId, "whatsapp.integration.enabled", "WhatsAppIntegration", integration.Id, "Succeeded", "Integration enabled.")); await dbContext.SaveChangesAsync(cancellationToken); await PublishUpdatedAsync(integration.TenantId, cancellationToken); }
    public async Task DisableCurrentAsync(CancellationToken cancellationToken) { var integration = await CurrentAsync(cancellationToken); integration.Disable(); dbContext.AuditRecords.Add(new AuditRecord(integration.TenantId, tenantContext.UserId, "whatsapp.integration.disabled", "WhatsAppIntegration", integration.Id, "Succeeded", "Integration disabled.")); await dbContext.SaveChangesAsync(cancellationToken); await PublishUpdatedAsync(integration.TenantId, cancellationToken); }
    public async Task QueueTestMessageAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || string.IsNullOrWhiteSpace(_options.TestRecipient)) throw new InvalidOperationException("Idempotency-Key and WhatsApp:TestRecipient are required."); var integration = await CurrentAsync(cancellationToken); if (integration.Status != WhatsAppIntegrationStatus.Connected) throw new InvalidOperationException("Enable the integration before sending a test message."); var scope = $"whatsapp.integration.test:{integration.Id}"; if (await dbContext.IdempotencyRecords.AnyAsync(item => item.Scope == scope && item.Key == idempotencyKey, cancellationToken)) return;
        var recipient = _options.TestRecipient.Trim(); var patient = await dbContext.Patients.SingleOrDefaultAsync(item => item.TenantId == integration.TenantId && item.Phone == recipient, cancellationToken); if (patient is null) { patient = new Patient(integration.TenantId, "WhatsApp Test Recipient", recipient, null, null, ConsentStatus.Granted, PatientSource.WhatsApp); dbContext.Patients.Add(patient); }
        var conversation = await dbContext.Conversations.SingleOrDefaultAsync(item => item.TenantId == integration.TenantId && item.IntegrationId == integration.Id && item.PatientId == patient.Id, cancellationToken); if (conversation is null) { conversation = new Conversation(integration.TenantId, patient.Id, integration.Id, recipient); dbContext.Conversations.Add(conversation); }
        var inbound = new ConversationMessage(integration.TenantId, conversation.Id, ConversationMessageType.System, "Test window opened.", integration.Provider, $"test-inbound-{Guid.NewGuid():N}", DateTimeOffset.UtcNow); var outbound = new ConversationMessage(integration.TenantId, conversation.Id, ConversationMessageType.Text, "Clinic Assistant: mensagem de teste da integração WhatsApp.", integration.Provider); var command = new SendWhatsAppMessageCommand(integration.TenantId, integration.Id, conversation.Id, outbound.Id, WhatsAppOutgoingMessageType.Text, recipient, outbound.Content, null, null, null, $"integration-test:{outbound.Id:N}", idempotencyKey);
        dbContext.AddRange(inbound, outbound, new OutboxMessage(integration.TenantId, nameof(SendWhatsAppMessageCommand), JsonSerializer.Serialize(command)), new IdempotencyRecord(scope, idempotencyKey, "{}"), new AuditRecord(integration.TenantId, tenantContext.UserId, "whatsapp.integration.test_queued", "WhatsAppIntegration", integration.Id, "Succeeded", "Test message queued.")); await dbContext.SaveChangesAsync(cancellationToken);
    }
    private async Task<WhatsAppIntegration> CurrentAsync(CancellationToken ct) { var tenantId = tenantContext.TenantId ?? throw new UnauthorizedAccessException(); return await dbContext.WhatsAppIntegrations.Where(item => item.TenantId == tenantId).OrderByDescending(item => item.UpdatedAt).FirstOrDefaultAsync(ct) ?? throw new KeyNotFoundException("WhatsApp integration not found."); }
    private async Task PublishUpdatedAsync(Guid tenantId, CancellationToken ct) { await events.PublishAsync(tenantId, "whatsapp.integration.updated", new { }, ct); await events.PublishAsync(tenantId, "dashboard.invalidated", new { }, ct); }
}
