using Xunit;
using DarkMarket.Services;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;
using DarkMarket.Data;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Microsoft.AspNetCore.Hosting;

namespace DarkMarket.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task AddProduct_ShouldAddProductToDatabase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb1")
                .Options;
            using var db = new AppDbContext(options);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns("/tmp");

            var service = new ProductService(db, envMock.Object);

            var product = new Product
            {
                Name = "Produto Teste",
                Price = 0.01m,
                Description = "Descrição teste"
            };

            await service.AddAsync(product, null);

            Assert.Single(db.Products);
            Assert.Equal("Produto Teste", db.Products.First().Name);
        }
    }
}