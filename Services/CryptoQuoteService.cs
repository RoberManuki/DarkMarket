using System.Net.Http;
using System.Net.Http.Json;
using DarkMarket.Models;

namespace DarkMarket.Services
{
    public class CryptoQuoteService
    {
        private readonly HttpClient _http;
        private readonly Dictionary<string, (CryptoQuote? quote, DateTime lastFetch)> _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2);

        public CryptoQuoteService(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _cache = new Dictionary<string, (CryptoQuote?, DateTime)>();
        }

        public async Task<CryptoQuote?> GetQuoteAsync(string cryptoId, string symbol, string name)
        {
            if (_cache.TryGetValue(cryptoId, out var cached) && 
                cached.quote != null && 
                DateTime.Now - cached.lastFetch < _cacheDuration)
            {
                return cached.quote;
            }

            try
            {
                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={cryptoId}&vs_currencies=brl,usd";
                var result = await _http.GetFromJsonAsync<Dictionary<string, CoinGeckoPrice>>(url);

                if (result == null || !result.ContainsKey(cryptoId))
                    return GetCachedQuote(cryptoId);

                var priceData = result[cryptoId];
                var quote = new CryptoQuote
                {
                    PriceBrl = priceData.Brl,
                    PriceUsd = priceData.Usd,
                    Symbol = symbol,
                    Name = name
                };

                _cache[cryptoId] = (quote, DateTime.Now);

                return quote;
            }
            catch
            {
                return GetCachedQuote(cryptoId);
            }
        }

        private CryptoQuote? GetCachedQuote(string cryptoId)
        {
            return _cache.TryGetValue(cryptoId, out var cached) ? cached.quote : null;
        }
    }
}
