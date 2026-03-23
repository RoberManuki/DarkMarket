using CryptoMarket.Data;
using CryptoMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoMarket.Tests;

internal static class TestDataFactory
{
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static PaymentRecord SeedPayment(
        AppDbContext db,
        bool isPaid,
        decimal amount,
        string? method,
        string paymentId,
        string address = "tb1qexampleaddress",
        string buyerId = "buyer-1",
        string sellerId = "seller-1")
    {
        var product = new Product
        {
            Name = "Produto teste",
            Description = "DescriÃ§Ã£o",
            Price = amount,
            UserId = sellerId
        };

        db.Products.Add(product);
        db.SaveChanges();

        var payment = new PaymentRecord
        {
            ProductId = product.Id,
            Address = address,
            PaymentId = paymentId,
            PaymentMethod = method,
            Amount = amount,
            IsPaid = isPaid,
            UserId = buyerId
        };

        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }
}
