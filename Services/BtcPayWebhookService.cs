using System.Text.Json;
using DarkMarket.Data;
using DarkMarket.Enums;
using DarkMarket.Hubs;
using DarkMarket.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services
{
    public class BtcPayWebhookService
    {
        private readonly AppDbContext _db;
        private readonly LogService _log;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<PaymentHub> _hubContext;

        public BtcPayWebhookService(
            AppDbContext db,
            LogService log,
            IConfiguration configuration,
            IHubContext<PaymentHub> hubContext)
        {
            _db = db;
            _log = log;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public async Task<IResult> HandleAsync(HttpContext context)
        {
            await _log.LogAsync("Webhook chamado.", source: "Webhook", level: "Info");

            var expectedSecret = _configuration["BtcPay:WebhookSecret"];
            var receivedSecret = context.Request.Headers["X-BTCPay-Secret"].FirstOrDefault();

            if (string.IsNullOrEmpty(expectedSecret) || receivedSecret != expectedSecret)
            {
                await _log.LogAsync(
                    $"Tentativa de acesso negada ao webhook. Header recebido: '{receivedSecret ?? "null"}'. IP: {context.Connection.RemoteIpAddress}",
                    source: "Webhook",
                    level: "Warning"
                );
                return Results.Unauthorized();
            }

            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            await _log.LogAsync($"Webhook chamado. Body recebido: {body}", source: "Webhook", level: "Info");

            using var doc = JsonDocument.Parse(body);
            var invoiceId = doc.RootElement.GetProperty("invoiceId").GetString();
            var status = doc.RootElement.GetProperty("type").GetString();

            await _log.LogAsync($"Webhook recebido: status={status}, invoiceId={invoiceId}", source: "Webhook", level: "Info");

            if (status == "InvoiceSettled" && !string.IsNullOrEmpty(invoiceId))
            {
                var payment = _db.Payments.Include(p => p.Product).FirstOrDefault(p => p.PaymentId == invoiceId);

                if (payment == null)
                {
                    await _log.LogAsync($"Pagamento não encontrado para invoiceId={invoiceId}", source: "Webhook", level: "Warning");
                }
                else if (payment.IsPaid)
                {
                    await _log.LogAsync($"Pagamento já está marcado como pago para invoiceId={invoiceId}", source: "Webhook", level: "Info");
                }
                else
                {
                    payment.IsPaid = true;
                    payment.PaidAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    await _log.LogAsync(
                        $"Preparando para criar pedido: paymentId={payment.Id}, userId={payment.UserId}, productId={payment.ProductId}, productUserId={payment.Product?.UserId}",
                        source: "Webhook",
                        level: "Info"
                    );

                    var order = new OrderModel
                    {
                        BuyerId = payment.UserId ?? string.Empty,
                        SellerId = payment.Product?.UserId ?? string.Empty,
                        ProductId = payment.ProductId,
                        Amount = payment.Amount,
                        IsPaid = true,
                        PaymentId = payment.Id,
                        Status = PaymentStatus.AguardandoEntrega,
                        CreatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        payment.OrderId = order.Id;
                        _db.Payments.Update(payment);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        await _log.LogAsync($"Erro ao criar pedido: {ex.Message}", source: "Webhook", level: "Error");
                    }

                    await _log.LogAsync(
                        $"Pagamento confirmado automaticamente via webhook para invoice {invoiceId}. Pedido criado: {order.Id}",
                        source: "Webhook",
                        level: "Info",
                        userId: payment.UserId
                    );

                    if (!string.IsNullOrEmpty(payment.UserId))
                    {
                        await _hubContext.Clients.User(payment.UserId).SendAsync("PaymentConfirmed", payment.PaymentId);
                    }
                }
            }

            return Results.Ok();
        }
    }
}