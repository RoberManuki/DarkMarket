using Microsoft.JSInterop;
using System.Globalization;

namespace DarkMarket.Services;

public sealed class AdminOrdersFilterState
{
    public int? Id { get; init; }
    public string Product { get; init; } = string.Empty;
    public string Buyer { get; init; } = string.Empty;
    public string Seller { get; init; } = string.Empty;
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? Date { get; init; }
    public int? Page { get; init; }
}

public class AdminOrdersFilterStateService
{
    private readonly IJSRuntime _js;

    public AdminOrdersFilterStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AdminOrdersFilterState> LoadAsync()
    {
        try
        {
            int? id = null;
            var idRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.IdFilter);
            if (int.TryParse(idRaw, out var parsedId) && parsedId >= 0)
            {
                id = parsedId;
            }

            var product = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.ProductFilter) ?? string.Empty;
            var buyer = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.BuyerFilter) ?? string.Empty;
            var seller = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.SellerFilter) ?? string.Empty;
            var status = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.StatusFilter) ?? string.Empty;

            decimal? minAmount = null;
            var minAmountRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.MinAmountFilter);
            if (decimal.TryParse(minAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMinAmount))
            {
                minAmount = parsedMinAmount;
            }

            decimal? maxAmount = null;
            var maxAmountRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.MaxAmountFilter);
            if (decimal.TryParse(maxAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMaxAmount))
            {
                maxAmount = parsedMaxAmount;
            }

            DateTime? date = null;
            var dateRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.DateFilter);
            if (DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDate))
            {
                date = parsedDate.Date;
            }

            int? page = null;
            var pageRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminOrdersStorageKeys.Page);
            if (int.TryParse(pageRaw, out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
            }

            return new AdminOrdersFilterState
            {
                Id = id,
                Product = product,
                Buyer = buyer,
                Seller = seller,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Status = status,
                Date = date,
                Page = page
            };
        }
        catch
        {
            return new AdminOrdersFilterState();
        }
    }

    public async Task SaveAsync(AdminOrdersFilterState state)
    {
        try
        {
            if (state.Id.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.IdFilter, state.Id.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminOrdersStorageKeys.IdFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.ProductFilter, state.Product);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.BuyerFilter, state.Buyer);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.SellerFilter, state.Seller);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.StatusFilter, state.Status);

            if (state.MinAmount.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.MinAmountFilter, state.MinAmount.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminOrdersStorageKeys.MinAmountFilter);
            }

            if (state.MaxAmount.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.MaxAmountFilter, state.MaxAmount.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminOrdersStorageKeys.MaxAmountFilter);
            }

            if (state.Date.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.DateFilter, state.Date.Value.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminOrdersStorageKeys.DateFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminOrdersStorageKeys.Page, (state.Page ?? 1).ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // No-op: ignore storage failures.
        }
    }
}