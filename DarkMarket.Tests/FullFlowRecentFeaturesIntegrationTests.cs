using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DarkMarket.Tests;

public class FullFlowRecentFeaturesIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public FullFlowRecentFeaturesIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LanguageFlow_AnonymousOrdersPage_RendersEnglish_WhenCookieIsSetByRoute()
    {
        var cookieHeader = await SetLanguageAndExtractCookieAsync("en-US", "/orders");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var response = await client.GetAsync("/orders");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("You are not authorized to access this page.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_AnonymousOrdersPage_RendersSpanish_WhenCookieIsSetByRoute()
    {
        var cookieHeader = await SetLanguageAndExtractCookieAsync("es-ES", "/orders");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var response = await client.GetAsync("/orders");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No tienes permiso para acceder a esta pagina.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_AnonymousOrdersPage_FallsBackToPortuguese_WhenLanguageIsInvalid()
    {
        var cookieHeader = await SetLanguageAndExtractCookieAsync("fr-FR", "/orders");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var response = await client.GetAsync("/orders");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Voce nao tem permissao para acessar esta pagina.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_AnonymousAdminPage_RendersEnglishNotAuthorized_WhenCookieIsEnglish()
    {
        var cookieHeader = await SetLanguageAndExtractCookieAsync("en-US", "/admin");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var response = await client.GetAsync("/admin");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("You are not authorized to access this page.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_ManualInvalidLanguageCookie_FallsBackToPortuguese()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "darkmarket.uiLanguage=fr-FR");

        var response = await client.GetAsync("/orders");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Voce nao tem permissao para acessar esta pagina.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SetLanguage_UnsafeReturnUrl_RedirectsHome_AndStillSetsLanguageCookie()
    {
        using var languageClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await languageClient.GetAsync("/set-language/en-US?returnUrl=%2F%2Fevil.example");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
        Assert.Contains(response.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("darkmarket.uiLanguage=en-US", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task CookieBanner_RendersEnglishTexts_WhenUiLanguageCookieIsEnglish()
    {
        var cookieHeader = await SetLanguageAndExtractCookieAsync("en-US", "/");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Cookie usage", html, StringComparison.Ordinal);
        Assert.Contains("Customize", html, StringComparison.Ordinal);
        Assert.Contains("Reject optional", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_QueryHintWithoutCookie_RendersEnglishOnOrders()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/orders?uiLang=en-US");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("You are not authorized to access this page.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LanguageFlow_QueryHintWithoutCookie_RendersSpanishOnOrders()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/orders?uiLang=es-ES");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No tienes permiso para acceder a esta pagina.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CookieConsent_AssetsAndMarkup_ArePublishedWithExpectedContracts()
    {
        using var client = _factory.CreateClient();

        var hostResponse = await client.GetAsync("/");
        var hostHtml = await hostResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, hostResponse.StatusCode);
        Assert.Contains("cookie-consent-banner", hostHtml, StringComparison.Ordinal);
        Assert.Contains("cookie-consent-modal", hostHtml, StringComparison.Ordinal);
        Assert.Contains("/js/cookie-consent.js", hostHtml, StringComparison.Ordinal);
        Assert.Contains("darkMarketIsDevelopment", hostHtml, StringComparison.Ordinal);

        var scriptResponse = await client.GetAsync("/js/cookie-consent.js");
        var script = await scriptResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, scriptResponse.StatusCode);
        Assert.Contains("window.darkMarketCookieConsent", script, StringComparison.Ordinal);
        Assert.Contains("function init()", script, StringComparison.Ordinal);
        Assert.Contains("function acceptAll()", script, StringComparison.Ordinal);
        Assert.Contains("function rejectOptional()", script, StringComparison.Ordinal);
        Assert.Contains("function savePreferences()", script, StringComparison.Ordinal);
        Assert.Contains("darkmarket.cookieConsent.v1", script, StringComparison.Ordinal);
    }

    private async Task<string> SetLanguageAndExtractCookieAsync(string languageCode, string returnUrl)
    {
        using var languageClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
        var response = await languageClient.GetAsync($"/set-language/{Uri.EscapeDataString(languageCode)}?returnUrl={encodedReturnUrl}");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(returnUrl, response.Headers.Location?.ToString());

        var setCookie = response.Headers
            .GetValues("Set-Cookie")
            .First(v => v.Contains("darkmarket.uiLanguage=", StringComparison.Ordinal));

        return setCookie.Split(';', 2)[0];
    }
}
