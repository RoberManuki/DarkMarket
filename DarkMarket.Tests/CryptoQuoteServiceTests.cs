using System.Net;
using DarkMarket.Services;

namespace DarkMarket.Tests;

public class CryptoQuoteServiceTests
{
    [Fact]
    public async Task GetQuoteAsync_ReturnsNull_WhenCryptoIdIsBlank()
    {
        var service = new CryptoQuoteService(
            new StubHttpClientFactory(_ => throw new InvalidOperationException("HTTP should not be called.")),
            TestConfigurationFactory.Create());

        var result = await service.GetQuoteAsync(" ", "BTC", "Bitcoin");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetQuoteAsync_UsesNormalizedIdAndCache()
    {
        var calls = 0;
        var service = new CryptoQuoteService(
            new StubHttpClientFactory(_ =>
            {
                calls++;
                return HttpTestResponses.Json("{\"bitcoin\":{\"brl\":100,\"usd\":20}}");
            }),
            TestConfigurationFactory.Create());

        var first = await service.GetQuoteAsync(" BitCoin ", "BTC", "Bitcoin");
        var second = await service.GetQuoteAsync("bitcoin", "BTC", "Bitcoin");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(100m, first!.PriceBrl);
        Assert.Equal(20m, first.PriceUsd);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetQuoteAsync_RetriesWithApiKey_WhenForbidden()
    {
        var calls = 0;
        var service = new CryptoQuoteService(
            new StubHttpClientFactory(request =>
            {
                calls++;
                if (!request.Headers.Contains("x-cg-demo-api-key"))
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }

                return HttpTestResponses.Json("{\"ethereum\":{\"brl\":200,\"usd\":40}}");
            }),
            TestConfigurationFactory.Create(
                ("CoinGecko:ApiKey", "demo-key"),
                ("CoinGecko:ApiHeaderName", "x-cg-demo-api-key")));

        var quote = await service.GetQuoteAsync("ethereum", "ETH", "Ethereum");

        Assert.NotNull(quote);
        Assert.Equal(200m, quote!.PriceBrl);
        Assert.Equal(40m, quote.PriceUsd);
        Assert.Equal(2, calls);
    }
}