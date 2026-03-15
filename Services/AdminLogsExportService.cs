using DarkMarket.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DarkMarket.Services;

public class AdminLogsExportService
{
    public string BuildCsv(IEnumerable<AppLog> rows)
    {
        var csv = new StringBuilder();
        csv.AppendLine("TimestampUtc,Level,Source,UserId,UserName,Message,Exception");

        foreach (var row in rows)
        {
            csv
                .Append(EscapeCsv(row.Timestamp.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                .Append(EscapeCsv(row.Level)).Append(',')
                .Append(EscapeCsv(row.Source)).Append(',')
                .Append(EscapeCsv(row.UserId)).Append(',')
                .Append(EscapeCsv(row.User?.UserName)).Append(',')
                .Append(EscapeCsv(row.Message)).Append(',')
                .Append(EscapeCsv(row.Exception))
                .AppendLine();
        }

        return csv.ToString();
    }

    public string BuildJson(IEnumerable<AppLog> rows)
    {
        var payload = rows.Select(row => new
        {
            timestampUtc = row.Timestamp.ToString("O", CultureInfo.InvariantCulture),
            row.Level,
            row.Source,
            row.UserId,
            userName = row.User?.UserName,
            row.Message,
            row.Exception
        });

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public string BuildExportFileName(
        string extension,
        string? filterLevel,
        string? filterSource,
        DateTime? filterStartDate,
        DateTime? filterEndDate,
        DateTime? utcNow = null)
    {
        var suffixParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(filterLevel))
        {
            suffixParts.Add($"level-{NormalizeForFileName(filterLevel)}");
        }

        if (!string.IsNullOrWhiteSpace(filterSource))
        {
            suffixParts.Add($"source-{NormalizeForFileName(filterSource)}");
        }

        if (filterStartDate.HasValue)
        {
            suffixParts.Add($"from-{filterStartDate.Value:yyyyMMdd}");
        }

        if (filterEndDate.HasValue)
        {
            suffixParts.Add($"to-{filterEndDate.Value:yyyyMMdd}");
        }

        var suffix = suffixParts.Count > 0 ? "-" + string.Join("-", suffixParts) : "";
        var timestamp = (utcNow ?? DateTime.UtcNow).ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return $"admin-logs{suffix}-{timestamp}.{extension}";
    }

    private static string NormalizeForFileName(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        var normalized = new StringBuilder(trimmed.Length);

        foreach (var ch in trimmed)
        {
            if (char.IsLetterOrDigit(ch))
            {
                normalized.Append(ch);
            }
            else if (ch == '-' || ch == '_')
            {
                normalized.Append(ch);
            }
            else
            {
                normalized.Append('-');
            }
        }

        return normalized.ToString().Trim('-');
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}