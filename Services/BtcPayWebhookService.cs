using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using CryptoMarket.Data;
using CryptoMarket.Enums;
using CryptoMarket.Hubs;
using CryptoMarket.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace CryptoMarket.Services
{
    public class BtcPayWebhookService
    {
        private const int DefaultMaxWebhookBodyBytes = 64 * 1024;
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
            var receivedSecret = GetSingleWebhookSecretHeader(context.Request.Headers);

            if (!IsValidWebhookSecret(expectedSecret, receivedSecret))
            {
                await _log.LogAsync(
                    $"Tentativa de acesso negada ao webhook. Header recebido: '{receivedSecret ?? "null"}'. IP: {context.Connection.RemoteIpAddress}",
                    source: "Webhook",
                    level: "Warning"
                );
                return Results.Unauthorized();
            }

            var maxWebhookBodyBytes = GetMaxWebhookBodyBytes();

            if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > maxWebhookBodyBytes)
            {
                await _log.LogAsync(
                    $"Payload do webhook excede limite permitido ({context.Request.ContentLength.Value} bytes). Limite: {maxWebhookBodyBytes} bytes.",
                    source: "Webhook",
                    level: "Warning"
                );
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (Encoding.UTF8.GetByteCount(body) > maxWebhookBodyBytes)
            {
                await _log.LogAsync(
                    $"Payload do webhook excede limite permitido apÃ³s leitura do corpo. Limite: {maxWebhookBodyBytes} bytes.",
                    source: "Webhook",
                    level: "Warning"
                );
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            await _log.LogAsync($"Webhook chamado. Body recebido: {body}", source: "Webhook", level: "Info");

            if (!TryParseWebhookPayload(body, out var invoiceId, out var status, out var logMessage, out var responseMessage))
            {
                await _log.LogAsync(logMessage, source: "Webhook", level: "Warning");
                return Results.BadRequest(responseMessage);
            }

            await _log.LogAsync($"Webhook recebido: status={status}, invoiceId={invoiceId}", source: "Webhook", level: "Info");

            if (status != "InvoiceSettled")
                return Results.Ok();

            var payment = _db.Payments.Include(p => p.Product).FirstOrDefault(p => p.PaymentId == invoiceId);

            if (payment == null)
            {
                await _log.LogAsync($"Pagamento nÃ£o encontrado para invoiceId={invoiceId}", source: "Webhook", level: "Warning");
                return Results.Ok();
            }

            if (payment.IsPaid)
            {
                await _log.LogAsync($"Pagamento jÃ¡ estÃ¡ marcado como pago para invoiceId={invoiceId}", source: "Webhook", level: "Info");
                await EnsureOrderAsync(payment, invoiceId);
                return Results.Ok();
            }

            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _log.LogAsync(
                $"Preparando para criar pedido: paymentId={payment.Id}, userId={payment.UserId}, productId={payment.ProductId}, productUserId={payment.Product?.UserId}",
                source: "Webhook",
                level: "Info"
            );

            await EnsureOrderAsync(payment, invoiceId);

            if (!string.IsNullOrEmpty(payment.UserId))
            {
                await _hubContext.Clients.User(payment.UserId).SendAsync("PaymentConfirmed", payment.PaymentId);
            }

            return Results.Ok();
        }

        private static bool IsValidWebhookSecret(string? expectedSecret, string? receivedSecret)
        {
            if (string.IsNullOrEmpty(expectedSecret) || string.IsNullOrEmpty(receivedSecret))
                return false;

            var expectedBytes = Encoding.UTF8.GetBytes(expectedSecret);
            var receivedBytes = Encoding.UTF8.GetBytes(receivedSecret);

            if (expectedBytes.Length != receivedBytes.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
        }

        private static string? GetSingleWebhookSecretHeader(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("X-BTCPay-Secret", out StringValues values))
                return null;

            if (values.Count != 1)
                return null;

            return values[0];
        }

        private static bool TryParseWebhookPayload(
            string body,
            out string invoiceId,
            out string status,
            out string logMessage,
            out string responseMessage)
        {
            invoiceId = string.Empty;
            status = string.Empty;
            logMessage = string.Empty;
            responseMessage = string.Empty;

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                logMessage = $"Payload JSON invÃ¡lido no webhook: {ex.Message}";
                responseMessage = "Payload invÃ¡lido.";
                return false;
            }

            using (doc)
            {
                if (!doc.RootElement.TryGetProperty("invoiceId", out var invoiceIdProp) ||
                    !doc.RootElement.TryGetProperty("type", out var typeProp))
                {
                    logMessage = "Payload do webhook sem campos obrigatÃ³rios (invoiceId/type).";
                    responseMessage = "Campos obrigatÃ³rios ausentes.";
                    return false;
                }

                if (invoiceIdProp.ValueKind != JsonValueKind.String || typeProp.ValueKind != JsonValueKind.String)
                {
                    logMessage = "Payload do webhook com tipos invÃ¡lidos para invoiceId/type.";
                    responseMessage = "Tipos de campos invÃ¡lidos.";
                    return false;
                }

                invoiceId = invoiceIdProp.GetString() ?? string.Empty;
                status = typeProp.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(invoiceId) || string.IsNullOrWhiteSpace(status))
                {
                    logMessage = "Payload do webhook com invoiceId/type vazios.";
                    responseMessage = "Campos obrigatÃ³rios invÃ¡lidos.";
                    return false;
                }
            }

            return true;
        }

        private int GetMaxWebhookBodyBytes()
        {
            var configuredValue = _configuration["BtcPay:WebhookMaxBodyBytes"];
            if (int.TryParse(configuredValue, out var parsed) && parsed > 0)
                return parsed;

            return DefaultMaxWebhookBodyBytes;
        }

        private async Task EnsureOrderAsync(PaymentRecord payment, string invoiceId)
        {
            var existingOrder = await _db.Orders.FirstOrDefaultAsync(o => o.PaymentId == payment.Id);
            if (existingOrder != null)
            {
                if (payment.OrderId != existingOrder.Id)
                {
                    payment.OrderId = existingOrder.Id;
                    _db.Payments.Update(payment);
                    await _db.SaveChangesAsync();
                }
                return;
            }

            try
            {
                var order = new OrderModel
                {
                    BuyerId = payment.UserId ?? string.Empty,
                    SellerId = payment.Product?.UserId ?? string.Empty,
                    ProductId = payment.ProductId,
                    Amount = payment.Amount,
                    IsPaid = true,
                    PaymentId = payment.Id,
                    DeliveryAgentId = payment.DeliveryAgentId,
                    EstimatedDeliveryDays = payment.EstimatedDeliveryDays,
                    Status = PaymentStatus.AguardandoEntrega,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                payment.OrderId = order.Id;
                _db.Payments.Update(payment);
                await _db.SaveChangesAsync();

                await _log.LogAsync(
                    $"Pagamento confirmado automaticamente via webhook para invoice {invoiceId}. Pedido criado: {order.Id}",
                    source: "Webhook",
                    level: "Info",
                    userId: payment.UserId
                );
            }
            catch (Exception ex)
            {
                await _log.LogAsync($"Erro ao criar pedido: {ex.Message}", source: "Webhook", level: "Error");
            }
        }
    }
}
