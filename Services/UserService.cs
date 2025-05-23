using Microsoft.AspNetCore.Identity;
using DarkMarket.Models;

public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetUserByNameAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return null;
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}