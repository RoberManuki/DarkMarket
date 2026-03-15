using System.Globalization;
using DarkMarket.Data;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services;

public class AdminSettingsService
{
    public const string OperationFeePercentKey = "OperationFeePercent";
    public const decimal DefaultOperationFeePercent = 2.0m;

    private readonly AppDbContext _db;

    public AdminSettingsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetOperationFeePercentAsync()
    {
        var setting = await _db.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == OperationFeePercentKey);

        if (setting is null)
        {
            return DefaultOperationFeePercent;
        }

        if (!decimal.TryParse(setting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return DefaultOperationFeePercent;
        }

        return Math.Clamp(parsed, 0m, 100m);
    }

    public async Task<bool> SetOperationFeePercentAsync(decimal operationFeePercent)
    {
        if (operationFeePercent < 0m || operationFeePercent > 100m)
        {
            return false;
        }

        var normalized = Math.Round(operationFeePercent, 2, MidpointRounding.AwayFromZero)
            .ToString("0.##", CultureInfo.InvariantCulture);

        var setting = await _db.AppSettings.FindAsync(OperationFeePercentKey);

        if (setting is null)
        {
            setting = new AppSetting
            {
                Key = OperationFeePercentKey,
                Value = normalized
            };
            _db.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = normalized;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
