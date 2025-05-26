using System.ComponentModel.DataAnnotations;

namespace DarkMarket.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamento com usuário
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}