using System.Net;
using System.Text.RegularExpressions;
using DarkMarket.Data;
using DarkMarket.Models;
using DarkMarket.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DarkMarket.Tests;

public class AuthenticationIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthenticationIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Lockout_AfterFiveFailedLoginAttempts()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        const string lockoutIdentity = "lockout@test.local";
        var user = new ApplicationUser
        {
            UserName = lockoutIdentity,
            Email = lockoutIdentity,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, "Lockout123!");
        Assert.True(createResult.Succeeded);

        SignInResult? lastResult = null;
        for (int i = 0; i < 5; i++)
        {
            lastResult = await signInManager.PasswordSignInAsync(lockoutIdentity, "WrongPassword!", isPersistent: false, lockoutOnFailure: true);
        }

        Assert.NotNull(lastResult);
        Assert.True(lastResult!.IsLockedOut);

        var refreshedUser = await userManager.FindByNameAsync(lockoutIdentity);
        Assert.NotNull(refreshedUser);
        Assert.True(await userManager.IsLockedOutAsync(refreshedUser!));
    }

    [Fact]
    public async Task IsNotAllowed_WhenEmailNotConfirmed_ProductionConfig()
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var requireConfirmedSetting = await db.AppSettings.FindAsync(AdminSecurityPolicyService.RequireConfirmedEmailKey);
        if (requireConfirmedSetting is null)
        {
            db.AppSettings.Add(new AppSetting
            {
                Key = AdminSecurityPolicyService.RequireConfirmedEmailKey,
                Value = "true"
            });
        }
        else
        {
            requireConfirmedSetting.Value = "true";
        }

        await db.SaveChangesAsync();

        const string unconfirmedIdentity = "unconfirmed@test.local";
        var user = new ApplicationUser { UserName = unconfirmedIdentity, Email = unconfirmedIdentity, EmailConfirmed = false };
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await userManager.CreateAsync(user, "Unconfirmed123!");

        var response = await PostLoginFormAsync(client, unconfirmedIdentity, "Unconfirmed123!", rememberMe: false);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("confirme seu e-mail", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AllowsLoginWithoutConfirmedEmail_WhenRuntimePolicyDisablesRequirement()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var currentFlag = await db.AppSettings.FindAsync(AdminSecurityPolicyService.RequireConfirmedEmailKey);
        if (currentFlag is null)
        {
            db.AppSettings.Add(new AppSetting
            {
                Key = AdminSecurityPolicyService.RequireConfirmedEmailKey,
                Value = "false"
            });
        }
        else
        {
            currentFlag.Value = "false";
        }

        const string identity = "runtime-unconfirmed@test.local";
        var user = new ApplicationUser
        {
            UserName = identity,
            Email = identity,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        await db.SaveChangesAsync();

        var createResult = await userManager.CreateAsync(user, "Runtime123!");
        Assert.True(createResult.Succeeded);

        var response = await PostLoginFormAsync(client, identity, "Runtime123!", rememberMe: false);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/dashboard", response.Headers.Location?.OriginalString);
    }

    private static async Task<HttpResponseMessage> PostLoginFormAsync(HttpClient client, string email, string password, bool rememberMe)
    {
        var getResponse = await client.GetAsync("/Identity/Account/Login");
        getResponse.EnsureSuccessStatusCode();

        var pageHtml = await getResponse.Content.ReadAsStringAsync();
        var verificationToken = ExtractVerificationToken(pageHtml);

        var form = new FormUrlEncodedContent(new[]
        {
            KeyValuePair.Create("Input.Email", email),
            KeyValuePair.Create("Input.Password", password),
            KeyValuePair.Create("Input.RememberMe", rememberMe ? "true" : "false"),
            KeyValuePair.Create("__RequestVerificationToken", verificationToken)
        });

        return await client.PostAsync("/Identity/Account/Login", form);
    }

    private static string ExtractVerificationToken(string html)
    {
        var match = Regex.Match(
            html,
            "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Could not extract antiforgery token from login page.");
        return match.Groups["token"].Value;
    }
}
