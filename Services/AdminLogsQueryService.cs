using DarkMarket.Data;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services;

public sealed record AdminLogsAuditCounts(
    int All,
    int ReleaseOnly,
    int ReleaseSuccess,
    int ReleaseRefused);

public sealed record AdminLogsPageData(
    int TotalLogs,
    int EffectivePage,
    List<AppLog> Logs,
    AdminLogsAuditCounts AuditCounts);

public class AdminLogsQueryService
{
    private readonly AppDbContext _db;

    public AdminLogsQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AdminLogsPageData> GetPageDataAsync(
        AdminLogFilterCriteria primaryCriteria,
        AdminLogFilterCriteria auditCountsCriteria,
        AdminLogSortColumn sortColumn,
        bool sortAscending,
        int requestedPage,
        int pageSize)
    {
        var filteredQuery = AdminLogFiltering.Apply(_db.Logs.AsQueryable(), primaryCriteria);
        var totalLogs = await filteredQuery.CountAsync();

        var totalPages = Math.Max(1, (int)Math.Ceiling(totalLogs / (double)pageSize));
        var effectivePage = Math.Min(Math.Max(requestedPage, 1), totalPages);

        var logs = await AdminLogSorting
            .Apply(filteredQuery, sortColumn, sortAscending)
            .Include(l => l.User)
            .Skip((effectivePage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var auditBase = AdminLogFiltering.Apply(_db.Logs.AsQueryable(), auditCountsCriteria);
        var counts = new AdminLogsAuditCounts(
            All: await auditBase.CountAsync(),
            ReleaseOnly: await auditBase.Where(log => log.Source == "AdminOrdersReview").CountAsync(),
            ReleaseSuccess: await auditBase.Where(log => log.Source == "AdminOrdersReview" && log.Level == "Info").CountAsync(),
            ReleaseRefused: await auditBase.Where(log => log.Source == "AdminOrdersReview" && log.Level == "Warning").CountAsync());

        return new AdminLogsPageData(totalLogs, effectivePage, logs, counts);
    }
}
