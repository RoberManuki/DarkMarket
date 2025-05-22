using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DarkMarket.Models;

namespace DarkMarket.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public new DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}