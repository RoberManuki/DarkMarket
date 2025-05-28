using NBitcoin;
using System.Net.Http.Json;
using System.Text.Json;

namespace DarkMarket.Services
{
    public class BitcoinPaymentService
    {
        public (string Address, string PrivateKey) GenerateTestnetAddress()
        {
            var network = Network.TestNet;
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, network).ToString();
            var wif = key.GetWif(network).ToString();
            return (address, wif);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em GetReceivedAmountAsync: {ex.Message}");
                throw;
            }
        }
    }
}