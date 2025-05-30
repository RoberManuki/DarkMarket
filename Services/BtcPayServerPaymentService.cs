using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace DarkMarket.Services
{
    public class BtcPayServerPaymentService : IBitcoinPaymentService
    {
        public string Name => "BTCPayServer";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey = "4ec8f17898b161f4f31e951a423c5e980999e871";
        private readonly string _storeId = "93VJRUdSP8XfYu3cfvFMqsW7krnuYiyjr5FVSfBosZ7y";
        private readonly string _btcpayUrl = "https://mainnet.demo.btcpayserver.org/";

        public BtcPayServerPaymentService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(string Address, string? PaymentId)> GenerateAddressAsync(decimal amount, string? orderId = null)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_btcpayUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _apiKey);

            var body = new
            {
                amount = amount.ToString("0.########", System.Globalization.CultureInfo.InvariantCulture),
                currency = "BTC",
                metadata = new { orderId }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/v1/stores/{_storeId}/invoices", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var invoiceId = doc.RootElement.GetProperty("id").GetString();
            var paymentAddress = doc.RootElement.GetProperty("checkoutLink").GetString();

            return (paymentAddress ?? "", invoiceId);
        }

        public async Task<decimal> GetReceivedAmountAsync(string invoiceId)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_btcpayUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _apiKey);

            var response = await client.GetAsync($"/api/v1/stores/{_storeId}/invoices/{invoiceId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var amountPaid = doc.RootElement.GetProperty("amountPaid").GetDecimal();

            return amountPaid;
        }
    }
}