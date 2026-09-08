using System.Data;
using System.Globalization;
using System.Text.Json;
using ClinicAssistant.Application.Identity;
using ClinicAssistant.Application.Scheduling;
using ClinicAssistant.Application.Realtime;
using ClinicAssistant.Application.Operations;
using ClinicAssistant.Contracts.Scheduling;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.Operations;
using ClinicAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ClinicAssistant.Domain.WhatsApp;

namespace ClinicAssistant.Infrastructure.Scheduling;

public sealed class SchedulingService(ClinicAssistantDbContext db, TenantAccessGuard guard, IOperationalEventPublisher events, Microsoft.Extensions.Options.IOptions<AppointmentReminderOptions> reminderOptions) : ISchedulingService
{
    private readonly AppointmentReminderOptions _reminderOptions = reminderOptions.Value;
    public async Task<IReadOnlyList<PatientResponse>> GetPatientsAsync(CancellationToken ct) => await db.Patients.OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);
    public async Task<PatientPage> SearchPatientsAsync(PatientSearchRequest r, CancellationToken ct)
    {
        var page = Math.Max(1, r.Page); var pageSize = Math.Clamp(r.PageSize, 1, 100);
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(r.Search)) { var term = r.Search.Trim(); query = query.Where(x => EF.Functions.ILike(x.Name, $"%{term}%") || EF.Functions.ILike(x.Phone, $"%{term}%") || (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%"))); }
        if (!string.IsNullOrWhiteSpace(r.ConsentStatus) && Enum.TryParse<ConsentStatus>(r.ConsentStatus, true, out var consent)) query = query.Where(x => x.ConsentStatus == consent);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new PatientListItem(x.Id, x.Name, x.Phone, x.Email, x.BirthDate, x.ConsentStatus.ToString(), x.Source.ToString(), x.LastContactAt)).ToListAsync(ct);
        return new(items, page, pageSize, total);
    }
    public async Task<PatientDetailResponse> GetPatientDetailAsync(Guid id, CancellationToken ct)
    {
        var patient = await PatientById(id, ct); var tenantId = guard.RequireTenantId(); var now = DateTimeOffset.UtcNow;
        var appointments = await db.Appointments.Where(x => x.PatientId == id && x.StartsAt >= now && x.Status != AppointmentStatus.Cancelled).OrderBy(x => x.StartsAt).Take(20).Select(x => new PatientAppointmentSummary(x.Id, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString())).ToListAsync(ct);
        var conversations = await db.Conversations.Where(x => x.PatientId == id).OrderByDescending(x => x.LastMessageAt).Take(20).Select(x => new PatientConversationSummary(x.Id, x.Status.ToString(), x.AutomationMode.ToString(), x.LastMessageAt)).ToListAsync(ct);
        var audit = await db.AuditRecords.AsNoTracking().Where(x => x.TenantId == tenantId && x.ResourceType == "Patient" && x.ResourceId == id).OrderByDescending(x => x.CreatedAt).Take(20).Select(x => new PatientAuditSummary(x.CreatedAt, x.Action, x.Result)).ToListAsync(ct);
        return new(Map(patient), patient.Source.ToString(), patient.FirstContactAt, patient.LastContactAt, appointments, conversations, audit);
    }
    public async Task<PatientResponse> CreatePatientAsync(PatientRequest r, CancellationToken ct) { var tenantId = guard.RequireTenantId(); var p = new Patient(tenantId, r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); db.Patients.Add(p); db.AuditRecords.Add(new AuditRecord(tenantId, null, "patient.created", "Patient", p.Id, "Succeeded", "Patient created by clinic administration.")); await db.SaveChangesAsync(ct); await PublishAuditAsync(tenantId, "patient.created", "Patient", p.Id, ct); return Map(p); }
    public async Task<PatientResponse> UpdatePatientAsync(Guid id, PatientRequest r, CancellationToken ct) { var tenantId = guard.RequireTenantId(); var p = await PatientById(id, ct); p.Update(r.Name, r.Phone, r.Email, r.BirthDate, ParseConsent(r.ConsentStatus)); db.AuditRecords.Add(new AuditRecord(tenantId, null, "patient.updated", "Patient", p.Id, "Succeeded", "Patient updated by clinic administration.")); await db.SaveChangesAsync(ct); await PublishAuditAsync(tenantId, "patient.updated", "Patient", p.Id, ct); return Map(p); }
    public async Task AddAvailabilityRuleAsync(Guid professionalId, AvailabilityRuleRequest r, CancellationToken ct)
    {
        await RequireProfessional(professionalId, ct);
        ValidateAvailabilityRule(r);
        if (await db.AvailabilityRules.AnyAsync(x => x.ProfessionalId == professionalId && x.Active && x.DayOfWeek == r.DayOfWeek && x.StartTime < r.EndTime && x.EndTime > r.StartTime, ct))
            throw new SchedulingConflictException("Availability period overlaps an existing period.");
        db.AvailabilityRules.Add(new AvailabilityRule(guard.RequireTenantId(), professionalId, r.DayOfWeek, r.StartTime, r.EndTime, r.SlotDurationMinutes));
        await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<AvailabilityRuleResponse>> GetAvailabilityRulesAsync(Guid professionalId, CancellationToken ct) { await RequireProfessional(professionalId, ct); return await db.AvailabilityRules.Where(x => x.ProfessionalId == professionalId).OrderBy(x => x.DayOfWeek).Select(x => new AvailabilityRuleResponse(x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.SlotDurationMinutes, x.Active)).ToListAsync(ct); }
    public async Task<IReadOnlyList<AvailabilityRuleResponse>> ReplaceAvailabilityRulesAsync(Guid professionalId, IReadOnlyList<AvailabilityRuleRequest> r, CancellationToken ct)
    {
        await RequireProfessional(professionalId, ct);
        foreach (var rule in r) ValidateAvailabilityRule(rule);
        if (r.GroupBy(x => x.DayOfWeek).Any(day => day.OrderBy(x => x.StartTime).Zip(day.OrderBy(x => x.StartTime).Skip(1)).Any(pair => pair.First.EndTime > pair.Second.StartTime)))
            throw new SchedulingConflictException("Availability periods cannot overlap.");
        db.AvailabilityRules.RemoveRange(await db.AvailabilityRules.Where(x => x.ProfessionalId == professionalId).ToListAsync(ct));
        var rules = r.Select(x => new AvailabilityRule(guard.RequireTenantId(), professionalId, x.DayOfWeek, x.StartTime, x.EndTime, x.SlotDurationMinutes)).ToList();
        db.AvailabilityRules.AddRange(rules);
        await db.SaveChangesAsync(ct);
        return rules.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).Select(x => new AvailabilityRuleResponse(x.Id, x.DayOfWeek, x.StartTime, x.EndTime, x.SlotDurationMinutes, x.Active)).ToList();
    }
    public async Task AddScheduleBlockAsync(Guid professionalId, ScheduleBlockRequest r, CancellationToken ct) { await RequireProfessional(professionalId, ct); var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime(); if (end <= start) throw new InvalidOperationException("Block end must be after start."); if (await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == professionalId && x.StartsAt < end && x.EndsAt > start, ct)) throw new SchedulingConflictException("Block overlaps an existing block."); db.ScheduleBlocks.Add(new ScheduleBlock(guard.RequireTenantId(), professionalId, start, end, r.Reason)); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<ScheduleBlockResponse>> GetScheduleBlocksAsync(Guid professionalId, CancellationToken ct) { await RequireProfessional(professionalId, ct); return await db.ScheduleBlocks.Where(x => x.ProfessionalId == professionalId).OrderBy(x => x.StartsAt).Select(x => new ScheduleBlockResponse(x.Id, x.StartsAt, x.EndsAt, x.Reason)).ToListAsync(ct); }
    public async Task DeleteScheduleBlockAsync(Guid professionalId, Guid blockId, CancellationToken ct) { await RequireProfessional(professionalId, ct); var block = await db.ScheduleBlocks.SingleOrDefaultAsync(x => x.Id == blockId && x.ProfessionalId == professionalId, ct) ?? throw new KeyNotFoundException("Schedule block not found."); db.ScheduleBlocks.Remove(block); await db.SaveChangesAsync(ct); }
    public async Task<IReadOnlyList<VacationResponse>> GetVacationsAsync(Guid professionalId, CancellationToken ct) { await RequireProfessional(professionalId, ct); return await db.ProfessionalVacations.Where(x => x.ProfessionalId == professionalId).OrderBy(x => x.StartsAt).Select(x => new VacationResponse(x.Id, x.StartsAt, x.EndsAt, x.Reason)).ToListAsync(ct); }
    public async Task AddVacationAsync(Guid professionalId, VacationRequest r, CancellationToken ct) { await RequireProfessional(professionalId, ct); var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime(); if (end <= start) throw new InvalidOperationException("Vacation end must be after start."); if (await db.ProfessionalVacations.AnyAsync(x => x.ProfessionalId == professionalId && x.StartsAt < end && x.EndsAt > start, ct)) throw new SchedulingConflictException("Vacation overlaps an existing vacation."); if (await db.Appointments.AnyAsync(x => x.ProfessionalId == professionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct)) throw new SchedulingConflictException("Vacation conflicts with an existing appointment."); db.ProfessionalVacations.Add(new ProfessionalVacation(guard.RequireTenantId(), professionalId, start, end, r.Reason)); await db.SaveChangesAsync(ct); }
    public async Task DeleteVacationAsync(Guid professionalId, Guid vacationId, CancellationToken ct) { await RequireProfessional(professionalId, ct); var vacation = await db.ProfessionalVacations.SingleOrDefaultAsync(x => x.Id == vacationId && x.ProfessionalId == professionalId, ct) ?? throw new KeyNotFoundException("Vacation not found."); db.ProfessionalVacations.Remove(vacation); await db.SaveChangesAsync(ct); }
    public async Task<ProfessionalScheduleResponse> GetProfessionalScheduleAsync(Guid professionalId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct) { await RequireProfessional(professionalId, ct); if (endsAt <= startsAt) throw new InvalidOperationException("Schedule period is invalid."); var start = startsAt.ToUniversalTime(); var end = endsAt.ToUniversalTime(); var appointments = await db.Appointments.Where(x => x.ProfessionalId == professionalId && x.StartsAt < end && x.EndsAt > start).OrderBy(x => x.StartsAt).Select(x => new AppointmentListItem(x.Id, x.ClinicUnitId, x.ProfessionalId, x.SpecialtyId, x.PatientId, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString(), x.Notes)).ToListAsync(ct); var blocks = await db.ScheduleBlocks.Where(x => x.ProfessionalId == professionalId && x.StartsAt < end && x.EndsAt > start).OrderBy(x => x.StartsAt).Select(x => new ScheduleBlockResponse(x.Id, x.StartsAt, x.EndsAt, x.Reason)).ToListAsync(ct); var vacations = await db.ProfessionalVacations.Where(x => x.ProfessionalId == professionalId && x.StartsAt < end && x.EndsAt > start).OrderBy(x => x.StartsAt).Select(x => new VacationResponse(x.Id, x.StartsAt, x.EndsAt, x.Reason)).ToListAsync(ct); return new(appointments, blocks, vacations); }
    public async Task<ScheduleImportPreview> PreviewScheduleImportAsync(Stream source, string? fileName, CancellationToken ct)
    {
        using var activity = ScheduleImportTelemetry.ActivitySource.StartActivity("ScheduleImport.Preview");
        var preview = await ScheduleImportParser.ParseAsync(source, ct);
        var errors = preview.Errors.ToList();
        var warnings = (preview.Warnings ?? []).ToList();
        var registrations = preview.Rows.Select(x => x.ProfessionalRegistration).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var known = await db.Professionals.Where(x => registrations.Contains(x.RegistrationNumber) && x.Status == ClinicAssistant.Domain.Clinics.CatalogStatus.Active).Select(x => x.RegistrationNumber).ToListAsync(ct);
        var knownSet = known.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var row in preview.Rows.Select((item, index) => new { item, index = index + 2 }))
            if (!string.IsNullOrWhiteSpace(row.item.ProfessionalRegistration) && !knownSet.Contains(row.item.ProfessionalRegistration))
                errors.Add(new(row.index, "professional_registration", "Não encontramos um profissional ativo com esse registro."));
        var clinicTimeZone = await db.Clinics.Where(x => x.TenantId == guard.RequireTenantId()).Select(x => x.TimeZone).SingleOrDefaultAsync(ct) ?? "UTC";
        var professionalIds = await db.Professionals.Where(x => registrations.Contains(x.RegistrationNumber) && x.Status == ClinicAssistant.Domain.Clinics.CatalogStatus.Active).ToDictionaryAsync(x => x.RegistrationNumber, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);
        var acceptedAvailabilityRows = new List<(int Row, Guid ProfessionalId, DayOfWeek Day, TimeOnly Start, TimeOnly End)>();
        foreach (var row in preview.Rows.Select((item, index) => new { item, index = index + 2 }))
        {
            if (row.item.RecordType != "availability_rule" || !professionalIds.TryGetValue(row.item.ProfessionalRegistration, out var professionalId) ||
                !Enum.TryParse<DayOfWeek>(row.item.DayOfWeek, true, out var day) ||
                !TimeOnly.TryParse(row.item.StartTime, CultureInfo.InvariantCulture, out var startTime) ||
                !TimeOnly.TryParse(row.item.EndTime, CultureInfo.InvariantCulture, out var endTime)) continue;

            if (await db.AvailabilityRules.AnyAsync(x => x.ProfessionalId == professionalId && x.Active && x.DayOfWeek == day && x.StartTime < endTime && x.EndTime > startTime, ct))
            {
                errors.Add(new(row.index, "availability", $"Já existe uma disponibilidade sobreposta para {row.item.ProfessionalRegistration} neste dia e horário."));
                continue;
            }

            var duplicate = acceptedAvailabilityRows.FirstOrDefault(x => x.ProfessionalId == professionalId && x.Day == day && x.Start < endTime && x.End > startTime);
            if (duplicate != default)
            {
                errors.Add(new(row.index, "availability", $"Esta disponibilidade sobrepõe a linha {duplicate.Row} da própria planilha."));
                continue;
            }

            acceptedAvailabilityRows.Add((row.index, professionalId, day, startTime, endTime));
        }
        foreach (var row in preview.Rows.Select((item, index) => new { item, index = index + 2 }))
        {
            if (!professionalIds.TryGetValue(row.item.ProfessionalRegistration, out var professionalId) || row.item.RecordType == "availability_rule" || !DateOnly.TryParseExact(row.item.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) || !DateOnly.TryParseExact(row.item.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate)) continue;
            var start = ToUtc(startDate, TimeOnly.TryParse(row.item.StartTime, CultureInfo.InvariantCulture, out var parsedStart) ? parsedStart : TimeOnly.MinValue, clinicTimeZone);
            var end = ToUtc(endDate, TimeOnly.TryParse(row.item.EndTime, CultureInfo.InvariantCulture, out var parsedEnd) ? parsedEnd : new TimeOnly(23, 59), clinicTimeZone);
            if (await db.Appointments.AnyAsync(x => x.ProfessionalId == professionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct)) warnings.Add(new(row.index, "data", "Há uma consulta existente neste período. A consulta não será alterada."));
        }
        var result = preview with { Errors = errors, ValidRows = preview.TotalRows - errors.Select(x => x.Row).Distinct().Count(), Warnings = warnings, FileName = fileName };
        var tenantId = guard.RequireTenantId();
        db.AuditRecords.Add(new AuditRecord(tenantId, null, "schedule.import.validated", "ScheduleImport", null, result.Errors.Count == 0 ? "Succeeded" : "Failed", $"Validated {result.TotalRows} row(s); {result.Errors.Count} error(s)."));
        await db.SaveChangesAsync(ct);
        activity?.SetTag("import.type", "schedule"); activity?.SetTag("rows.total", result.TotalRows); activity?.SetTag("rows.valid", result.ValidRows); activity?.SetTag("rows.error", result.Errors.Count);
        return result;
    }
    public async Task<ScheduleImportResult> ImportScheduleAsync(Stream source, string idempotencyKey, CancellationToken ct)
    {
        using var activity = ScheduleImportTelemetry.ActivitySource.StartActivity("ScheduleImport.Commit");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required.");
        var tenantId = guard.RequireTenantId();
        var scope = $"schedule.import:{tenantId}";
        var prior = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct);
        if (prior is not null) return JsonSerializer.Deserialize<ScheduleImportResult>(prior.ResponseJson)! with { Replayed = true };
        var preview = await ScheduleImportParser.ParseAsync(source, ct);
        if (preview.Errors.Count > 0) throw new InvalidOperationException($"Importação inválida: {preview.Errors.Count} erro(s) encontrado(s).");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var clinicTimeZone = await db.Clinics.Where(x => x.TenantId == tenantId).Select(x => x.TimeZone).SingleOrDefaultAsync(ct) ?? "UTC";
        var rules = new List<AvailabilityRule>(); var blocks = new List<ScheduleBlock>(); var vacations = new List<ProfessionalVacation>();
        var importLine = 2;
        foreach (var row in preview.Rows)
        {
            var rowNumber = importLine++;
            var professional = await db.Professionals.SingleOrDefaultAsync(x => x.RegistrationNumber == row.ProfessionalRegistration && x.Status == ClinicAssistant.Domain.Clinics.CatalogStatus.Active, ct) ?? throw new InvalidOperationException($"Profissional não encontrado na linha de importação: {row.ProfessionalRegistration}.");
            if (row.RecordType.Equals("availability_rule", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<DayOfWeek>(row.DayOfWeek, true, out var day) || !TimeOnly.TryParse(row.StartTime, CultureInfo.InvariantCulture, out var startTime) || !TimeOnly.TryParse(row.EndTime, CultureInfo.InvariantCulture, out var endTime) || row.SlotDurationMinutes is null) throw new InvalidOperationException("Disponibilidade inválida.");
                if (await db.AvailabilityRules.AnyAsync(x => x.ProfessionalId == professional.Id && x.Active && x.DayOfWeek == day && x.StartTime < endTime && x.EndTime > startTime, ct) || rules.Any(x => x.ProfessionalId == professional.Id && x.DayOfWeek == day && x.StartTime < endTime && x.EndTime > startTime)) throw new SchedulingConflictException($"Linha {rowNumber}: a disponibilidade de {professional.RegistrationNumber} sobrepõe outra regra recorrente.");
                rules.Add(new AvailabilityRule(tenantId, professional.Id, day, startTime, endTime, row.SlotDurationMinutes.Value));
            }
            else
            {
                if (!DateOnly.TryParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) || !DateOnly.TryParseExact(row.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate)) throw new InvalidOperationException("Data inválida. Use dd/MM/aaaa.");
                var startTime = TimeOnly.TryParse(row.StartTime, CultureInfo.InvariantCulture, out var parsedStart) ? parsedStart : TimeOnly.MinValue;
                var endTime = TimeOnly.TryParse(row.EndTime, CultureInfo.InvariantCulture, out var parsedEnd) ? parsedEnd : new TimeOnly(23, 59);
                var start = ToUtc(startDate, startTime, clinicTimeZone);
                var end = ToUtc(endDate, endTime, clinicTimeZone);
                if (end <= start) throw new InvalidOperationException("O período informado é inválido.");
                start = start.ToUniversalTime(); end = end.ToUniversalTime();
                if (await db.Appointments.AnyAsync(x => x.ProfessionalId == professional.Id && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct)) throw new SchedulingConflictException("Importação conflita com uma consulta existente.");
                if (row.RecordType.Equals("schedule_block", StringComparison.OrdinalIgnoreCase)) { if (await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == professional.Id && x.StartsAt < end && x.EndsAt > start, ct) || blocks.Any(x => x.ProfessionalId == professional.Id && x.StartsAt < end && x.EndsAt > start)) throw new SchedulingConflictException($"Linha {rowNumber}: o bloqueio sobrepõe outro bloqueio existente."); blocks.Add(new ScheduleBlock(tenantId, professional.Id, start, end, row.Reason)); }
                else
                {
                    if (await db.ProfessionalVacations.AnyAsync(x => x.ProfessionalId == professional.Id && x.StartsAt < end && x.EndsAt > start, ct) || vacations.Any(x => x.ProfessionalId == professional.Id && x.StartsAt < end && x.EndsAt > start))
                        throw new SchedulingConflictException($"Linha {rowNumber}: o período de férias sobrepõe férias já cadastradas.");
                    vacations.Add(new ProfessionalVacation(tenantId, professional.Id, start, end, row.Reason));
                }
            }
        }
        db.AddRange(rules); db.AddRange(blocks); db.AddRange(vacations);
        var result = new ScheduleImportResult(rules.Count, blocks.Count, vacations.Count, false);
        activity?.SetTag("import.type", "schedule"); activity?.SetTag("rows.total", preview.TotalRows); activity?.SetTag("rows.valid", preview.ValidRows); activity?.SetTag("rows.error", preview.Errors.Count);
        db.AddRange(new AuditRecord(tenantId, null, "schedule.imported", "ScheduleImport", null, "Succeeded", $"Created {rules.Count} rule(s), {blocks.Count} block(s), {vacations.Count} vacation(s)."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(result)));
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return result;
    }
    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time, string timeZoneId)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { zone = TimeZoneInfo.Utc; }
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone));
    }
    public async Task<IReadOnlyList<AvailableSlot>> GetAvailabilityAsync(Guid professionalId, DateOnly appointmentDate, CancellationToken ct)
    {
        await RequireProfessional(professionalId, ct); var rules = await db.AvailabilityRules.Where(x => x.ProfessionalId == professionalId && x.Active && x.DayOfWeek == appointmentDate.DayOfWeek).ToListAsync(ct);
        var dayStart = new DateTimeOffset(appointmentDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); var dayEnd = dayStart.AddDays(1);
        var busy = await db.Appointments.Where(x => x.ProfessionalId == professionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < dayEnd && x.EndsAt > dayStart).Select(x => new { x.StartsAt, x.EndsAt }).ToListAsync(ct);
        var blocks = await db.ScheduleBlocks.Where(x => x.ProfessionalId == professionalId && x.StartsAt < dayEnd && x.EndsAt > dayStart).Select(x => new { x.StartsAt, x.EndsAt }).ToListAsync(ct);
        var vacations = await db.ProfessionalVacations.Where(x => x.ProfessionalId == professionalId && x.StartsAt < dayEnd && x.EndsAt > dayStart).Select(x => new { x.StartsAt, x.EndsAt }).ToListAsync(ct);
        var slots = new List<AvailableSlot>(); foreach (var rule in rules) for (var start = new DateTimeOffset(appointmentDate.ToDateTime(rule.StartTime), TimeSpan.Zero); start.AddMinutes(rule.SlotDurationMinutes) <= new DateTimeOffset(appointmentDate.ToDateTime(rule.EndTime), TimeSpan.Zero); start = start.AddMinutes(rule.SlotDurationMinutes)) { var end = start.AddMinutes(rule.SlotDurationMinutes); if (!busy.Concat(blocks).Concat(vacations).Any(x => x.StartsAt < end && x.EndsAt > start)) slots.Add(new(start, end)); } return slots;
    }
    public async Task<IReadOnlyList<AppointmentListItem>> GetAppointmentsAsync(DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct)
    {
        if (endsAt <= startsAt) throw new InvalidOperationException("Appointment period is invalid.");
        return await db.Appointments.Where(x => x.StartsAt < endsAt.ToUniversalTime() && x.EndsAt > startsAt.ToUniversalTime()).OrderBy(x => x.StartsAt).Select(x => new AppointmentListItem(x.Id, x.ClinicUnitId, x.ProfessionalId, x.SpecialtyId, x.PatientId, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString(), x.Notes)).ToListAsync(ct);
    }
    public async Task<AppointmentPage> SearchAppointmentsAsync(AppointmentSearchRequest r, CancellationToken ct)
    {
        var page = Math.Max(1, r.Page); var pageSize = Math.Clamp(r.PageSize, 1, 100); var query = db.Appointments.AsNoTracking();
        if (r.ProfessionalId.HasValue) query = query.Where(x => x.ProfessionalId == r.ProfessionalId); if (r.SpecialtyId.HasValue) query = query.Where(x => x.SpecialtyId == r.SpecialtyId); if (r.UnitId.HasValue) query = query.Where(x => x.ClinicUnitId == r.UnitId); if (r.PatientId.HasValue) query = query.Where(x => x.PatientId == r.PatientId);
        if (!string.IsNullOrWhiteSpace(r.Status) && Enum.TryParse<AppointmentStatus>(r.Status, true, out var status)) query = query.Where(x => x.Status == status);
        else query = query.Where(x => x.Status != AppointmentStatus.Rescheduled);
        if (!string.IsNullOrWhiteSpace(r.Source) && Enum.TryParse<AppointmentSource>(r.Source, true, out var source)) query = query.Where(x => x.Source == source);
        if (r.From.HasValue) query = query.Where(x => x.StartsAt >= r.From.Value.ToUniversalTime()); if (r.To.HasValue) query = query.Where(x => x.StartsAt < r.To.Value.ToUniversalTime());
        var total = await query.CountAsync(ct); var ordered = string.Equals(r.Sort, "startsAt:desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(x => x.StartsAt) : query.OrderBy(x => x.StartsAt);
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new AppointmentListItem(x.Id, x.ClinicUnitId, x.ProfessionalId, x.SpecialtyId, x.PatientId, x.StartsAt, x.EndsAt, x.Status.ToString(), x.Source.ToString(), x.Notes)).ToListAsync(ct); return new(items, page, pageSize, total);
    }
    public async Task<AppointmentDetailResponse> GetAppointmentDetailAsync(Guid id, CancellationToken ct)
    {
        var result = await (from appointment in db.Appointments where appointment.Id == id join patient in db.Patients on appointment.PatientId equals patient.Id join professional in db.Professionals on appointment.ProfessionalId equals professional.Id join unit in db.ClinicUnits on appointment.ClinicUnitId equals unit.Id join specialty in db.Specialties on appointment.SpecialtyId equals specialty.Id select new { appointment, patient.Name, ProfessionalName = professional.Name, UnitName = unit.Name, SpecialtyName = specialty.Name }).SingleOrDefaultAsync(ct) ?? throw new KeyNotFoundException("Appointment not found.");
        var a = result.appointment; return new(new AppointmentListItem(a.Id, a.ClinicUnitId, a.ProfessionalId, a.SpecialtyId, a.PatientId, a.StartsAt, a.EndsAt, a.Status.ToString(), a.Source.ToString(), a.Notes), result.Name, result.ProfessionalName, result.UnitName, result.SpecialtyName, a.CancelledAt, a.CancellationReason, a.Version);
    }
    public async Task<AppointmentResponse> CreateAppointmentAsync(AppointmentRequest r, string idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required."); if (r.EndsAt <= r.StartsAt) throw new InvalidOperationException("Appointment end must be after start."); var tenant = guard.RequireTenantId(); var scope = $"appointment.create:{tenant}"; var prior = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct); if (prior is not null) return JsonSerializer.Deserialize<AppointmentResponse>(prior.ResponseJson) ?? throw new InvalidOperationException("Stored idempotency response is invalid."); await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await db.Professionals.AnyAsync(x => x.Id == r.ProfessionalId && x.ClinicUnitId == r.ClinicUnitId, ct) || !await db.Patients.AnyAsync(x => x.Id == r.PatientId, ct) || !await db.Specialties.AnyAsync(x => x.Id == r.SpecialtyId, ct)) throw new KeyNotFoundException("Appointment references are invalid.");
        var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime(); var conflict = await db.Appointments.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.Status != AppointmentStatus.Cancelled && x.StartsAt < end && x.EndsAt > start, ct) || await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct) || await db.ProfessionalVacations.AnyAsync(x => x.ProfessionalId == r.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct); if (conflict) { OperationalTelemetry.AppointmentConflicts.Add(1); throw new SchedulingConflictException("The selected slot is no longer available."); }
        var a = new Appointment(tenant, r.ClinicUnitId, r.ProfessionalId, r.SpecialtyId, r.PatientId, start, end, Enum.Parse<AppointmentSource>(r.Source, true), r.Notes); var response = Map(a); db.Add(a); ScheduleReminders(a, tenant); db.AddRange(new AuditRecord(tenant, null, "appointment.created", "Appointment", a.Id, "Succeeded", "Appointment created."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response))); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); await events.PublishAsync(tenant, "appointment.created", new { a.Id }, ct); await PublishAuditAsync(tenant, "appointment.created", "Appointment", a.Id, ct); return response;
    }
    public async Task<AppointmentResponse> ConfirmAsync(Guid id, AppointmentOperationRequest r, string idempotencyKey, CancellationToken ct) { if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required."); var scope = $"appointment.confirm:{id}"; var prior = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct); if (prior is not null) return JsonSerializer.Deserialize<AppointmentResponse>(prior.ResponseJson) ?? throw new InvalidOperationException("Stored idempotency response is invalid."); var tenantId = guard.RequireTenantId(); var a = await AppointmentById(id, ct); if (a.Version != r.ExpectedVersion) throw new SchedulingConflictException("The appointment was changed by another operation."); a.Confirm(); var response = Map(a); db.AddRange(new AuditRecord(tenantId, null, "appointment.confirmed", "Appointment", id, "Succeeded", "Appointment confirmed."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response))); await db.SaveChangesAsync(ct); await events.PublishAsync(tenantId, "appointment.updated", new { a.Id }, ct); await PublishAuditAsync(tenantId, "appointment.confirmed", "Appointment", id, ct); return response; }
    public async Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, string idempotencyKey, CancellationToken ct) { if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required."); var scope = $"appointment.cancel:{id}"; var prior = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct); if (prior is not null) return JsonSerializer.Deserialize<AppointmentResponse>(prior.ResponseJson) ?? throw new InvalidOperationException("Stored idempotency response is invalid."); var tenantId = guard.RequireTenantId(); var a = await AppointmentById(id, ct); if (a.Version != r.ExpectedVersion) throw new SchedulingConflictException("The appointment was changed by another operation."); a.Cancel(r.Reason); await db.AppointmentReminders.Where(x => x.AppointmentId == id && x.Status != AppointmentReminderStatus.Sent).ForEachAsync(x => x.Cancel(), ct); var response = Map(a); db.AddRange(new AuditRecord(tenantId, null, "appointment.cancelled", "Appointment", id, "Succeeded", "Appointment cancelled."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response))); await db.SaveChangesAsync(ct); await events.PublishAsync(tenantId, "appointment.cancelled", new { a.Id }, ct); await PublishAuditAsync(tenantId, "appointment.cancelled", "Appointment", id, ct); return response; }
    public async Task<RescheduleAppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest r, string idempotencyKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new InvalidOperationException("Idempotency-Key is required."); if (r.EndsAt <= r.StartsAt) throw new InvalidOperationException("Appointment end must be after start.");
        var scope = $"appointment.reschedule:{id}"; var prior = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Scope == scope && x.Key == idempotencyKey, ct); if (prior is not null) return JsonSerializer.Deserialize<RescheduleAppointmentResponse>(prior.ResponseJson) ?? throw new InvalidOperationException("Stored idempotency response is invalid.");
        var tenantId = guard.RequireTenantId(); await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct); var original = await AppointmentById(id, ct); if (original.Version != r.ExpectedVersion) throw new SchedulingConflictException("The appointment was changed by another operation."); var start = r.StartsAt.ToUniversalTime(); var end = r.EndsAt.ToUniversalTime();
        var conflict = await db.Appointments.AnyAsync(x => x.ProfessionalId == original.ProfessionalId && x.Id != id && x.Status != AppointmentStatus.Cancelled && x.Status != AppointmentStatus.Rescheduled && x.StartsAt < end && x.EndsAt > start, ct) || await db.ScheduleBlocks.AnyAsync(x => x.ProfessionalId == original.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct) || await db.ProfessionalVacations.AnyAsync(x => x.ProfessionalId == original.ProfessionalId && x.StartsAt < end && x.EndsAt > start, ct); if (conflict) { OperationalTelemetry.AppointmentConflicts.Add(1); throw new SchedulingConflictException("The selected slot is no longer available."); }
        original.MarkRescheduled(); await db.AppointmentReminders.Where(x => x.AppointmentId == id && x.Status != AppointmentReminderStatus.Sent).ForEachAsync(x => x.Cancel(), ct); var replacement = new Appointment(tenantId, original.ClinicUnitId, original.ProfessionalId, original.SpecialtyId, original.PatientId, start, end, original.Source, r.Notes ?? original.Notes); var response = new RescheduleAppointmentResponse(Map(original), Map(replacement), false); db.Add(replacement); ScheduleReminders(replacement, tenantId); db.AddRange(new AuditRecord(tenantId, null, "appointment.rescheduled", "Appointment", id, "Succeeded", $"Replacement appointment {replacement.Id}."), new IdempotencyRecord(scope, idempotencyKey, JsonSerializer.Serialize(response))); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); OperationalTelemetry.AppointmentsRescheduled.Add(1); await events.PublishAsync(tenantId, "appointment.updated", new { OriginalId = original.Id, ReplacementId = replacement.Id }, ct); await PublishAuditAsync(tenantId, "appointment.rescheduled", "Appointment", id, ct); return response;
    }
    private void ScheduleReminders(Appointment appointment, Guid tenantId)
    {
        if (!_reminderOptions.Enabled) return;
        var now = DateTimeOffset.UtcNow;
        var channel = db.WhatsAppChannels.Where(x => x.TenantId == tenantId && x.Status == WhatsAppChannelStatus.Active && x.IsDefault).OrderByDescending(x => x.UpdatedAt).Select(x => (Guid?)x.Id).FirstOrDefault();
        if (_reminderOptions.DayBeforeEnabled) AddReminder(AppointmentReminderType.DayBefore, appointment.StartsAt, appointment.StartsAt.AddHours(-24), appointment, tenantId, channel, now);
        if (_reminderOptions.HourBeforeEnabled) AddReminder(AppointmentReminderType.HourBefore, appointment.StartsAt, appointment.StartsAt.AddHours(-1), appointment, tenantId, channel, now);
    }
    private void AddReminder(AppointmentReminderType type, DateTimeOffset start, DateTimeOffset scheduled, Appointment appointment, Guid tenantId, Guid? channel, DateTimeOffset now)
    { var reminder = new AppointmentReminder(tenantId, appointment.Id, channel, type, start.ToUniversalTime(), scheduled.ToUniversalTime(), $"appointment-reminder:{appointment.Id:N}:{type}:{start.UtcTicks}"); if (scheduled <= now) reminder.Skip(); db.AppointmentReminders.Add(reminder); }
    private async Task<Patient> PatientById(Guid id, CancellationToken ct) => await db.Patients.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Patient not found.");
    private async Task<Appointment> AppointmentById(Guid id, CancellationToken ct) => await db.Appointments.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Appointment not found.");
    private async Task RequireProfessional(Guid id, CancellationToken ct) { if (!await db.Professionals.AnyAsync(x => x.Id == id && x.Status == ClinicAssistant.Domain.Clinics.CatalogStatus.Active, ct)) throw new KeyNotFoundException("Active professional not found."); }
    private static void ValidateAvailabilityRule(AvailabilityRuleRequest rule)
    {
        if (rule.EndTime <= rule.StartTime || rule.SlotDurationMinutes is < 5 or > 240)
            throw new InvalidOperationException("Invalid availability rule.");
    }
    private Task PublishAuditAsync(Guid tenantId, string action, string resourceType, Guid resourceId, CancellationToken ct) => events.PublishAsync(tenantId, "audit.created", new { Action = action, ResourceType = resourceType, ResourceId = resourceId, Result = "Succeeded" }, ct);
    private static ConsentStatus ParseConsent(string value) => Enum.TryParse<ConsentStatus>(value, true, out var status) ? status : throw new InvalidOperationException("Invalid consent status.");
    private static PatientResponse Map(Patient x) => new(x.Id, x.Name, x.Phone, x.Email, x.BirthDate, x.ConsentStatus.ToString());
    private static AppointmentResponse Map(Appointment x) => new(x.Id, x.PatientId, x.ProfessionalId, x.StartsAt, x.EndsAt, x.Status.ToString());
}
