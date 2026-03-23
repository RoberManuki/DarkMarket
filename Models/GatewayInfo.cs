using System.ComponentModel.DataAnnotations;

namespace CryptoMarket.Models
{
    public class GatewayInfo
    {
        [Key]
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
    }
}
