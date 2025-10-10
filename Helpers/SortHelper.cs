using System.Linq.Dynamic.Core;

namespace HotelManagement.Helpers;

public static class SortHelper
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return source;

        var orderParams = orderBy.Trim().Split(',');

        var sortExpression = string.Join(",", orderParams.Select(param =>
        {
            var trimmed = param.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return "";

            return trimmed.StartsWith("-")
                ? $"{trimmed[1..]} descending"
                : $"{trimmed} ascending";
        }));

        return source.OrderBy(sortExpression);
    }
}