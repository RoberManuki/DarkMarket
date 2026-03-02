using DarkMarket.Data;
using DarkMarket.Models;
using Microsoft.AspNetCore.Identity;

namespace DarkMarket.Services
{
    public class AppInitializationService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        public AppInitializationService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            AppDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _db = db;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAndAdminAsync();
            await SeedGatewaysAsync();
        }

        private async Task SeedRolesAndAdminAsync()
        {
            string[] roles = new[] { "admin", "user" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = _configuration["AdminSeed:Email"] ?? "god@god";
            var adminPassword = _configuration["AdminSeed:Password"];
            var adminFullName = _configuration["AdminSeed:FullName"] ?? "Administrator";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null && !string.IsNullOrWhiteSpace(adminPassword))
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = adminFullName
                };

                var createAdminResult = await _userManager.CreateAsync(adminUser, adminPassword);
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

            if (adminUser != null && !await _userManager.IsInRoleAsync(adminUser, "admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "admin");
                Console.WriteLine($"Usuário {adminEmail} promovido a admin.");
            }
        }

        private async Task SeedGatewaysAsync()
        {
            var defaultGateways = new[] { "BTCPayServer", "Testnet" };
            var existingGatewayNames = _db.Gateways
                .Select(g => g.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var gatewayName in defaultGateways)
            {
                if (!existingGatewayNames.Contains(gatewayName))
                {
                    _db.Gateways.Add(new GatewayInfo
                    {
                        Name = gatewayName,
                        Enabled = true
                    });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}