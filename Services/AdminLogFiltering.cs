using DarkMarket.Models;

namespace DarkMarket.Services;

public sealed class AdminLogFilterCriteria
{
    public string? GlobalTerm { get; init; }
    public string? UserId { get; init; }
    public string? Source { get; init; }
    public string? Message { get; init; }
    public string? Level { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public static class AdminLogFiltering
{
    public static IQueryable<AppLog> Apply(IQueryable<AppLog> source, AdminLogFilterCriteria criteria)
    {
        var query = source;

        if (!string.IsNullOrWhiteSpace(criteria.GlobalTerm))
        {
            query = query.Where(log =>
                (log.UserId != null && log.UserId.Contains(criteria.GlobalTerm))
                || log.Source.Contains(criteria.GlobalTerm)
                || log.Message.Contains(criteria.GlobalTerm));
        }

        if (!string.IsNullOrWhiteSpace(criteria.UserId))
        {
            query = query.Where(log => log.UserId != null && log.UserId.Contains(criteria.UserId));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Source))
        {
            query = query.Where(log => log.Source.Contains(criteria.Source));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Message))
        {
            query = query.Where(log => log.Message.Contains(criteria.Message));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Level))
        {
            query = query.Where(log => log.Level == criteria.Level);
        }

        if (criteria.StartDate.HasValue)
        {
            query = query.Where(log => log.Timestamp.Date >= criteria.StartDate.Value.Date);
        }

        if (criteria.EndDate.HasValue)
        {
            query = query.Where(log => log.Timestamp.Date <= criteria.EndDate.Value.Date);
        }

        return query;
    }
}
