using System.Text;
using DarkMarket.Data;
using DarkMarket.Hubs;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DarkMarket.Tests;

public class BtcPayWebhookServiceTests
{
    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenJsonIsInvalid()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{invalid-json");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{\"invoice\":\"inv-1\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenInvoiceIdHasInvalidType()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{\"invoiceId\":123,\"type\":\"InvoiceSettled\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenTypeHasInvalidType()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"inv-7\",\"type\":5}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenInvoiceIdIsBlank()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"   \",\"type\":\"InvoiceSettled\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenTypeIsBlank()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"inv-8\",\"type\":\"   \"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnauthorized_WhenSecretIsInvalid()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContext(secret: "wrong", invoiceId: "inv-1", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnauthorized_WhenConfiguredSecretIsMissing()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "");
        var context = CreateWebhookContext(secret: "anything", invoiceId: "inv-0", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPayloadTooLarge_WhenBodyExceedsLimit()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");

        var hugeValue = new string('a', 70 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{hugeValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_RespectsConfiguredWebhookMaxBodyBytes()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytes: 64);

        var rawBody = "{\"invoiceId\":\"" + new string('b', 120) + "\",\"type\":\"InvoiceSettled\"}";
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_UsesDefaultLimit_WhenWebhookMaxBodyBytesIsNotANumber()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytesRaw: "abc");

        var mediumValue = new string('x', 10 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{mediumValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_UsesDefaultLimit_WhenWebhookMaxBodyBytesIsNegative()
    {
        using var db = CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytesRaw: "-5");

        var mediumValue = new string('y', 10 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{mediumValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = CreateWebhookContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_DoesNotConfirm_WhenEventTypeIsNotSettled()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, paymentId: "inv-2", isPaid: false);
        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContext(secret: "expected", invoiceId: "inv-2", eventType: "InvoiceProcessing");

        var result = await service.HandleAsync(context);
        var persisted = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.False(persisted.IsPaid);
    }

    [Fact]
    public async Task HandleAsync_ConfirmsPaymentAndNotifiesUser_WhenInvoiceSettled()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, paymentId: "inv-3", isPaid: false, userId: "buyer-1");

        var clientProxy = new Mock<IClientProxy>();
        clientProxy
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clients = new Mock<IHubClients>();
        clients.Setup(c => c.User("buyer-1")).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<PaymentHub>>();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);
        hubContext.SetupGet(h => h.Groups).Returns(Mock.Of<IGroupManager>());

        var service = CreateService(db, webhookSecret: "expected", hubContext: hubContext.Object);
        var context = CreateWebhookContext(secret: "expected", invoiceId: "inv-3", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);
        var persisted = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.True(persisted.IsPaid);
        Assert.NotNull(persisted.PaidAt);
        Assert.NotNull(order);
        Assert.Equal(order!.Id, persisted.OrderId);

        clientProxy.Verify(
            p => p.SendCoreAsync(
                "PaymentConfirmed",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], "inv-3")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentAlreadyPaidAndOrderMissing_CreatesOrderAndLinksPayment()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, paymentId: "inv-4", isPaid: true, userId: "buyer-1");

        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContext(secret: "expected", invoiceId: "inv-4", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);
        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var order = await db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.NotNull(order);
        Assert.Equal(order!.Id, persistedPayment.OrderId);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentAlreadyPaidAndOrderExistsButLinkMissing_RepairsPaymentOrderLink()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, paymentId: "inv-5", isPaid: true, userId: "buyer-1");

        var order = new OrderModel
        {
            BuyerId = payment.UserId ?? string.Empty,
            SellerId = "seller-1",
            ProductId = payment.ProductId,
            Amount = payment.Amount,
            IsPaid = true,
            PaymentId = payment.Id,
            Status = DarkMarket.Enums.PaymentStatus.AguardandoEntrega,
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        payment.OrderId = null;
        db.Payments.Update(payment);
        await db.SaveChangesAsync();

        var service = CreateService(db, webhookSecret: "expected");
        var context = CreateWebhookContext(secret: "expected", invoiceId: "inv-5", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);
        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.Equal(order.Id, persistedPayment.OrderId);
    }

    [Fact]
    public async Task HandleAsync_WhenSameSettledPayloadIsReceivedTwice_KeepsSingleOrder()
    {
        using var db = CreateDbContext();
        var payment = SeedPayment(db, paymentId: "inv-6", isPaid: false, userId: "buyer-1");

        var service = CreateService(db, webhookSecret: "expected");
        var firstContext = CreateWebhookContext(secret: "expected", invoiceId: "inv-6", eventType: "InvoiceSettled");
        var secondContext = CreateWebhookContext(secret: "expected", invoiceId: "inv-6", eventType: "InvoiceSettled");

        await service.HandleAsync(firstContext);
        await service.HandleAsync(secondContext);

        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        var orders = await db.Orders.Where(o => o.PaymentId == payment.Id).ToListAsync();

        Assert.True(persistedPayment.IsPaid);
        Assert.Single(orders);
        Assert.Equal(orders[0].Id, persistedPayment.OrderId);
    }

    private static BtcPayWebhookService CreateService(
        AppDbContext db,
        string webhookSecret,
        int? webhookMaxBodyBytes = null,
        string? webhookMaxBodyBytesRaw = null,
        IHubContext<PaymentHub>? hubContext = null)
    {
        var configEntries = new Dictionary<string, string?>
        {
            ["BtcPay:WebhookSecret"] = webhookSecret
        };

        if (webhookMaxBodyBytes.HasValue)
        {
            configEntries["BtcPay:WebhookMaxBodyBytes"] = webhookMaxBodyBytes.Value.ToString();
        }

        if (webhookMaxBodyBytesRaw != null)
        {
            configEntries["BtcPay:WebhookMaxBodyBytes"] = webhookMaxBodyBytesRaw;
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configEntries)
            .Build();

        var logService = new LogService(db);

        hubContext ??= CreateDefaultHubContext();
        return new BtcPayWebhookService(db, logService, config, hubContext);
    }

    private static IHubContext<PaymentHub> CreateDefaultHubContext()
    {
        var clientProxy = new Mock<IClientProxy>();
        clientProxy
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clients = new Mock<IHubClients>();
        clients.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<PaymentHub>>();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);
        hubContext.SetupGet(h => h.Groups).Returns(Mock.Of<IGroupManager>());
        return hubContext.Object;
    }

    private static DefaultHttpContext CreateWebhookContext(string secret, string invoiceId, string eventType)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-BTCPay-Secret"] = secret;

        var body = $"{{\"invoiceId\":\"{invoiceId}\",\"type\":\"{eventType}\"}}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Request.Body.Position = 0;

        return context;
    }

    private static DefaultHttpContext CreateWebhookContextRaw(string secret, string rawBody)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-BTCPay-Secret"] = secret;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(rawBody));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Request.Body.Position = 0;
        return context;
    }

    private static PaymentRecord SeedPayment(AppDbContext db, string paymentId, bool isPaid, string userId = "buyer-1")
    {
        var product = new Product
        {
            Name = "Produto webhook",
            Description = "Descrição",
            Price = 0.00003m,
            UserId = "seller-1"
        };
        db.Products.Add(product);
        db.SaveChanges();

        var payment = new PaymentRecord
        {
            ProductId = product.Id,
            Address = "tb1qaddress",
            PaymentId = paymentId,
            PaymentMethod = "BTCPayServer",
            Amount = 0.00003m,
            IsPaid = isPaid,
            UserId = userId
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
}