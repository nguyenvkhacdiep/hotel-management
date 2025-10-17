using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class Booking
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Guid CustomerId { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    public Room Room { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}