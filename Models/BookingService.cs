namespace HotelManagement.Models;

public class BookingService
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public Service Service { get; set; } = null!;
}