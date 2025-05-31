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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<BitcoinQuoteService>();

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IBitcoinPaymentService, BtcPayServerPaymentService>();
builder.Services.AddScoped<IBitcoinPaymentService, TestnetBitcoinPaymentService>();
builder.Services.AddScoped<BitcoinPaymentFactory>();
builder.Services.AddScoped<LogService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    using var doc = JsonDocument.Parse(body);
    var invoiceId = doc.RootElement.GetProperty("invoiceId").GetString();
    var status = doc.RootElement.GetProperty("type").GetString(); // Ex: "InvoiceSettled"

    if (status == "InvoiceSettled" && !string.IsNullOrEmpty(invoiceId))
    {
        var payment = db.Payments.FirstOrDefault(p => p.PaymentId == invoiceId);
        if (payment != null && !payment.IsPaid)
        {
            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await log.LogAsync(
                $"Pagamento confirmado automaticamente via webhook para invoice {invoiceId}.",
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
    string[] roles = new[] { "admin", "user" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// using (var scope = app.Services.CreateScope())
// {
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//     var adminEmail = "teste6@teste6.com.br"; // coloque o email do usuário que deseja promover
//     var user = await userManager.FindByEmailAsync(adminEmail);
//     if (user != null && !await userManager.IsInRoleAsync(user, "admin"))
//     {
//         await userManager.AddToRoleAsync(user, "admin");
//         Console.WriteLine($"Usuário {adminEmail} promovido a admin.");
//     }
// }

app.MapHub<PaymentHub>("/paymentHub");

app.Run();