using System.ComponentModel.DataAnnotations;

namespace DarkMarket.Models
{
    public class GatewayInfo
    {
        [Key]
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
    }
}