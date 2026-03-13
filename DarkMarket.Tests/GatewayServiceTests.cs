using DarkMarket.Models;
using DarkMarket.Services;

namespace DarkMarket.Tests;

public class GatewayServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsGateways()
    {
        using var db = TestDataFactory.CreateDbContext();
        db.Gateways.AddRange(
            new GatewayInfo { Name = "BTCPayServer", Enabled = true },
            new GatewayInfo { Name = "Testnet", Enabled = false });
        await db.SaveChangesAsync();

        var service = new GatewayService(db);
        var gateways = await service.GetAllAsync();

        Assert.Equal(2, gateways.Count);
    }

    [Fact]
    public async Task SetStatusAsync_Updates_WhenGatewayExists_AndReturnsFalseOtherwise()
    {
        using var db = TestDataFactory.CreateDbContext();
        db.Gateways.Add(new GatewayInfo { Name = "Testnet", Enabled = false });
        await db.SaveChangesAsync();

        var service = new GatewayService(db);

        var updated = await service.SetStatusAsync("Testnet", enabled: true);
        var missing = await service.SetStatusAsync("Unknown", enabled: true);

        Assert.True(updated);
        Assert.False(missing);
        Assert.True((await db.Gateways.FindAsync("Testnet"))!.Enabled);
    }
}