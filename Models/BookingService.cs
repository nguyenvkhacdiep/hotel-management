namespace HotelManagement.Models;

public class BookingService
{
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }

    public Booking Booking { get; set; } = null!;
    public Service Service { get; set; } = null!;
}