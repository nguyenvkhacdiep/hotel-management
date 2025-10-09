namespace HotelManagement.Services.Common;

public class PageList<T>
{
    public PageList()
    {
    }

    public PageList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalItems = count;
        Data = new List<T>();
        Data.AddRange(items);
    }

    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; }
    public int TotalItems { get; set; }
}