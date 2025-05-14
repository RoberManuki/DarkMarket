using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Builder;
using DarkMarket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseAntiforgery();
app.UseStaticFiles(); // Garante que wwwroot seja servido

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();