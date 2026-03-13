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
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateConfirmationService(db, new FakeBitcoinPaymentService("Testnet", 0m));

        var result = await service.ConfirmAsync(new PaymentRecord { Id = 999 });

        Assert.False(result.Confirmed);
        Assert.False(result.AlreadyPaid);
        Assert.Equal(0m, result.ReceivedAmount);
    }

    [Fact]
    public async Task ConfirmAsync_Returns_AlreadyPaid_WhenPaymentIsAlreadyPaid()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: true, amount: 0.00003m, method: "Testnet", paymentId: "pay-1");
        var service = CreateConfirmationService(db, new FakeBitcoinPaymentService("Testnet", 0m));

        var result = await service.ConfirmAsync(payment);

        Assert.True(result.Confirmed);
        Assert.True(result.AlreadyPaid);
        Assert.Equal(payment.Amount, result.ReceivedAmount);
    }

    [Fact]
    public async Task ConfirmAsync_Returns_NotConfirmed_WhenReceivedIsLessThanExpected()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "Testnet", paymentId: "pay-1");
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
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-123");
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

    [Fact]
    public async Task ConfirmAsync_WithTestnetService_CreatesOrder_WhenReceivedIsEnough()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "Testnet", paymentId: "testnet-1");

        var httpFactory = new StubHttpClientFactory(request =>
        {
            if (request.RequestUri?.Host == "api.blockcypher.com")
                return HttpTestResponses.Json("{\"total_received\":3000}");

            return HttpTestResponses.Json("{\"chain_stats\":{\"funded_txo_sum\":0,\"spent_txo_sum\":0}}");
        });

        var testnetService = new TestnetBitcoinPaymentService(httpFactory);
        var service = CreateConfirmationService(db, testnetService);

        var result = await service.ConfirmAsync(payment);
        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        Assert.True(result.Confirmed);
        Assert.False(result.AlreadyPaid);
        Assert.Equal(0.00003m, result.ReceivedAmount);
        Assert.True(persistedPayment.IsPaid);
        Assert.NotNull(order);
        Assert.Equal(order!.Id, persistedPayment.OrderId);
    }

    [Fact]
    public async Task ConfirmAsync_UsesTestnetAsDefault_WhenPaymentMethodIsNull()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: null, paymentId: "testnet-2");

        string? requestedUrl = null;
        var httpFactory = new StubHttpClientFactory(request =>
        {
            requestedUrl = request.RequestUri?.ToString();
            return HttpTestResponses.Json("{\"total_received\":3000}");
        });

        var testnetService = new TestnetBitcoinPaymentService(httpFactory);
        var service = CreateConfirmationService(db, testnetService);

        var result = await service.ConfirmAsync(payment);

        Assert.True(result.Confirmed);
        Assert.Contains(payment.Address, requestedUrl);
    }

    [Fact]
    public async Task ConfirmAsync_WhenAlreadyPaidInTestnet_EnsuresOrderExists()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: true, amount: 0.00003m, method: "Testnet", paymentId: "testnet-3");

        var httpFactory = new StubHttpClientFactory(_ => HttpTestResponses.Json("{}"));
        var testnetService = new TestnetBitcoinPaymentService(httpFactory);
        var service = CreateConfirmationService(db, testnetService);

        var result = await service.ConfirmAsync(payment);
        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        Assert.True(result.Confirmed);
        Assert.True(result.AlreadyPaid);
        Assert.NotNull(order);
        Assert.Equal(order!.Id, persistedPayment.OrderId);
    }

    private static PaymentConfirmationService CreateConfirmationService(AppDbContext db, IBitcoinPaymentService paymentService)
    {
        var factory = new BitcoinPaymentFactory(new[] { paymentService });
        var logService = new LogService(db);
        return new PaymentConfirmationService(db, factory, logService);
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
