using System.Net.Http;
using System.Net.Http.Json;
using DarkMarket.Models;
using DarkMarket.Shared;
using Microsoft.AspNetCore.Components;

namespace DarkMarket.Services
{
    public class BitcoinQuoteService
    {
        private readonly HttpClient _http;

        public BitcoinQuoteService(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        private BitcoinQuote? _cachedQuote;
        private DateTime _lastFetch = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2);

        public async Task<BitcoinQuote?> GetQuoteAsync()
        {
            if (_cachedQuote != null && DateTime.Now - _lastFetch < _cacheDuration)
                return _cachedQuote;

            try
            {
                var url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl,usd";
                var result = await _http.GetFromJsonAsync<CoinGeckoResponse>(url);

                if (result?.bitcoin == null)
                    return null;

                _cachedQuote = new BitcoinQuote
                {
                    btc_brl = result.bitcoin.brl,
                    btc_usd = result.bitcoin.usd
                };

                _lastFetch = DateTime.Now;

                return _cachedQuote;
            }
            catch
            {
                return _cachedQuote; // Retorna último valor em caso de erro
            }
        }
    }
}