using DarkMarket.Data;
using DarkMarket.Hubs;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DarkMarket.Tests;

public class BtcPayWebhookServiceTests
{
    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenJsonIsInvalid()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{invalid-json");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{\"invoice\":\"inv-1\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenInvoiceIdHasInvalidType()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{\"invoiceId\":123,\"type\":\"InvoiceSettled\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenTypeHasInvalidType()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"inv-7\",\"type\":5}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenInvoiceIdIsBlank()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"   \",\"type\":\"InvoiceSettled\"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenTypeIsBlank()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: "{\"invoiceId\":\"inv-8\",\"type\":\"   \"}");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnauthorized_WhenSecretIsInvalid()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContext(secret: "wrong", invoiceId: "inv-1", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUnauthorized_WhenConfiguredSecretIsMissing()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "");
        var context = WebhookTestFactory.CreateContext(secret: "anything", invoiceId: "inv-0", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPayloadTooLarge_WhenBodyExceedsLimit()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected");

        var hugeValue = new string('a', 70 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{hugeValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_RespectsConfiguredWebhookMaxBodyBytes()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytes: 64);

        var rawBody = "{\"invoiceId\":\"" + new string('b', 120) + "\",\"type\":\"InvoiceSettled\"}";
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_UsesDefaultLimit_WhenWebhookMaxBodyBytesIsNotANumber()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytesRaw: "abc");

        var mediumValue = new string('x', 10 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{mediumValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_UsesDefaultLimit_WhenWebhookMaxBodyBytesIsNegative()
    {
        using var db = TestDataFactory.CreateDbContext();
        var service = CreateService(db, webhookSecret: "expected", webhookMaxBodyBytesRaw: "-5");

        var mediumValue = new string('y', 10 * 1024);
        var rawBody = $"{{\"invoiceId\":\"{mediumValue}\",\"type\":\"InvoiceSettled\"}}";
        var context = WebhookTestFactory.CreateContextRaw(secret: "expected", rawBody: rawBody);

        var result = await service.HandleAsync(context);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_DoesNotConfirm_WhenEventTypeIsNotSettled()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-2", address: "tb1qaddress");
        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-2", eventType: "InvoiceProcessing");

        var result = await service.HandleAsync(context);
        var persisted = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.False(persisted.IsPaid);
    }

    [Fact]
    public async Task HandleAsync_ConfirmsPaymentAndNotifiesUser_WhenInvoiceSettled()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-3", address: "tb1qaddress", buyerId: "buyer-1");

        var (hubContext, clientProxy) = SignalRTestFactory.CreateHubContextForUser("buyer-1");
        var service = CreateService(db, webhookSecret: "expected", hubContext: hubContext);
        var context = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-3", eventType: "InvoiceSettled");

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
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: true, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-4", address: "tb1qaddress", buyerId: "buyer-1");

        var service = CreateService(db, webhookSecret: "expected");
        var context = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-4", eventType: "InvoiceSettled");

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
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: true, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-5", address: "tb1qaddress", buyerId: "buyer-1");

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
        var context = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-5", eventType: "InvoiceSettled");

        var result = await service.HandleAsync(context);
        var persistedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        Assert.Equal(order.Id, persistedPayment.OrderId);
    }

    [Fact]
    public async Task HandleAsync_WhenSameSettledPayloadIsReceivedTwice_KeepsSingleOrder()
    {
        using var db = TestDataFactory.CreateDbContext();
        var payment = TestDataFactory.SeedPayment(db, isPaid: false, amount: 0.00003m, method: "BTCPayServer", paymentId: "inv-6", address: "tb1qaddress", buyerId: "buyer-1");

        var service = CreateService(db, webhookSecret: "expected");
        var firstContext = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-6", eventType: "InvoiceSettled");
        var secondContext = WebhookTestFactory.CreateContext(secret: "expected", invoiceId: "inv-6", eventType: "InvoiceSettled");

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

        var config = TestConfigurationFactory.Create(configEntries);

        var logService = new LogService(db);

        hubContext ??= SignalRTestFactory.CreateHubContext();
        return new BtcPayWebhookService(db, logService, config, hubContext);
    }

}