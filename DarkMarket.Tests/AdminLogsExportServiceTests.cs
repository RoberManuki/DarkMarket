using DarkMarket.Models;
using DarkMarket.Services;

namespace DarkMarket.Tests;

public class AdminLogsExportServiceTests
{
    [Fact]
    public void BuildCsv_WhenRowsProvided_ReturnsHeaderAndEscapedValues()
    {
        var service = new AdminLogsExportService();
        var rows = new[]
        {
            new AppLog
            {
                Timestamp = new DateTime(2026, 3, 15, 12, 34, 56, DateTimeKind.Utc),
                Level = "Info",
                Source = "AdminOrdersReview",
                UserId = "admin-1",
                User = new ApplicationUser { UserName = "admin\"name" },
                Message = "msg,with,comma",
                Exception = "err\"quoted"
            }
        };

        var csv = service.BuildCsv(rows);

        Assert.Contains("TimestampUtc,Level,Source,UserId,UserName,Message,Exception", csv);
        Assert.Contains("\"admin\"\"name\"", csv);
        Assert.Contains("\"msg,with,comma\"", csv);
        Assert.Contains("\"err\"\"quoted\"", csv);
    }

    [Fact]
    public void BuildJson_WhenRowsProvided_ReturnsIndentedPayloadWithFields()
    {
        var service = new AdminLogsExportService();
        var rows = new[]
        {
            new AppLog
            {
                Timestamp = new DateTime(2026, 3, 15, 12, 34, 56, DateTimeKind.Utc),
                Level = "Warning",
                Source = "Webhook",
                UserId = "user-1",
                User = new ApplicationUser { UserName = "john" },
                Message = "hello",
                Exception = null
            }
        };

        var json = service.BuildJson(rows);

        Assert.Contains("\"timestampUtc\"", json);
        Assert.Contains("\"Level\": \"Warning\"", json);
        Assert.Contains("\"Source\": \"Webhook\"", json);
        Assert.Contains("\"userName\": \"john\"", json);
    }

    [Fact]
    public void BuildExportFileName_WithFilters_ReturnsNormalizedAndDeterministicName()
    {
        var service = new AdminLogsExportService();

        var name = service.BuildExportFileName(
            extension: "csv",
            filterLevel: "Info",
            filterSource: "Admin Orders/Review",
            filterStartDate: new DateTime(2026, 3, 1),
            filterEndDate: new DateTime(2026, 3, 15),
            utcNow: new DateTime(2026, 3, 15, 1, 2, 3, DateTimeKind.Utc));

        Assert.Equal("admin-logs-level-info-source-admin-orders-review-from-20260301-to-20260315-20260315-010203.csv", name);
    }
}
