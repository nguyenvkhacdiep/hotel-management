namespace HotelManagement.Models;

public class Service
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}