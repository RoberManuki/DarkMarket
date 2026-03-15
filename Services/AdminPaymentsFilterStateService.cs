using Microsoft.JSInterop;
using System.Globalization;

namespace DarkMarket.Services;

public sealed class AdminPaymentsFilterState
{
    public string UserId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? Date { get; init; }
    public int? Page { get; init; }
}

public class AdminPaymentsFilterStateService
{
    private readonly IJSRuntime _js;

    public AdminPaymentsFilterStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AdminPaymentsFilterState> LoadAsync()
    {
        try
        {
            var userId = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.UserFilter) ?? string.Empty;
            var productId = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.ProductFilter) ?? string.Empty;
            var status = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.StatusFilter) ?? string.Empty;

            decimal? minAmount = null;
            var minAmountRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.MinAmountFilter);
            if (decimal.TryParse(minAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMinAmount))
            {
                minAmount = parsedMinAmount;
            }

            decimal? maxAmount = null;
            var maxAmountRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.MaxAmountFilter);
            if (decimal.TryParse(maxAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedMaxAmount))
            {
                maxAmount = parsedMaxAmount;
            }

            DateTime? date = null;
            var dateRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.DateFilter);
            if (DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDate))
            {
                date = parsedDate.Date;
            }

            int? page = null;
            var pageRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminPaymentsStorageKeys.Page);
            if (int.TryParse(pageRaw, out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
            }

            return new AdminPaymentsFilterState
            {
                UserId = userId,
                ProductId = productId,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Status = status,
                Date = date,
                Page = page
            };
        }
        catch
        {
            return new AdminPaymentsFilterState();
        }
    }

    public async Task SaveAsync(AdminPaymentsFilterState state)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.UserFilter, state.UserId);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.ProductFilter, state.ProductId);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.StatusFilter, state.Status);

            if (state.MinAmount.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.MinAmountFilter, state.MinAmount.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminPaymentsStorageKeys.MinAmountFilter);
            }

            if (state.MaxAmount.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.MaxAmountFilter, state.MaxAmount.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminPaymentsStorageKeys.MaxAmountFilter);
            }

            if (state.Date.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.DateFilter, state.Date.Value.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminPaymentsStorageKeys.DateFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminPaymentsStorageKeys.Page, (state.Page ?? 1).ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // No-op: ignore storage failures.
        }
    }
}