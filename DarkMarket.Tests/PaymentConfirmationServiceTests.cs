using DarkMarket.Data;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Tests;

public class PaymentConfirmationServiceTests
{
    [Fact]
    public async Task ConfirmAsync_Returns_NotConfirmed_WhenPaymentDoesNotExist()
    {
        using var db = CreateDbContext();
        var service = CreateConfirmationService(db, new FakeBitcoinPaymentService("Testnet", 0m));

        var result = await service.ConfirmAsync(new PaymentRecord { Id = 999 });

        Assert.False(result.Confirmed);
        Assert.False(result.AlreadyPaid);
        Assert.Equal(0m, result.ReceivedAmount);
    }

    [Fact]
    public async Task ConfirmAsync_Returns_AlreadyPaid_WhenPaymentIsAlreadyPaid()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, isPaid: true, amount: 0.00003m, method: "Testnet");
        var service = CreateConfirmationService(db, new FakeBitcoinPaymentService("Testnet", 0m));

        var result = await service.ConfirmAsync(payment);

        Assert.True(result.Confirmed);
        Assert.True(result.AlreadyPaid);
        Assert.Equal(payment.Amount, result.ReceivedAmount);
    }

    [Fact]
    public async Task ConfirmAsync_Returns_NotConfirmed_WhenReceivedIsLessThanExpected()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, isPaid: false, amount: 0.00003m, method: "Testnet");
        var fakeGateway = new FakeBitcoinPaymentService("Testnet", 0.00002m);
        var service = CreateConfirmationService(db, fakeGateway);

        var result = await service.ConfirmAsync(payment);
        var persisted = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        Assert.False(result.Confirmed);
        Assert.False(result.AlreadyPaid);
        Assert.Equal(0.00002m, result.ReceivedAmount);
        Assert.False(persisted.IsPaid);
        Assert.Equal(payment.Address, fakeGateway.LastParameter);
    }

    [Fact]
    public async Task ConfirmAsync_MarksAsPaid_WhenReceivedIsEnough()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, isPaid: false, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-123");
        var fakeGateway = new FakeBitcoinPaymentService("BTCPayServer", 0.00003m);
        var service = CreateConfirmationService(db, fakeGateway);

        var result = await service.ConfirmAsync(payment);
        var persisted = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        Assert.True(result.Confirmed);
        Assert.False(result.AlreadyPaid);
        Assert.Equal(0.00003m, result.ReceivedAmount);
        Assert.True(persisted.IsPaid);
        Assert.NotNull(persisted.PaidAt);
        Assert.Equal("inv-123", fakeGateway.LastParameter);
    }

    private static PaymentConfirmationService CreateConfirmationService(AppDbContext db, IBitcoinPaymentService paymentService)
    {
        var factory = new BitcoinPaymentFactory(new[] { paymentService });
        var logService = new LogService(db);
        return new PaymentConfirmationService(db, factory, logService);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static PaymentRecord SeedPayment(AppDbContext db, bool isPaid, decimal amount, string method, string paymentId = "pay-1")
    {
        var product = new Product
        {
            Name = "Produto teste",
            Description = "Descrição",
            Price = amount,
            UserId = "seller-1"
        };

        db.Products.Add(product);
        db.SaveChanges();

        var payment = new PaymentRecord
        {
            ProductId = product.Id,
            Address = "tb1qexampleaddress",
            PaymentId = paymentId,
            PaymentMethod = method,
            Amount = amount,
            IsPaid = isPaid,
            UserId = "buyer-1"
        };

        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    private sealed class FakeBitcoinPaymentService : IBitcoinPaymentService
    {
        private readonly decimal _receivedAmount;

        public FakeBitcoinPaymentService(string name, decimal receivedAmount)
        {
            Name = name;
            _receivedAmount = receivedAmount;
        }

        public string Name { get; }
        public string? LastParameter { get; private set; }

        public Task<(string Address, string PaymentId)> GenerateAddressAsync(decimal amount, string? orderId = null)
        {
            throw new NotSupportedException();
        }

        public Task<(string Address, string PaymentId, string PrivateKey)> GenerateAddressWithKeyAsync(decimal amount, string? orderId = null)
        {
            throw new NotSupportedException();
        }

        public Task<decimal> GetReceivedAmountAsync(string address)
        {
            LastParameter = address;
            return Task.FromResult(_receivedAmount);
        }
    }
}
