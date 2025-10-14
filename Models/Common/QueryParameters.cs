namespace HotelManagement.Models.Common;

public class QueryParameters
{
    public string? SearchKey { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? OrderBy { get; set; }
    
    public Dictionary<string, string> Filters { get; set; } = new();

    public string ToQueryString()
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(SearchKey))
            parameters.Add($"SearchKey={Uri.EscapeDataString(SearchKey)}");

        if (!string.IsNullOrEmpty(OrderBy))
            parameters.Add($"OrderBy={Uri.EscapeDataString(OrderBy)}");

        parameters.Add($"PageNumber={PageNumber}");
        parameters.Add($"PageSize={PageSize}");
        
        foreach (var filter in Filters)
        {
            if (!string.IsNullOrEmpty(filter.Value))
                parameters.Add($"{filter.Key}={Uri.EscapeDataString(filter.Value)}");
        }

        return string.Join("&", parameters);
    }

    public void AddFilter(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Filters[key] = value;
    }

    public string? GetFilter(string key)
    {
        return Filters.TryGetValue(key, out var value) ? value : null;
    }

    public static QueryParameters FromRequest(HttpRequest request)
    {
        var query = request.Query;
        var queryParams = new QueryParameters
        {
            SearchKey = query["SearchKey"].ToString(),
            OrderBy = query["OrderBy"].ToString(),
            PageNumber = int.TryParse(query["PageNumber"], out int page) ? page : 1,
            PageSize = int.TryParse(query["PageSize"], out int pageSize) ? pageSize : 10
        };
        
        var excludedKeys = new[] { "SearchKey", "OrderBy", "PageNumber", "PageSize" };
        foreach (var key in query.Keys.Where(k => !excludedKeys.Contains(k)))
        {
            queryParams.Filters[key] = query[key].ToString();
        }

        return queryParams;
    }
}