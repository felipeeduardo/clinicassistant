using ClinicAssistant.Application.WhatsApp;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.WhatsApp;

public sealed class WhatsAppChannelResolver(ClinicAssistantDbContext db, Microsoft.Extensions.Options.IOptions<WhatsAppOptions> options) : IWhatsAppChannelResolver
{
    private readonly WhatsAppOptions _options = options.Value;
    public async Task<WhatsAppChannelResolution?> ResolveInboundAsync(string? recipientPhone, string integrationKey, CancellationToken cancellationToken)
    {
        var normalized = string.IsNullOrWhiteSpace(recipientPhone) ? null : WhatsAppChannel.Normalize(recipientPhone);
        if (_options.MultiTenantChannelEnabled && normalized is not null)
        {
            var channel = await db.WhatsAppChannels.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.NormalizedPhoneNumber == normalized && x.Status == WhatsAppChannelStatus.Active, cancellationToken);
            if (channel is not null)
            {
                var integration = await db.WhatsAppIntegrations.IgnoreQueryFilters().Where(x => x.TenantId == channel.TenantId && x.Status == WhatsAppIntegrationStatus.Connected && (channel.IntegrationId == null || x.Id == channel.IntegrationId || x.IntegrationKey == integrationKey)).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);
                return new(channel.Id, channel.TenantId, integration?.Id ?? channel.IntegrationId, channel.PhoneNumber, channel.NormalizedPhoneNumber);
            }
        }
        // Migration fallback is deliberately constrained to the integration key and To number.
        var legacy = await db.WhatsAppIntegrations.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.IntegrationKey == integrationKey && x.Status == WhatsAppIntegrationStatus.Connected, cancellationToken);
        if (legacy is null || normalized is null || WhatsAppChannel.Normalize(legacy.WhatsAppFrom) != normalized) return null;
        return new(Guid.Empty, legacy.TenantId, legacy.Id, legacy.WhatsAppFrom, normalized);
    }

    public async Task<WhatsAppChannelResolution?> ResolveOutboundAsync(Guid tenantId, Guid? channelId, CancellationToken cancellationToken)
    {
        if (_options.MultiTenantChannelEnabled && channelId.HasValue)
        {
            var channel = await db.WhatsAppChannels.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == channelId && x.TenantId == tenantId && x.Status == WhatsAppChannelStatus.Active, cancellationToken);
            if (channel is not null) return new(channel.Id, tenantId, channel.IntegrationId, channel.PhoneNumber, channel.NormalizedPhoneNumber);
        }
        var legacy = await db.WhatsAppIntegrations.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.Status == WhatsAppIntegrationStatus.Connected).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);
        return legacy is null ? null : new(Guid.Empty, tenantId, legacy.Id, legacy.WhatsAppFrom, WhatsAppChannel.Normalize(legacy.WhatsAppFrom));
    }
}
