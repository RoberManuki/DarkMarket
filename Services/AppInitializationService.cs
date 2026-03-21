using DarkMarket.Data;
using DarkMarket.Configuration;
using DarkMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DarkMarket.Services
{
    public class AppInitializationService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _db;
        private readonly ILogger<AppInitializationService> _logger;

        public AppInitializationService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            AppDbContext db,
            ILogger<AppInitializationService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _environment = environment;
            _db = db;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAndAdminAsync();
            await SeedGatewaysAsync();
            await SeedAdminSettingsAsync();
            await SeedDeliveryAgentsAsync();
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
            var syncAdminPassword = _configuration.GetValue<bool?>("AdminSeed:SyncPassword") ?? _environment.IsDevelopment();

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
                    _logger.LogError("Falha ao criar usuário admin seed ({AdminEmail}): {Errors}", adminEmail, errors);
                    adminUser = null;
                }
                else
                {
                    _logger.LogInformation("Usuário admin seed criado: {AdminEmail}", adminEmail);
                }
            }

            if (adminUser != null && !await _userManager.IsInRoleAsync(adminUser, "admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "admin");
                _logger.LogInformation("Usuário {AdminEmail} promovido a admin.", adminEmail);
            }

            if (adminUser != null && !string.IsNullOrWhiteSpace(adminPassword) && syncAdminPassword)
            {
                var isExpectedPassword = await _userManager.CheckPasswordAsync(adminUser, adminPassword);
                if (!isExpectedPassword)
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetPasswordResult = await _userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);

                    if (!resetPasswordResult.Succeeded)
                    {
                        var errors = string.Join("; ", resetPasswordResult.Errors.Select(e => e.Description));
                        _logger.LogError("Falha ao sincronizar senha do admin seed ({AdminEmail}): {Errors}", adminEmail, errors);
                    }
                    else
                    {
                        _logger.LogInformation("Senha do admin seed sincronizada para {AdminEmail}.", adminEmail);
                    }
                }
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

        private async Task SeedAdminSettingsAsync()
        {
            var hasOperationFee = await _db.AppSettings
                .AnyAsync(s => s.Key == AdminSettingsService.OperationFeePercentKey);

            if (!hasOperationFee)
            {
                _db.AppSettings.Add(new AppSetting
                {
                    Key = AdminSettingsService.OperationFeePercentKey,
                    Value = AdminSettingsService.DefaultOperationFeePercent
                        .ToString("0.##", CultureInfo.InvariantCulture)
                });

                await _db.SaveChangesAsync();
            }

            var defaults = SecurityPolicyDefaults.Create(_environment.IsDevelopment());

            await EnsureAppSettingAsync(AdminSecurityPolicyService.RequireConfirmedEmailKey, defaults.RequireConfirmedEmail ? "true" : "false");
            await EnsureAppSettingAsync(AdminSecurityPolicyService.LockoutMaxAttemptsKey, defaults.LockoutMaxFailedAccessAttempts.ToString(CultureInfo.InvariantCulture));
            await EnsureAppSettingAsync(AdminSecurityPolicyService.LockoutMinutesKey, defaults.LockoutMinutes.ToString(CultureInfo.InvariantCulture));

            await _db.SaveChangesAsync();
        }

        private async Task EnsureAppSettingAsync(string key, string value)
        {
            var exists = await _db.AppSettings.AnyAsync(s => s.Key == key);
            if (exists)
            {
                return;
            }

            _db.AppSettings.Add(new AppSetting
            {
                Key = key,
                Value = value
            });
        }

        private async Task SeedDeliveryAgentsAsync()
        {
            if (await _db.DeliveryAgents.AnyAsync())
            {
                return;
            }

            _db.DeliveryAgents.AddRange(
                new DeliveryAgent
                {
                    Name = "Equipe Centro",
                    Contact = "centro@darkmarket.local",
                    EstimatedBusinessDays = 2,
                    IsActive = true
                },
                new DeliveryAgent
                {
                    Name = "Equipe Express",
                    Contact = "express@darkmarket.local",
                    EstimatedBusinessDays = 1,
                    IsActive = true
                }
            );

            await _db.SaveChangesAsync();
        }
    }
}