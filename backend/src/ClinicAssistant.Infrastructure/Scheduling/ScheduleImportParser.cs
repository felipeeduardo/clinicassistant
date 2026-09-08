using System.Data;
using System.Globalization;
using System.Text;
using ExcelDataReader;
using ClinicAssistant.Contracts.Scheduling;

namespace ClinicAssistant.Infrastructure.Scheduling;

/// <summary>Reads the controlled agenda template without persisting anything.</summary>
public sealed class ScheduleImportParser
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private const int MaxRows = 5000;
    private static readonly string[] RequiredHeaders = ["professional_registration", "record_type"];
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["registro_profissional"] = "professional_registration", ["registro profissional"] = "professional_registration", ["profissional"] = "professional_registration",
        ["tipo_registro"] = "record_type", ["tipo registro"] = "record_type", ["tipo"] = "record_type",
        ["dia_semana"] = "day_of_week", ["dia da semana"] = "day_of_week", ["dia"] = "day_of_week",
        ["data_inicial"] = "start_date", ["data inicial"] = "start_date",
        ["data_final"] = "end_date", ["data final"] = "end_date",
        ["hora_inicio"] = "start_time", ["hora inicial"] = "start_time", ["início"] = "start_time", ["inicio"] = "start_time",
        ["hora_fim"] = "end_time", ["hora final"] = "end_time", ["fim"] = "end_time",
        ["duracao_minutos"] = "slot_duration_minutes", ["duração (min)"] = "slot_duration_minutes", ["duração_minutos"] = "slot_duration_minutes", ["duracao"] = "slot_duration_minutes",
        ["motivo"] = "reason"
    };

    public static async Task<ScheduleImportPreview> ParseAsync(Stream source, CancellationToken cancellationToken = default)
    {
        if (source.Length > MaxBytes) throw new InvalidOperationException("The XLSX file exceeds the 5 MB limit.");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;
        using var reader = ExcelReaderFactory.CreateReader(memory);
        var data = reader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
        var table = data.Tables.Cast<DataTable>().FirstOrDefault(x => string.Equals(x.TableName, "agenda", StringComparison.OrdinalIgnoreCase)) ?? data.Tables[0];
        foreach (DataColumn column in table.Columns)
        {
            var normalized = column.ColumnName.Trim().ToLowerInvariant();
            if (HeaderAliases.TryGetValue(normalized, out var canonical)) column.ColumnName = canonical;
        }
        var headers = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName.Trim().ToLowerInvariant()).ToArray();
        var missing = RequiredHeaders.Where(x => !headers.Contains(x, StringComparer.Ordinal)).Select(x => new ScheduleImportValidationError(1, x, "Cabeçalho obrigatório ausente.")).ToList();
        if (missing.Count > 0) return new(0, 0, [], missing);
        if (table.Rows.Count > MaxRows) throw new InvalidOperationException("The XLSX file exceeds the 5,000 row limit.");
        var rows = new List<ScheduleImportRow>(table.Rows.Count);
        var errors = new List<ScheduleImportValidationError>();
        for (var index = 0; index < table.Rows.Count; index++)
        {
            var rowNumber = index + 2;
            var row = table.Rows[index];
            var item = new ScheduleImportRow(Value(row, "professional_registration"), NormalizeRecordType(Value(row, "record_type")), NormalizeDay(NullValue(row, "day_of_week")), TimeValue(row, "start_time"), TimeValue(row, "end_time"), IntValue(row, "slot_duration_minutes"), NullValue(row, "reason"), DateValue(row, "start_date"), DateValue(row, "end_date"));
            item = NormalizeDateRange(item);
            rows.Add(item);
            Validate(item, rowNumber, errors);
        }
        return new(table.Rows.Count, table.Rows.Count - errors.Select(x => x.Row).Distinct().Count(), rows, errors);
    }

    private static void Validate(ScheduleImportRow row, int line, List<ScheduleImportValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(row.ProfessionalRegistration)) errors.Add(new(line, "professional_registration", "Profissional é obrigatório."));
        if (!new[] { "availability_rule", "schedule_block", "vacation" }.Contains(row.RecordType, StringComparer.OrdinalIgnoreCase)) errors.Add(new(line, "record_type", "Escolha Disponibilidade, Bloqueio ou Férias."));
        if (row.RecordType.Equals("availability_rule", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(row.DayOfWeek)) errors.Add(new(line, "day_of_week", "Informe o dia da semana para uma disponibilidade."));
            if (string.IsNullOrWhiteSpace(row.StartTime)) errors.Add(new(line, "start_time", "Informe a hora inicial no formato HH:mm."));
            if (string.IsNullOrWhiteSpace(row.EndTime)) errors.Add(new(line, "end_time", "Informe a hora final no formato HH:mm."));
            if (row.SlotDurationMinutes is < 5 or > 240 or null) errors.Add(new(line, "slot_duration_minutes", "A duração deve estar entre 5 e 240 minutos."));
        }
        if (row.RecordType is "schedule_block" or "vacation")
        {
            if (string.IsNullOrWhiteSpace(row.StartDate)) errors.Add(new(line, "start_date", "Informe a data inicial no formato dd/MM/aaaa."));
            if (string.IsNullOrWhiteSpace(row.EndDate)) errors.Add(new(line, "end_date", "Informe a data final no formato dd/MM/aaaa."));
            if (row.RecordType == "schedule_block" && string.IsNullOrWhiteSpace(row.StartTime)) errors.Add(new(line, "start_time", "Informe a hora inicial no formato HH:mm."));
            if (row.RecordType == "schedule_block" && string.IsNullOrWhiteSpace(row.EndTime)) errors.Add(new(line, "end_time", "Informe a hora final no formato HH:mm."));
        }
        if (!string.IsNullOrWhiteSpace(row.StartTime) && !TimeOnly.TryParse(row.StartTime, CultureInfo.InvariantCulture, out _)) errors.Add(new(line, "start_time", "Hora inválida. Use HH:mm, por exemplo 08:00."));
        if (!string.IsNullOrWhiteSpace(row.EndTime) && !TimeOnly.TryParse(row.EndTime, CultureInfo.InvariantCulture, out _)) errors.Add(new(line, "end_time", "Hora inválida. Use HH:mm, por exemplo 17:30."));
        if (!string.IsNullOrWhiteSpace(row.StartDate) && !DateOnly.TryParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) errors.Add(new(line, "start_date", "Data inválida. Use dd/MM/aaaa, por exemplo 15/01/2027."));
        if (!string.IsNullOrWhiteSpace(row.EndDate) && !DateOnly.TryParseExact(row.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) errors.Add(new(line, "end_date", "Data inválida. Use dd/MM/aaaa, por exemplo 15/01/2027."));
        if (DateOnly.TryParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) && DateOnly.TryParseExact(row.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate) && endDate < startDate)
            errors.Add(new(line, "end_date", "A data final deve ser igual ou posterior à data inicial."));
        if (TimeOnly.TryParse(row.StartTime, CultureInfo.InvariantCulture, out var start) && TimeOnly.TryParse(row.EndTime, CultureInfo.InvariantCulture, out var end) && end <= start && row.RecordType == "schedule_block")
            errors.Add(new(line, "end_time", "A hora final deve ser posterior à hora inicial."));
    }

    private static string Value(DataRow row, string name) => row.Table.Columns.Contains(name) ? row[name]?.ToString()?.Trim() ?? string.Empty : string.Empty;
    private static string NormalizeRecordType(string value) => RemoveDiacritics(value).Trim().ToLowerInvariant() switch { "regra_disponibilidade" or "disponibilidade" or "availability_rule" => "availability_rule", "bloqueio" or "schedule_block" => "schedule_block", "ferias" or "vacation" => "vacation", _ => value.Trim().ToLowerInvariant() };
    private static string? NormalizeDay(string? value) => RemoveDiacritics(value ?? string.Empty).Trim().ToLowerInvariant() switch { "domingo" => "Sunday", "segunda" or "segunda-feira" => "Monday", "terca" or "terca-feira" => "Tuesday", "quarta" or "quarta-feira" => "Wednesday", "quinta" or "quinta-feira" => "Thursday", "sexta" or "sexta-feira" => "Friday", "sabado" or "sabado-feira" => "Saturday", _ => value };
    private static string? NullValue(DataRow row, string name) => string.IsNullOrWhiteSpace(Value(row, name)) ? null : Value(row, name);
    private static string? DateValue(DataRow row, string name)
    {
        if (!row.Table.Columns.Contains(name)) return null;
        var raw = row[name];
        if (raw is DateTime dateTime) return dateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (raw is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (raw is double serial && serial > 1) return DateTime.FromOADate(serial).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var value = NullValue(row, name);
        if (DateTimeOffset.TryParse(value, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.AllowWhiteSpaces, out var parsedWithOffset)) return parsedWithOffset.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (DateOnly.TryParse(value, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out var parsed)) return parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        return value;
    }
    private static string? TimeValue(DataRow row, string name)
    {
        if (!row.Table.Columns.Contains(name)) return null;
        var raw = row[name];
        if (raw is DateTime dateTime) return dateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        if (raw is double serial && serial >= 0 && serial < 1) return DateTime.FromOADate(serial).ToString("HH:mm", CultureInfo.InvariantCulture);
        var value = NullValue(row, name);
        return TimeOnly.TryParse(value, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out var parsed) ? parsed.ToString("HH:mm", CultureInfo.InvariantCulture) : value;
    }
    private static int? IntValue(DataRow row, string name) => int.TryParse(Value(row, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
    private static ScheduleImportRow NormalizeDateRange(ScheduleImportRow row)
    {
        if (row.RecordType is not ("schedule_block" or "vacation") || !DateOnly.TryParseExact(row.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) || !DateOnly.TryParseExact(row.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end) || start <= end) return row;
        // Some Excel readers interpret an ambiguous dd/MM value as MM/dd. Correct it only when swapping makes the range chronological.
        if (start.Day <= 12 && start.Month <= 12)
        {
            var swapped = new DateOnly(start.Year, start.Day, start.Month);
            if (swapped <= end) return row with { StartDate = swapped.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) };
        }
        return row;
    }
    private static string RemoveDiacritics(string value) => new string(value.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Normalize(NormalizationForm.FormC);
}
