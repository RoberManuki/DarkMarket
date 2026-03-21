using DarkMarket;
using DarkMarket.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DarkMarket.Models;
using DarkMarket.Services;
using DarkMarket.Config;
using DarkMarket.Configuration;
using DarkMarket.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

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
builder.Services.AddScoped<OrderAccessService>();
builder.Services.AddScoped<AppInitializationService>();
builder.Services.AddScoped<BtcPayWebhookService>();
builder.Services.AddScoped<CurrencyPreferenceService>();
builder.Services.AddScoped<LanguagePreferenceService>();
builder.Services.AddScoped<UiTextService>();
builder.Services.AddScoped<DashboardMetricsService>();
builder.Services.AddScoped<AdminSettingsService>();
builder.Services.AddScoped<OperationFeeCalculatorService>();
builder.Services.AddScoped<AdminOrderReleaseService>();
builder.Services.AddScoped<AdminLogsQueryService>();
builder.Services.AddScoped<AdminLogsExportService>();
builder.Services.AddScoped<AdminLogsFilterStateService>();
builder.Services.AddScoped<AdminUsersFilterStateService>();
builder.Services.AddScoped<AdminPaymentsFilterStateService>();
builder.Services.AddScoped<AdminOrdersFilterStateService>();
builder.Services.AddScoped<AdminProductsFilterStateService>();
builder.Services.AddScoped<IEmailSender, IdentityEmailSender>();
builder.Services.AddScoped<AdminSecurityPolicyService>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection) || defaultConnection.Contains("__SET_VIA_USER_SECRETS__"))
{
    throw new InvalidOperationException(
        "DefaultConnection não configurada. Defina em User Secrets com: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=localhost;Port=5432;Database=darkmarket;Username=freeza;Password=...\" --project .\\DarkMarket.csproj"
    );
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

var isDevelopment = builder.Environment.IsDevelopment();
var securityPolicy = SecurityPolicyDefaults.Create(isDevelopment);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = securityPolicy.RequireConfirmedEmail;

        options.Password.RequiredLength = securityPolicy.PasswordRequiredLength;
        options.Password.RequireDigit = securityPolicy.PasswordRequireDigit;
        options.Password.RequireLowercase = securityPolicy.PasswordRequireLowercase;
        options.Password.RequireUppercase = securityPolicy.PasswordRequireUppercase;
        options.Password.RequireNonAlphanumeric = securityPolicy.PasswordRequireNonAlphanumeric;
        options.Password.RequiredUniqueChars = securityPolicy.PasswordRequiredUniqueChars;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = securityPolicy.LockoutMaxFailedAccessAttempts;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(securityPolicy.LockoutMinutes);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(securityPolicy.SessionTimeoutMinutes);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/Login";
});

builder.Services.Configure<BtcPayOptions>(builder.Configuration.GetSection("BtcPay"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    var languagePreference = context.RequestServices.GetRequiredService<LanguagePreferenceService>();
    context.Request.Cookies.TryGetValue("darkmarket.uiLanguage", out var languageFromCookie);
    languagePreference.SetLanguage(languageFromCookie);
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/set-language/{languageCode}", (HttpContext context, string languageCode, string? returnUrl) =>
{
    var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "pt-BR",
        "en-US",
        "es-ES"
    };

    var normalized = string.IsNullOrWhiteSpace(languageCode) ? LanguagePreferenceService.DefaultLanguage : languageCode.Trim();
    if (!supported.Contains(normalized))
    {
        normalized = LanguagePreferenceService.DefaultLanguage;
    }

    context.Response.Cookies.Append("darkmarket.uiLanguage", normalized, new CookieOptions
    {
        Path = "/",
        HttpOnly = false,
        IsEssential = true,
        Secure = !isDevelopment,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(365)
    });

    var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    if (!Uri.TryCreate(target, UriKind.Relative, out _) || target.StartsWith("//", StringComparison.Ordinal))
    {
        target = "/";
    }
    else if (!target.StartsWith('/'))
    {
        target = "/" + target;
    }

    return Results.LocalRedirect(target);
});

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.MapPost("/api/btcpay/webhook", async (HttpContext context, BtcPayWebhookService webhookService) =>
{
    return await webhookService.HandleAsync(context);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    var initializer = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await initializer.SeedAsync();
}

app.MapHub<PaymentHub>("/paymentHub");
app.Run();

public partial class Program { }