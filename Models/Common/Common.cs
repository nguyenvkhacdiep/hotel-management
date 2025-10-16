namespace HotelManagement.Models.Common;

public class MessageResponse
{
    public string Message { get; set; }
}

public class FilterConfig
{
    public string Name { get; set; }
    public string Label { get; set; }
    public FilterType Type { get; set; }
    public Dictionary<string, string>? Options { get; set; }
    public string? Placeholder { get; set; }
}

public enum FilterType
{
    Select,
    Text,
    Date,
    Number
}