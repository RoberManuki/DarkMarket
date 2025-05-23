using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using DarkMarket;
using DarkMarket.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DarkMarket.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<DarkMarket.Services.ProductService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

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

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "teste6@teste6.com.br"; // coloque o email do usuário que deseja promover
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user != null && !await userManager.IsInRoleAsync(user, "admin"))
    {
        await userManager.AddToRoleAsync(user, "admin");
        Console.WriteLine($"Usuário {adminEmail} promovido a admin.");
    }
}

app.Run();