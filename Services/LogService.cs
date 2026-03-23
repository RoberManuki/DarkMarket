using CryptoMarket.Data;
using CryptoMarket.Models;

namespace CryptoMarket.Services
{
    public class LogService
    {
        private readonly AppDbContext _db;

        public LogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string message, string source = "App", string level = "Info", string? userId = null, Exception? ex = null)
        {
            var log = new AppLog
            {
                UserId = userId,
                Level = level,
                Source = source,
                Message = message,
                Exception = ex?.ToString()
            };
            
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
