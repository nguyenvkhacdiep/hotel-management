namespace HotelManagement.Services.Common;

public class RequestParameters
{
    public int PageSize { get; set; } = 10;

    public int PageNumber { get; set; } = 1;

    public string? OrderBy { get; set; }

    public string? SearchKey { get; set; }
}