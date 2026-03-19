using NBitcoin;
using System.Net.Http.Json;
using System.Text.Json;
using DarkMarket.Data;
using DarkMarket.Models;
using DarkMarket.Enums;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services
{
    public class TestnetBitcoinPaymentService : IBitcoinPaymentService
    {
        public string Name => "Testnet";
        private readonly IHttpClientFactory _httpClientFactory;

        public TestnetBitcoinPaymentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<(string Address, string PaymentId, string PrivateKey)> GenerateAddressWithKeyAsync(decimal amount, string? orderId = null)
        {
            var network = Network.TestNet;
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, network).ToString();
            var privateKey = key.GetWif(network).ToString();
            var paymentId = Guid.NewGuid().ToString();

            return Task.FromResult((address, paymentId, privateKey));
        }

        public Task<(string Address, string PaymentId)> GenerateAddressAsync(decimal amount, string? orderId = null)
        {
            var network = Network.TestNet;
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, network).ToString();
            var paymentId = Guid.NewGuid().ToString();

            return Task.FromResult((address, paymentId));
        }

        public async Task<decimal> GetReceivedAmountAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return 0m;

            var normalizedAddress = address.Trim();

            try
            {
                var receivedFromBlockCypher = await GetReceivedFromBlockCypherAsync(normalizedAddress);
                if (receivedFromBlockCypher > 0m)
                    return receivedFromBlockCypher;

                return await GetReceivedFromBlockstreamAsync(normalizedAddress);
            }
            catch
            {
                return 0m;
            }
        }

        private async Task<decimal> GetReceivedFromBlockCypherAsync(string address)
        {
            var http = _httpClientFactory.CreateClient();
            var url = $"https://api.blockcypher.com/v1/btc/test3/addrs/{address}/balance";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("total_received", out var totalReceivedProp))
                return 0m;

            var received = totalReceivedProp.GetInt64();
            return received / 100_000_000m;
        }

        private async Task<decimal> GetReceivedFromBlockstreamAsync(string address)
        {
            var http = _httpClientFactory.CreateClient();
            var url = $"https://blockstream.info/testnet/api/address/{address}";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("chain_stats", out var chainStats))
                return 0m;

            var funded = chainStats.TryGetProperty("funded_txo_sum", out var fundedProp)
                ? fundedProp.GetInt64()
                : 0L;

            var spent = chainStats.TryGetProperty("spent_txo_sum", out var spentProp)
                ? spentProp.GetInt64()
                : 0L;

            var received = Math.Max(0L, funded - spent);

            return received / 100_000_000m;
        }

        public async Task<bool> CheckAndMarkPaymentAsync(AppDbContext db, LogService log, string paymentId)
        {
            var payment = db.Payments.Include(p => p.Product).FirstOrDefault(p => p.PaymentId == paymentId);
            if (payment == null)
            {
                await log.LogAsync($"[Testnet] Pagamento não encontrado para paymentId={paymentId}", source: "Testnet", level: "Warning");
                return false;
            }

            if (payment.IsPaid)
            {
                await log.LogAsync($"[Testnet] Pagamento já está marcado como pago para paymentId={paymentId}", source: "Testnet", level: "Info");
                // Garante que a order existe
                if (!db.Orders.Any(o => o.PaymentId == payment.Id))
                {
                    await CreateOrderAsync(db, payment, log);
                }
                return true;
            }

            // Aqui você pode adicionar lógica para checar na blockchain se quiser
            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await log.LogAsync($"[Testnet] Pagamento marcado como pago para paymentId={paymentId}", source: "Testnet", level: "Info");

            // Cria order se não existir
            if (!db.Orders.Any(o => o.PaymentId == payment.Id))
            {
                await CreateOrderAsync(db, payment, log);
            }

            return true;
        }

        private async Task CreateOrderAsync(AppDbContext db, PaymentRecord payment, LogService log)
        {
            if (string.IsNullOrEmpty(payment.UserId))
                throw new Exception("UserId do pagamento está vazio ou nulo. Não é possível criar a order.");

            var order = new OrderModel
            {
                BuyerId = payment.UserId,
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

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            payment.OrderId = order.Id;
            db.Payments.Update(payment);
            await db.SaveChangesAsync();

            await log.LogAsync($"[Testnet] Order criada para paymentId={payment.PaymentId}, orderId={order.Id}", source: "Testnet", level: "Info");
        }
    }
}