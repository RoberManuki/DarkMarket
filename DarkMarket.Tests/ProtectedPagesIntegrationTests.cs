using System.Net;
using DarkMarket.Data;
using DarkMarket.Enums;
using DarkMarket.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DarkMarket.Tests;

public class ProtectedPagesIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public ProtectedPagesIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OrdersPage_ShowsNotAuthorized_WhenAnonymous()
    {
        var result = await GetPageAsync(_factory.CreateClient(), "/orders");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    [Fact]
    public async Task OrdersPage_RendersForAuthenticatedUser()
    {
        var client = CreateAuthenticatedClient(userId: "buyer-int", userName: "buyer");
        var result = await GetPageAsync(client, "/orders");
        AssertOkAndContains(result, "Meus Pedidos");
    }

    [Fact]
    public async Task AdminPage_ShowsNotAuthorized_ForNonAdminUser()
    {
        var client = CreateAuthenticatedClient(userId: "user-int", userName: "user", roles: "user");
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    [Fact]
    public async Task AdminPage_RendersForAdminRole()
    {
        var client = CreateAuthenticatedClient(userId: "admin-int", userName: "admin", roles: "admin");
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Painel Administrativo");
    }

    [Fact]
    public async Task AdminPage_ShowsNotAuthorized_WhenRoleHeaderIsBlank()
    {
        var client = CreateHeaderClient(("X-Test-UserId", "user-int-blank-role"), ("X-Test-UserName", "userblankrole"), ("X-Test-Roles", "   "));
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    [Fact]
    public async Task AdminPage_RendersForMixedRoleHeader_WhenAdminIsPresent()
    {
        var client = CreateHeaderClient(("X-Test-UserId", "admin-int-mixed-role"), ("X-Test-UserName", "adminmixedrole"), ("X-Test-Roles", "user, ,admin, "));
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Painel Administrativo");
    }

    [Fact]
    public async Task AdminPage_ShowsNotAuthorized_ForMixedRoleHeader_WithoutAdmin()
    {
        var client = CreateHeaderClient(("X-Test-UserId", "user-int-mixed-no-admin"), ("X-Test-UserName", "usermixednoadmin"), ("X-Test-Roles", "user, ,manager, "));
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    [Fact]
    public async Task AdminPage_RendersWhenAdminRoleIsDuplicated()
    {
        var client = CreateHeaderClient(("X-Test-UserId", "admin-int-dup-role"), ("X-Test-UserName", "admindupe"), ("X-Test-Roles", "admin,admin"));
        var result = await GetPageAsync(client, "/admin");
        AssertOkAndContains(result, "Painel Administrativo");
    }

    [Fact]
    public async Task OrderDetails_ShowsAccessDenied_ForUnrelatedUser()
    {
        var orderId = await SeedOrderAsync(buyerId: "buyer-allowed", sellerId: "seller-allowed");

        var client = CreateAuthenticatedClient(userId: "intruder-user", userName: "intruder", roles: "user");
        var result = await GetPageAsync(client, $"/orders/{orderId}");
        AssertOkAndContains(result, "Transação não encontrada ou acesso negado.");
    }

    [Fact]
    public async Task OrderDetails_ShowsNotAuthorized_WhenUserIdHeaderIsMissing()
    {
        var orderId = await SeedOrderAsync(buyerId: "buyer-allowed-2", sellerId: "seller-allowed-2");

        var client = CreateHeaderClient(("X-Test-UserName", "missing-userid"));
        var result = await GetPageAsync(client, $"/orders/{orderId}");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    [Fact]
    public async Task OrderDetails_ShowsNotAuthorized_WhenUserIdHeaderIsBlank()
    {
        var orderId = await SeedOrderAsync(buyerId: "buyer-allowed-3", sellerId: "seller-allowed-3");

        var client = CreateHeaderClient(("X-Test-UserId", "   "), ("X-Test-UserName", "blank-userid"));
        var result = await GetPageAsync(client, $"/orders/{orderId}");
        AssertOkAndContains(result, "Você não tem permissão para acessar esta página.");
    }

    private HttpClient CreateAuthenticatedClient(string userId, string userName, params string[] roles)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        client.DefaultRequestHeaders.Add("X-Test-UserName", userName);

        if (roles.Length > 0)
            client.DefaultRequestHeaders.Add("X-Test-Roles", string.Join(',', roles));

        return client;
    }

    private HttpClient CreateHeaderClient(params (string Name, string Value)[] headers)
    {
        var client = _factory.CreateClient();
        foreach (var (name, value) in headers)
            client.DefaultRequestHeaders.Add(name, value);

        return client;
    }

    private async Task<(HttpResponseMessage Response, string Html)> GetPageAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();
        return (response, html);
    }

    private static void AssertOkAndContains((HttpResponseMessage Response, string Html) result, string expectedText)
    {
        Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
        Assert.Contains(expectedText, result.Html);
    }

    private async Task<int> SeedOrderAsync(string buyerId, string sellerId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = new Product
        {
            Name = "Produto restrito",
            Description = "Produto para teste de autorização",
            ShortDescription = "Resumo",
            Price = 0.001m,
            UserId = sellerId
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var order = new OrderModel
        {
            BuyerId = buyerId,
            SellerId = sellerId,
            ProductId = product.Id,
            Amount = 0.001m,
            IsPaid = true,
            Status = PaymentStatus.Pago,
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
    }
}
