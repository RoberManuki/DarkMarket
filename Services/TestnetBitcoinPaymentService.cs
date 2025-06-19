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
            try
            {
                using var http = new HttpClient();
                var url = $"https://api.blockcypher.com/v1/btc/test3/addrs/{address}/balance";
                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var received = doc.RootElement.GetProperty("total_received").GetInt64();

                return received / 100_000_000m;
            }
            catch
            {
                return 0m;
            }
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
                Status = PaymentStatus.AguardandoEntrega,
                CreatedAt = DateTime.UtcNow
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            await log.LogAsync($"[Testnet] Order criada para paymentId={payment.PaymentId}, orderId={order.Id}", source: "Testnet", level: "Info");
        }
    }
}