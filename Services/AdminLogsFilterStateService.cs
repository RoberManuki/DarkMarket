using Microsoft.JSInterop;
using System.Globalization;

namespace DarkMarket.Services;

public sealed class AdminLogsFilterState
{
    public string GlobalSearch { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? Page { get; init; }
    public AdminLogsQuickRangePreset? QuickRangePreset { get; init; }
    public AdminLogsAuditQuickFilter? AuditQuickFilter { get; init; }
    public AdminLogSortColumn? SortColumn { get; init; }
    public bool? SortAscending { get; init; }
}

public class AdminLogsFilterStateService
{
    private readonly IJSRuntime _js;

    public AdminLogsFilterStateService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AdminLogsFilterState> LoadAsync()
    {
        try
        {
            var globalSearch = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.GlobalSearch) ?? string.Empty;
            var userId = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.UserFilter) ?? string.Empty;
            var source = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.SourceFilter) ?? string.Empty;
            var message = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.MessageFilter) ?? string.Empty;
            var level = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.LevelFilter) ?? string.Empty;

            DateTime? startDate = null;
            var startDateRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.StartDateFilter);
            if (DateTime.TryParse(startDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedStartDate))
            {
                startDate = parsedStartDate.Date;
            }

            DateTime? endDate = null;
            var endDateRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.EndDateFilter);
            if (DateTime.TryParse(endDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedEndDate))
            {
                endDate = parsedEndDate.Date;
            }

            int? page = null;
            var pageRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.Page);
            if (int.TryParse(pageRaw, out var parsedPage) && parsedPage > 0)
            {
                page = parsedPage;
            }

            AdminLogsQuickRangePreset? quickRangePreset = null;
            var quickRangePresetRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.QuickRangePreset);
            if (Enum.TryParse<AdminLogsQuickRangePreset>(quickRangePresetRaw, out var parsedQuickRangePreset))
            {
                quickRangePreset = parsedQuickRangePreset;
            }

            AdminLogsAuditQuickFilter? auditQuickFilter = null;
            var auditQuickFilterRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.AuditQuickFilter);
            if (Enum.TryParse<AdminLogsAuditQuickFilter>(auditQuickFilterRaw, out var parsedAuditQuickFilter))
            {
                auditQuickFilter = parsedAuditQuickFilter;
            }

            AdminLogSortColumn? sortColumn = null;
            var sortColumnRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.SortColumn);
            if (Enum.TryParse<AdminLogSortColumn>(sortColumnRaw, out var parsedSortColumn))
            {
                sortColumn = parsedSortColumn;
            }

            bool? sortAscending = null;
            var sortAscendingRaw = await _js.InvokeAsync<string?>("localStorage.getItem", AdminLogsStorageKeys.SortAscending);
            if (bool.TryParse(sortAscendingRaw, out var parsedSortAscending))
            {
                sortAscending = parsedSortAscending;
            }

            return new AdminLogsFilterState
            {
                GlobalSearch = globalSearch,
                UserId = userId,
                Source = source,
                Message = message,
                Level = level,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                QuickRangePreset = quickRangePreset,
                AuditQuickFilter = auditQuickFilter,
                SortColumn = sortColumn,
                SortAscending = sortAscending
            };
        }
        catch
        {
            return new AdminLogsFilterState();
        }
    }

    public async Task SaveAsync(AdminLogsFilterState state)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.GlobalSearch, state.GlobalSearch);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.UserFilter, state.UserId);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.SourceFilter, state.Source);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.MessageFilter, state.Message);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.LevelFilter, state.Level);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.QuickRangePreset, state.QuickRangePreset?.ToString() ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.AuditQuickFilter, state.AuditQuickFilter?.ToString() ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.SortColumn, state.SortColumn?.ToString() ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.SortAscending, state.SortAscending?.ToString() ?? string.Empty);

            if (state.StartDate.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.StartDateFilter, state.StartDate.Value.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminLogsStorageKeys.StartDateFilter);
            }

            if (state.EndDate.HasValue)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.EndDateFilter, state.EndDate.Value.ToString("o", CultureInfo.InvariantCulture));
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AdminLogsStorageKeys.EndDateFilter);
            }

            await _js.InvokeVoidAsync("localStorage.setItem", AdminLogsStorageKeys.Page, (state.Page ?? 1).ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // No-op: ignore storage failures.
        }
    }
}