using NBitcoin;

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
                var url = $"https://blockstream.info/testnet/api/address/{address}";
                var response = await http.GetFromJsonAsync<BlockstreamAddressInfo>(url);

                // O saldo é retornado em satoshis, converta para BTC
                return response?.chain_stats?.funded_txo_sum is long sats
                    ? sats / 100_000_000m
                    : 0m;
            }
    }
}