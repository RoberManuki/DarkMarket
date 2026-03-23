using System.ComponentModel.DataAnnotations;

namespace CryptoMarket.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome Ã© obrigatÃ³rio.")]
        [StringLength(100, ErrorMessage = "O nome deve ter atÃ© 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descriÃ§Ã£o Ã© obrigatÃ³ria.")]
        [StringLength(500, ErrorMessage = "A descriÃ§Ã£o deve ter atÃ© 500 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "O preÃ§o Ã© obrigatÃ³rio.")]
        [Range(0.00001, double.MaxValue, ErrorMessage = "O preÃ§o deve ser no mÃ­nimo 0.00001.")]
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
