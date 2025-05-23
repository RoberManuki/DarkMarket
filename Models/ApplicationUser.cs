using Microsoft.AspNetCore.Identity;

namespace DarkMarket.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        // Adicione outros campos personalizados aqui
    }
}