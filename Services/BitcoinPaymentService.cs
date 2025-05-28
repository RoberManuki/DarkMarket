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
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Segwit, network).ToString();
            var wif = key.GetWif(network).ToString();
            return (address, wif);
        }

        public async Task<decimal> GetReceivedAmountAsync(string address)
        {
            using var http = new HttpClient();
            // Blockstream testnet API
            var url = $"https://blockstream.info/testnet/api/address/{address}";
            var json = await http.GetStringAsync(url);
            var received = JsonDocument.Parse(json)
                .RootElement.GetProperty("chain_stats")
                .GetProperty("funded_txo_sum")
                .GetInt64();
            // Convert satoshis to BTC
            return received / 100_000_000m;
        }
    }
}