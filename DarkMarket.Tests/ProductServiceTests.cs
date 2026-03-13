using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DarkMarket.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task AddAsync_PersistsProduct_WhenImageIsNull()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = new ProductService(db, CreateEnvironment());

        await service.AddAsync(new Product
        {
            Name = "Produto 1",
            Description = "Descrição",
            Price = 0.001m,
            UserId = "seller-1"
        }, imageFile: null);

        Assert.Equal(1, await db.Products.CountAsync());
    }

    [Fact]
    public async Task UpdateAndDeleteAsync_WorkAsExpected()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = new ProductService(db, CreateEnvironment());

        var product = new Product
        {
            Name = "Antes",
            Description = "Desc",
            Price = 0.01m,
            UserId = "seller-1"
        };

        await service.AddAsync(product, imageFile: null);
        product.Name = "Depois";
        await service.UpdateAsync(product, imageFile: null);

        var updated = await service.GetByIdAsync(product.Id);
        Assert.Equal("Depois", updated!.Name);

        await service.DeleteAsync(product.Id);
        Assert.Null(await service.GetByIdAsync(product.Id));
    }

    [Fact]
    public async Task GetAllExceptUserAsync_FiltersByUser()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = new ProductService(db, CreateEnvironment());

        await service.AddAsync(new Product { Name = "A", Description = "D", Price = 0.1m, UserId = "u1" }, null);
        await service.AddAsync(new Product { Name = "B", Description = "D", Price = 0.2m, UserId = "u2" }, null);

        var products = await service.GetAllExceptUserAsync("u1");

        Assert.Single(products);
        Assert.Equal("u2", products[0].UserId);
    }

    private static IWebHostEnvironment CreateEnvironment()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(x => x.WebRootPath).Returns(Path.GetTempPath());
        return env.Object;
    }
}