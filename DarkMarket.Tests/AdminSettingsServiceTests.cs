using DarkMarket.Data;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Tests;

public class AdminSettingsServiceTests
{
    [Fact]
    public async Task GetOperationFeePercentAsync_ReturnsDefault_WhenSettingDoesNotExist()
    {
        await using var db = CreateDbContext();
        var service = new AdminSettingsService(db);

        var fee = await service.GetOperationFeePercentAsync();

        Assert.Equal(AdminSettingsService.DefaultOperationFeePercent, fee);
    }

    [Fact]
    public async Task GetOperationFeePercentAsync_ReturnsDefault_WhenStoredValueIsInvalid()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSetting
        {
            Key = AdminSettingsService.OperationFeePercentKey,
            Value = "not-a-number"
        });
        await db.SaveChangesAsync();

        var service = new AdminSettingsService(db);
        var fee = await service.GetOperationFeePercentAsync();

        Assert.Equal(AdminSettingsService.DefaultOperationFeePercent, fee);
    }

    [Fact]
    public async Task SetOperationFeePercentAsync_PersistsRoundedValue_WhenInputIsValid()
    {
        await using var db = CreateDbContext();
        var service = new AdminSettingsService(db);

        var saved = await service.SetOperationFeePercentAsync(3.257m);
        var fee = await service.GetOperationFeePercentAsync();

        Assert.True(saved);
        Assert.Equal(3.26m, fee);
    }

    [Fact]
    public async Task SetOperationFeePercentAsync_ReturnsFalse_WhenInputIsOutOfRange()
    {
        await using var db = CreateDbContext();
        var service = new AdminSettingsService(db);

        var savedNegative = await service.SetOperationFeePercentAsync(-1m);
        var savedAboveHundred = await service.SetOperationFeePercentAsync(100.01m);

        Assert.False(savedNegative);
        Assert.False(savedAboveHundred);
        Assert.Empty(db.AppSettings);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"admin-settings-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}
