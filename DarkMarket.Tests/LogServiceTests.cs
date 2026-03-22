using DarkMarket.Data;
using DarkMarket.Services;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Tests;

public class LogServiceTests
{
    [Fact]
    public async Task LogAsync_PersistsLogEntry_WithProvidedValues()
    {
        await using var db = CreateDbContext();
        var service = new LogService(db);

        await service.LogAsync(
            message: "payment started",
            source: "Payment",
            level: "Warning",
            userId: "user-42");

        var saved = Assert.Single(db.Logs);
        Assert.Equal("payment started", saved.Message);
        Assert.Equal("Payment", saved.Source);
        Assert.Equal("Warning", saved.Level);
        Assert.Equal("user-42", saved.UserId);
        Assert.Null(saved.Exception);
    }

    [Fact]
    public async Task LogAsync_PersistsExceptionText_WhenExceptionIsProvided()
    {
        await using var db = CreateDbContext();
        var service = new LogService(db);

        var ex = new InvalidOperationException("boom");

        await service.LogAsync(message: "failed", ex: ex);

        var saved = Assert.Single(db.Logs);
        Assert.NotNull(saved.Exception);
        Assert.Contains("InvalidOperationException", saved.Exception, StringComparison.Ordinal);
        Assert.Contains("boom", saved.Exception, StringComparison.Ordinal);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"log-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}
