using System.Net.Mail;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Leads;
using ClinicAssistant.Contracts.Leads;
using ClinicAssistant.Domain.Platform;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Leads;

public sealed class DemoLeadService(ClinicAssistantDbContext db, ITenantContext tenantContext) : IDemoLeadService
{
    private const string Source = "LandingDemoForm";
    private static readonly DemoLeadStatus[] Statuses = Enum.GetValues<DemoLeadStatus>();

    public async Task<bool> CreateAsync(CreateDemoLeadRequest request, CancellationToken cancellationToken)
    {
        // Honeypot submissions are acknowledged without persistence so bots cannot infer the filter.
        if (!string.IsNullOrWhiteSpace(request.Website)) return true;
        var fullName = Require(request.FullName, "Nome completo", 200);
        var company = Require(request.CompanyOrClinicName, "Clínica ou empresa", 200);
        var email = NormalizeEmail(request.Email);
        var phone = Require(request.Phone, "Telefone", 40);
        var description = Optional(request.Description, 2000);
        var lead = new DemoLead(fullName, company, email, phone, description, Source,
            Optional(request.UtmSource, 100), Optional(request.UtmMedium, 100), Optional(request.UtmCampaign, 200),
            Optional(request.UtmContent, 200), Optional(request.UtmTerm, 200), Optional(request.LandingPage, 500), Optional(request.Referrer, 1000));
        db.DemoLeads.Add(lead);
        db.AuditRecords.Add(new AuditRecord(null, null, "demo_lead.created", "DemoLead", lead.Id, "Succeeded", Source));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DemoLeadPage> SearchAsync(DemoLeadListQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var source = db.DemoLeads.AsNoTracking().IgnoreQueryFilters();
        if (TryStatus(query.Status, out var status)) source = source.Where(x => x.Status == status);
        if (query.AssignedToUserId.HasValue) source = source.Where(x => x.AssignedToUserId == query.AssignedToUserId);
        if (!string.IsNullOrWhiteSpace(query.UtmSource)) source = source.Where(x => x.UtmSource == query.UtmSource.Trim());
        if (query.From.HasValue) source = source.Where(x => x.CreatedAt >= query.From.Value.ToUniversalTime());
        if (query.To.HasValue) source = source.Where(x => x.CreatedAt < query.To.Value.ToUniversalTime());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.FullName.Contains(search) || x.CompanyOrClinicName.Contains(search) || x.Email.Contains(search) || x.Phone.Contains(search));
        }
        var total = await source.CountAsync(cancellationToken);
        var entities = await source.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = entities.Select(ToItem).ToList();
        return new(items, page, pageSize, total);
    }

    public async Task<DemoLeadDetail?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.DemoLeads.AsNoTracking().IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null) return null;
        var audit = await (from item in db.AuditRecords.AsNoTracking()
                           join actor in db.Users.IgnoreQueryFilters() on item.ActorUserId equals actor.Id into actors
                           from actor in actors.DefaultIfEmpty()
                           where item.ResourceType == "DemoLead" && item.ResourceId == id
                           orderby item.CreatedAt descending
                           select new DemoLeadNote(item.CreatedAt, item.ActorUserId, actor == null ? null : actor.Name, item.Details)).ToListAsync(cancellationToken);
        return new(ToItem(lead), lead.Description, audit.Where(x => x.Note.StartsWith("note:", StringComparison.Ordinal)).Select(x => x with { Note = x.Note[5..].Trim() }).ToList(), audit);
    }

    public async Task<DemoLeadSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await db.DemoLeads.AsNoTracking().IgnoreQueryFilters().GroupBy(x => x.Status).Select(x => new { Status = x.Key, Count = x.Count() }).ToListAsync(cancellationToken);
        int Count(DemoLeadStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new(Count(DemoLeadStatus.New), Count(DemoLeadStatus.Contacted), Count(DemoLeadStatus.Qualified), Count(DemoLeadStatus.DemoScheduled), Count(DemoLeadStatus.Won), Count(DemoLeadStatus.Lost), Count(DemoLeadStatus.Archived), counts.Sum(x => x.Count), Count(DemoLeadStatus.DemoCompleted), Count(DemoLeadStatus.Pilot));
    }

    public async Task UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken)
    {
        if (!TryStatus(status, out var parsed)) throw new InvalidOperationException("Status de lead inválido.");
        var lead = await Find(id, cancellationToken);
        lead.ChangeStatus(parsed);
        var action = parsed switch
        {
            DemoLeadStatus.Contacted => "demo_lead.contacted",
            DemoLeadStatus.DemoScheduled => "demo_lead.demo_scheduled",
            DemoLeadStatus.DemoCompleted => "demo_lead.demo_completed",
            DemoLeadStatus.Pilot => "demo_lead.pilot_started",
            DemoLeadStatus.Won => "demo_lead.won",
            DemoLeadStatus.Lost => "demo_lead.lost",
            _ => "demo_lead.status_changed"
        };
        db.AuditRecords.Add(new AuditRecord(null, tenantContext.UserId, action, "DemoLead", id, "Succeeded", parsed.ToString()));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        if (userId.HasValue && !await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Id == userId && x.Role == Domain.Identity.UserRole.PlatformAdmin, cancellationToken))
            throw new KeyNotFoundException("Usuário PlatformAdmin não encontrado.");
        var lead = await Find(id, cancellationToken);
        lead.Assign(userId);
        db.AuditRecords.Add(new AuditRecord(null, tenantContext.UserId, "demo_lead.assigned", "DemoLead", id, "Succeeded", userId?.ToString() ?? "unassigned"));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddNoteAsync(Guid id, string note, CancellationToken cancellationToken)
    {
        _ = await Find(id, cancellationToken);
        var value = Require(note, "Observação", 2000);
        db.AuditRecords.Add(new AuditRecord(null, tenantContext.UserId, "demo_lead.note_added", "DemoLead", id, "Succeeded", $"note: {value}"));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetNextActionAsync(Guid id, UpdateDemoLeadNextActionRequest request, CancellationToken cancellationToken)
    {
        var lead = await Find(id, cancellationToken);
        var description = request.Description?.Trim();
        if (description is { Length: > 500 }) throw new InvalidOperationException("Próxima ação excede o limite permitido.");
        lead.SetNextAction(request.At, description);
        db.AuditRecords.Add(new AuditRecord(null, tenantContext.UserId, "demo_lead.next_action_changed", "DemoLead", id, "Succeeded", description ?? "cleared"));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DemoLead> Find(Guid id, CancellationToken ct) => await db.DemoLeads.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Lead não encontrado.");
    private static DemoLeadListItem ToItem(DemoLead x) => new(x.Id, x.FullName, x.CompanyOrClinicName, x.Email, x.Phone, x.Status.ToString(), x.Source, x.AssignedToUserId, x.CreatedAt, x.LastContactAt, x.NextActionAt, x.NextActionDescription, x.UtmSource, x.UtmMedium, x.UtmCampaign, x.UtmContent, x.UtmTerm, x.LandingPage, x.Referrer);
    private static bool TryStatus(string? value, out DemoLeadStatus status) => Enum.TryParse(value, true, out status) && Statuses.Contains(status);
    private static string Require(string? value, string field, int max) { var result = value?.Trim() ?? string.Empty; if (result.Length == 0) throw new InvalidOperationException($"{field} é obrigatório."); if (result.Length > max) throw new InvalidOperationException($"{field} excede o limite permitido."); return result; }
    private static string? Optional(string? value, int max) { var result = value?.Trim(); if (string.IsNullOrWhiteSpace(result)) return null; if (result.Length > max) throw new InvalidOperationException("Descrição excede o limite permitido."); return result; }
    private static string NormalizeEmail(string? value) { var email = Require(value, "E-mail", 320).ToLowerInvariant(); try { _ = new MailAddress(email); } catch (FormatException) { throw new InvalidOperationException("E-mail inválido."); } return email; }
}
