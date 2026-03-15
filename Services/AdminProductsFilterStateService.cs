using Microsoft.JSInterop;
using System.Globalization;

namespace DarkMarket.Services;

public sealed class AdminProductsFilterState
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? Page { get; init; }
}

public class AdminProductsFilterStateService
{
    private readonly IJSRuntime _js;

    public AdminProductsFilterStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AdminProductsFilterState> LoadAsync()
    {
        try
        {
            int? id = null;
            var idRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.IdFilter);
            if (int.TryParse(idRaw, out var parsedId) && parsedId >= 0)
            {
                id = parsedId;
            }

            var name = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.NameFilter) ?? string.Empty;
            var userId = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.UserFilter) ?? string.Empty;

            decimal? minPrice = null;
            var minPriceRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.MinPriceFilter);
            if (decimal.TryParse(minPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMinPrice))
            {
                minPrice = parsedMinPrice;
            }

            decimal? maxPrice = null;
            var maxPriceRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.MaxPriceFilter);
            if (decimal.TryParse(maxPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMaxPrice))
            {
                maxPrice = parsedMaxPrice;
            }

            int? page = null;
            var pageRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminProductsStorageKeys.Page);
            if (int.TryParse(pageRaw, out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
            }

            return new AdminProductsFilterState
            {
                Id = id,
                Name = name,
                UserId = userId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Page = page
            };
        }
        catch
        {
            return new AdminProductsFilterState();
        }
    }

    public async Task SaveAsync(AdminProductsFilterState state)
    {
        try
        {
            if (state.Id.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.IdFilter, state.Id.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminProductsStorageKeys.IdFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.NameFilter, state.Name);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.UserFilter, state.UserId);

            if (state.MinPrice.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.MinPriceFilter, state.MinPrice.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminProductsStorageKeys.MinPriceFilter);
            }

            if (state.MaxPrice.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.MaxPriceFilter, state.MaxPrice.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminProductsStorageKeys.MaxPriceFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminProductsStorageKeys.Page, (state.Page ?? 1).ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // No-op: ignore storage failures.
        }
    }
}