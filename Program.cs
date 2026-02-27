using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using DarkMarket;
using DarkMarket.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DarkMarket.Models;
using DarkMarket.Services;
using System.Text.Json;
using DarkMarket.Config;
using DarkMarket.Hubs;
using Microsoft.AspNetCore.SignalR;
using DarkMarket.Enums;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var userSecretsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Microsoft",
    "UserSecrets",
    "55e5ddc6-76ae-4a68-aee1-b6f0e240e1d5",
    "secrets.json"
);

builder.Configuration.AddJsonFile(userSecretsPath, optional: true, reloadOnChange: true);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<BitcoinQuoteService>();
builder.Services.AddSingleton<CryptoQuoteService>();

builder.Services.AddScoped<IBitcoinPaymentService, BtcPayServerPaymentService>();
builder.Services.AddScoped<IBitcoinPaymentService, TestnetBitcoinPaymentService>();
builder.Services.AddScoped<BitcoinPaymentFactory>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<GatewayService>();
builder.Services.AddScoped<PaymentConfirmationService>();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection) || defaultConnection.Contains("__SET_VIA_USER_SECRETS__"))
{
    throw new InvalidOperationException(
        "DefaultConnection não configurada. Defina em User Secrets com: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=localhost;Port=5432;Database=darkmarket;Username=freeza;Password=...\" --project .\\DarkMarket.csproj"
    );
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<BtcPayOptions>(builder.Configuration.GetSection("BtcPay"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.MapPost("/api/btcpay/webhook", async (HttpContext context, AppDbContext db, LogService log) =>
{
    await log.LogAsync($"Webhook chamado.", source: "Webhook", level: "Info");

    var config = context.RequestServices.GetRequiredService<IConfiguration>();
    var expectedSecret = config["BtcPay:WebhookSecret"];
    var receivedSecret = context.Request.Headers["X-BTCPay-Secret"].FirstOrDefault();

    if (string.IsNullOrEmpty(expectedSecret) || receivedSecret != expectedSecret)
    {
        await log.LogAsync(
            $"Tentativa de acesso negada ao webhook. Header recebido: '{receivedSecret ?? "null"}'. IP: {context.Connection.RemoteIpAddress}",
            source: "Webhook",
            level: "Warning"
        );
        return Results.Unauthorized();
    }

    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    await log.LogAsync($"Webhook chamado. Body recebido: {body}", source: "Webhook", level: "Info");

    using var doc = JsonDocument.Parse(body);
    var invoiceId = doc.RootElement.GetProperty("invoiceId").GetString();
    var status = doc.RootElement.GetProperty("type").GetString(); // Ex: "InvoiceSettled"

    await log.LogAsync($"Webhook recebido: status={status}, invoiceId={invoiceId}", source: "Webhook", level: "Info");

    if (status == "InvoiceSettled" && !string.IsNullOrEmpty(invoiceId))
    {
        var payment = db.Payments.Include(p => p.Product).FirstOrDefault(p => p.PaymentId == invoiceId);

        if (payment == null)
        {
            await log.LogAsync($"Pagamento não encontrado para invoiceId={invoiceId}", source: "Webhook", level: "Warning");
        }
        else if (payment.IsPaid)
        {
            await log.LogAsync($"Pagamento já está marcado como pago para invoiceId={invoiceId}", source: "Webhook", level: "Info");
        }
        else
        {
            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await log.LogAsync(
                $"Preparando para criar pedido: paymentId={payment.Id}, userId={payment.UserId}, productId={payment.ProductId}, productUserId={payment.Product?.UserId}",
                source: "Webhook",
                level: "Info"
            );

            // Criação do pedido após pagamento confirmado
            var order = new OrderModel
            {
                BuyerId = payment.UserId ?? string.Empty,
                SellerId = payment.Product?.UserId ?? string.Empty,
                ProductId = payment.ProductId,
                Amount = payment.Amount,
                IsPaid = true,
                PaymentId = payment.Id,
                Status = PaymentStatus.AguardandoEntrega,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                payment.OrderId = order.Id;
                db.Payments.Update(payment);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await log.LogAsync($"Erro ao criar pedido: {ex.Message}", source: "Webhook", level: "Error");
            }

            await log.LogAsync(
                $"Pagamento confirmado automaticamente via webhook para invoice {invoiceId}. Pedido criado: {order.Id}",
                source: "Webhook",
                level: "Info",
                userId: payment.UserId
            );

            var hubContext = app.Services.GetRequiredService<IHubContext<PaymentHub>>();
            if (!string.IsNullOrEmpty(payment.UserId))
            {
                await hubContext.Clients.User(payment.UserId).SendAsync("PaymentConfirmed", payment.PaymentId);
            }
        }
    }

    return Results.Ok();
});

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    string[] roles = new[] { "admin", "user" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = config["AdminSeed:Email"] ?? "god@god";
    var adminPassword = config["AdminSeed:Password"];
    var adminFullName = config["AdminSeed:FullName"] ?? "Administrator";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null && !string.IsNullOrWhiteSpace(adminPassword))
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = adminFullName
        };

        var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createAdminResult.Succeeded)
        {
            var errors = string.Join("; ", createAdminResult.Errors.Select(e => e.Description));
            Console.WriteLine($"Falha ao criar usuário admin seed ({adminEmail}): {errors}");
            adminUser = null;
        }
        else
        {
            Console.WriteLine($"Usuário admin seed criado: {adminEmail}");
        }
    }

    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "admin");
        Console.WriteLine($"Usuário {adminEmail} promovido a admin.");
    }
}

// qual a melhor maneira de fazermos isso?
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Limpa todos os gateways antigos
    db.Gateways.RemoveRange(db.Gateways);
    db.SaveChanges();

    // Adiciona os gateways padronizados
    db.Gateways.AddRange(
        new GatewayInfo { Name = "BTCPayServer", Enabled = true },
        new GatewayInfo { Name = "Testnet", Enabled = true }
    );
    db.SaveChanges();
}

app.MapHub<PaymentHub>("/paymentHub");
app.Run();