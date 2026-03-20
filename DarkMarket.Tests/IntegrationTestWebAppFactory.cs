using DarkMarket.Data;
using DarkMarket.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DarkMarket.Tests;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"darkmarket-int-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=darkmarket_tests;Username=test;Password=test",
                ["AdminSeed:Email"] = "admin@test.local",
                ["AdminSeed:Password"] = "Admin123!Aa",
                ["AdminSeed:FullName"] = "Admin Test",
                ["BtcPay:WebhookSecret"] = "expected-secret",
                ["BtcPay:WebhookMaxBodyBytes"] = "256"
            };

            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            var dbContextOptionsDescriptors = services
                .Where(service => service.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

            foreach (var descriptor in dbContextOptionsDescriptors)
                services.Remove(descriptor);

            var dbContextOptionsConfigType = typeof(IDbContextOptionsConfiguration<AppDbContext>);
            var dbContextOptionsConfigDescriptors = services
                .Where(service => service.ServiceType == dbContextOptionsConfigType)
                .ToList();

            foreach (var descriptor in dbContextOptionsConfigDescriptors)
                services.Remove(descriptor);

            var dbContextDescriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(AppDbContext));

            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<BitcoinQuoteService>();
            services.RemoveAll<CryptoQuoteService>();

            var quoteHttpFactory = new StubHttpClientFactory(_ =>
                HttpTestResponses.Json("{\"bitcoin\":{\"brl\":500000,\"usd\":100000},\"ethereum\":{\"brl\":15000,\"usd\":3000},\"solana\":{\"brl\":800,\"usd\":160}}"));

            services.AddSingleton<BitcoinQuoteService>(provider =>
                new BitcoinQuoteService(
                    quoteHttpFactory,
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetService<IServiceScopeFactory>()));

            services.AddSingleton<CryptoQuoteService>(provider =>
                new CryptoQuoteService(
                    quoteHttpFactory,
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetService<IServiceScopeFactory>()));

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ =>
                    {
                    });
        });
    }
}
