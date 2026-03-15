using DarkMarket.Data;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DarkMarket.Tests;

public class AdminLogsQueryServiceIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public AdminLogsQueryServiceIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPageDataAsync_ReturnsCountsAndSortedPage()
    {
        var marker = Guid.NewGuid().ToString("N");
        _ = await SeedLogAsync("AdminOrdersReview", $"msg {marker}", level: "Info", userId: $"user-b-{marker}", timestampUtc: new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc));
        _ = await SeedLogAsync("AdminOrdersReview", $"msg {marker}", level: "Warning", userId: $"user-c-{marker}", timestampUtc: new DateTime(2026, 3, 11, 8, 0, 0, DateTimeKind.Utc));
        _ = await SeedLogAsync("Webhook", $"msg {marker}", level: "Info", userId: $"user-a-{marker}", timestampUtc: new DateTime(2026, 3, 12, 8, 0, 0, DateTimeKind.Utc));

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AdminLogsQueryService>();

        var data = await service.GetPageDataAsync(
            primaryCriteria: new AdminLogFilterCriteria { GlobalTerm = marker },
            auditCountsCriteria: new AdminLogFilterCriteria { GlobalTerm = marker },
            sortColumn: AdminLogSortColumn.User,
            sortAscending: true,
            requestedPage: 1,
            pageSize: 2);

        Assert.Equal(3, data.TotalLogs);
        Assert.Equal(1, data.EffectivePage);
        Assert.Equal(2, data.Logs.Count);
        Assert.Equal(3, data.AuditCounts.All);
        Assert.Equal(2, data.AuditCounts.ReleaseOnly);
        Assert.Equal(1, data.AuditCounts.ReleaseSuccess);
        Assert.Equal(1, data.AuditCounts.ReleaseRefused);

        Assert.Contains($"user-a-{marker}", data.Logs[0].UserId);
        Assert.Contains($"user-b-{marker}", data.Logs[1].UserId);
    }

    [Fact]
    public async Task GetPageDataAsync_WhenRequestedPageIsTooHigh_ClampsToLastPage()
    {
        var marker = Guid.NewGuid().ToString("N");
        _ = await SeedLogAsync("AdminOrdersReview", $"msg {marker}", userId: $"user-1-{marker}", timestampUtc: new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc));
        _ = await SeedLogAsync("AdminOrdersReview", $"msg {marker}", userId: $"user-2-{marker}", timestampUtc: new DateTime(2026, 3, 11, 8, 0, 0, DateTimeKind.Utc));
        _ = await SeedLogAsync("AdminOrdersReview", $"msg {marker}", userId: $"user-3-{marker}", timestampUtc: new DateTime(2026, 3, 12, 8, 0, 0, DateTimeKind.Utc));

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AdminLogsQueryService>();

        var data = await service.GetPageDataAsync(
            primaryCriteria: new AdminLogFilterCriteria { GlobalTerm = marker },
            auditCountsCriteria: new AdminLogFilterCriteria { GlobalTerm = marker },
            sortColumn: AdminLogSortColumn.Timestamp,
            sortAscending: false,
            requestedPage: 99,
            pageSize: 2);

        Assert.Equal(3, data.TotalLogs);
        Assert.Equal(2, data.EffectivePage);
        Assert.Single(data.Logs);
    }

    private async Task<int> SeedLogAsync(
        string source,
        string message,
        string level = "Info",
        string? userId = null,
        DateTime? timestampUtc = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var log = new AppLog
        {
            Source = source,
            Message = message,
            Level = level,
            UserId = userId,
            Timestamp = timestampUtc ?? DateTime.UtcNow
        };

        db.Logs.Add(log);
        await db.SaveChangesAsync();
        return log.Id;
    }
}
