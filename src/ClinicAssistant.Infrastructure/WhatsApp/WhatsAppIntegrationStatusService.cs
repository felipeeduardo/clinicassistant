using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppIntegrationStatusService(ClinicAssistantDbContext dbContext, ITenantContext tenantContext, IPhoneMasker phoneMasker) : IWhatsAppIntegrationStatusService
{
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
}
