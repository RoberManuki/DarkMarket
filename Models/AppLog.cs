using System;

namespace DarkMarket.Models
{
    public class AppLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string Level { get; set; } = "Info"; // Info, Warning, Error
        public string Source { get; set; } = "";    // Ex: "Payment", "Auth"
        public string Message { get; set; } = "";
        public string? Exception { get; set; }
    }
}