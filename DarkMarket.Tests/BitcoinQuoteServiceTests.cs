using System.Reflection;
using System.Net;
using DarkMarket.Models;
using DarkMarket.Services;

namespace DarkMarket.Tests;

public class BitcoinQuoteServiceTests
{
    [Fact]
    public async Task GetQuoteAsync_FetchesAndCachesQuote()
    {
        var calls = 0;
        var service = new BitcoinQuoteService(new StubHttpClientFactory(_ =>
        {
            calls++;
            return HttpTestResponses.Json("{\"bitcoin\":{\"brl\":500000,\"usd\":100000}}");
        }), TestConfigurationFactory.Create());

        var first = await service.GetQuoteAsync();
        var second = await service.GetQuoteAsync();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(500000m, first!.btc_brl);
        Assert.Equal(100000m, first.btc_usd);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetQuoteAsync_ReturnsCachedQuote_WhenHttpFails()
    {
        var service = new BitcoinQuoteService(new StubHttpClientFactory(_ => throw new HttpRequestException("fail")), TestConfigurationFactory.Create());

        SetPrivateField(service, "_cachedQuote", new BitcoinQuote { btc_brl = 1m, btc_usd = 2m });
        SetPrivateField(service, "_lastFetch", DateTime.UtcNow.AddMinutes(-10));

        var result = await service.GetQuoteAsync();

        Assert.NotNull(result);
        Assert.Equal(1m, result!.btc_brl);
        Assert.Equal(2m, result.btc_usd);
    }

    [Fact]
    public async Task GetQuoteAsync_RetriesWithApiKey_WhenForbidden()
    {
        var calls = 0;
        var service = new BitcoinQuoteService(new StubHttpClientFactory(request =>
        {
            calls++;
            if (!request.Headers.Contains("x-cg-demo-api-key"))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);

            return HttpTestResponses.Json("{\"bitcoin\":{\"brl\":400000,\"usd\":80000}}");
        }),
        TestConfigurationFactory.Create(
            ("CoinGecko:ApiKey", "demo-key"),
            ("CoinGecko:ApiHeaderName", "x-cg-demo-api-key")));

        var result = await service.GetQuoteAsync();

        Assert.NotNull(result);
        Assert.Equal(400000m, result!.btc_brl);
        Assert.Equal(80000m, result.btc_usd);
        Assert.Equal(2, calls);
    }

    private static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }
}