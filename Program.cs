using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using DarkMarket;
using DarkMarket.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<DarkMarket.Services.CustomAuthStateProvider>();
builder.Services.AddScoped<DarkMarket.Services.AuthService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, DarkMarket.Services.CustomAuthStateProvider>();
builder.Services.AddScoped<DarkMarket.Services.ProductService>();
builder.Services.AddScoped<DarkMarket.Services.UserService>();
// builder.Services.AddScoped<DarkMarket.Services.OrderService>();
// builder.Services.AddScoped<DarkMarket.Services.CartService>();
// builder.Services.AddScoped<DarkMarket.Services.NotificationService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();