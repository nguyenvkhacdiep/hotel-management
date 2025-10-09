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

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2
}
