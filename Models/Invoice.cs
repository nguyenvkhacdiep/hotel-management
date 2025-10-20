using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public Booking Booking { get; set; } = null!;
}
