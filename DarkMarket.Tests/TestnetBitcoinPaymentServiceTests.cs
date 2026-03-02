using System.Net;
using System.Text;
using DarkMarket.Data;
using DarkMarket.Enums;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Tests;

public class TestnetBitcoinPaymentServiceTests
{
    [Fact]
    public async Task GetReceivedAmountAsync_ReturnsZero_WhenAddressIsEmpty()
    {
        var service = new TestnetBitcoinPaymentService(new StubHttpClientFactory(_ => throw new InvalidOperationException("Should not call HTTP")));

        var received = await service.GetReceivedAmountAsync(" ");

        Assert.Equal(0m, received);
    }

    [Fact]
    public async Task GetReceivedAmountAsync_UsesBlockCypher_WhenValueIsPositive()
    {
        var service = new TestnetBitcoinPaymentService(new StubHttpClientFactory(request =>
        {
            if (request.RequestUri?.Host == "api.blockcypher.com")
            {
                return JsonResponse("{\"total_received\":3000}");
            }

            throw new InvalidOperationException("Blockstream should not be called when BlockCypher already has value.");
        }));

        var received = await service.GetReceivedAmountAsync("tb1qtest");

        Assert.Equal(0.00003m, received);
    }

    [Fact]
    public async Task GetReceivedAmountAsync_FallsBackToBlockstream_WhenBlockCypherReturnsZero()
    {
        var service = new TestnetBitcoinPaymentService(new StubHttpClientFactory(request =>
        {
            if (request.RequestUri?.Host == "api.blockcypher.com")
            {
                return JsonResponse("{\"total_received\":0}");
            }

            if (request.RequestUri?.Host == "blockstream.info")
            {
                return JsonResponse("{\"chain_stats\":{\"funded_txo_sum\":7000,\"spent_txo_sum\":2000}}");
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }));

        var received = await service.GetReceivedAmountAsync("tb1qtest");

        Assert.Equal(0.00005m, received);
    }

    [Fact]
    public async Task CheckAndMarkPaymentAsync_MarksAsPaid_AndCreatesOrder()
    {
        using var db = CreateDbContext();
        var service = new TestnetBitcoinPaymentService(new StubHttpClientFactory(_ => JsonResponse("{}")));
        var log = new LogService(db);

        var payment = SeedPayment(db, isPaid: false, paymentId: "pay-testnet-1");

        var result = await service.CheckAndMarkPaymentAsync(db, log, "pay-testnet-1");

        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var persistedOrder = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        Assert.True(result);
        Assert.True(persistedPayment.IsPaid);
        Assert.NotNull(persistedPayment.PaidAt);
        Assert.NotNull(persistedOrder);
        Assert.Equal(PaymentStatus.AguardandoEntrega, persistedOrder!.Status);
    }

    [Fact]
    public async Task CheckAndMarkPaymentAsync_WhenAlreadyPaid_CreatesMissingOrder()
    {
        using var db = CreateDbContext();
        var service = new TestnetBitcoinPaymentService(new StubHttpClientFactory(_ => JsonResponse("{}")));
        var log = new LogService(db);

        var payment = SeedPayment(db, isPaid: true, paymentId: "pay-testnet-2");

        var result = await service.CheckAndMarkPaymentAsync(db, log, "pay-testnet-2");

        var order = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        Assert.True(result);
        Assert.NotNull(order);
        Assert.Equal(payment.UserId, order!.BuyerId);
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static PaymentRecord SeedPayment(AppDbContext db, bool isPaid, string paymentId)
    {
        var product = new Product
        {
            Name = "Produto Testnet",
            Description = "Descrição",
            Price = 0.00003m,
            UserId = "seller-1"
        };
        db.Products.Add(product);
        db.SaveChanges();

        var payment = new PaymentRecord
        {
            ProductId = product.Id,
            UserId = "buyer-1",
            Address = "tb1qaddress",
            PaymentId = paymentId,
            PaymentMethod = "Testnet",
            Amount = 0.00003m,
            IsPaid = isPaid,
            Product = product
        };
        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new RoutingHandler(_responder));
        }
    }

    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public RoutingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}