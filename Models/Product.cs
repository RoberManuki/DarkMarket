using System.ComponentModel.DataAnnotations;

namespace DarkMarket.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(500, ErrorMessage = "A descrição deve ter até 500 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.00001, double.MaxValue, ErrorMessage = "O preço deve ser no mínimo 0.00001.")]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string? ShortDescription { get; set; }
        public string? Category { get; set; }
        public bool RequiresDelivery { get; set; } = true;
    }
}