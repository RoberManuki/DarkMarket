using DarkMarket.Models;
using DarkMarket.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;


namespace DarkMarket.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.FindAsync(id);

        public async Task AddAsync(Product product, IBrowserFile? imageFile)
        {
            if (imageFile != null)
            {
                var imagePath = await SaveImageAsync(imageFile);
                product.ImagePath = imagePath;
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product, IBrowserFile? imageFile)
        {
            if (imageFile != null)
            {
                var imagePath = await SaveImageAsync(imageFile);
                product.ImagePath = imagePath;
            }
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> SaveImageAsync(IBrowserFile imageFile)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.Name)}";
            var filePath = Path.Combine(uploads, fileName);

            await using var stream = File.Create(filePath);
            await imageFile.OpenReadStream(5 * 1024 * 1024).CopyToAsync(stream); // 5MB max

            return $"/uploads/{fileName}";
        }

        public async Task<int> GetProductsCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<List<Product>> GetAllExceptUserAsync(string userId) =>
            await _context.Products.Where(p => p.UserId != userId).OrderByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<List<Product>> GetByUserIdAsync(string userId) =>
            await _context.Products.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
}