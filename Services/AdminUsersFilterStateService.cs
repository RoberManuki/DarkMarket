using Microsoft.JSInterop;
using System.Globalization;

namespace DarkMarket.Services;

public sealed class AdminUsersFilterState
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? Page { get; init; }
}

public class AdminUsersFilterStateService
{
    private readonly IJSRuntime _js;

    public AdminUsersFilterStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AdminUsersFilterState> LoadAsync()
    {
        try
        {
            var userName = await _js.InvokeAsync<string?>("localStorage.getItem", AdminUsersStorageKeys.UserNameFilter) ?? string.Empty;
            var email = await _js.InvokeAsync<string?>("localStorage.getItem", AdminUsersStorageKeys.EmailFilter) ?? string.Empty;
            var role = await _js.InvokeAsync<string?>("localStorage.getItem", AdminUsersStorageKeys.RoleFilter) ?? string.Empty;
            var status = await _js.InvokeAsync<string?>("localStorage.getItem", AdminUsersStorageKeys.StatusFilter) ?? string.Empty;

            int? page = null;
            var pageRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminUsersStorageKeys.Page);
            if (int.TryParse(pageRaw, out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
            }

            return new AdminUsersFilterState
            {
                UserName = userName,
                Email = email,
                Role = role,
                Status = status,
                Page = page
            };
        }
        catch
        {
            return new AdminUsersFilterState();
        }
    }

    public async Task SaveAsync(AdminUsersFilterState state)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AdminUsersStorageKeys.UserNameFilter, state.UserName);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminUsersStorageKeys.EmailFilter, state.Email);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminUsersStorageKeys.RoleFilter, state.Role);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminUsersStorageKeys.StatusFilter, state.Status);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminUsersStorageKeys.Page, (state.Page ?? 1).ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // No-op: ignore storage failures.
        }
    }
}