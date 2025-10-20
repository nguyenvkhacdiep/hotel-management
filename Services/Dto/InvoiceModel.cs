using HotelManagement.Models.Common;

namespace HotelManagement.Services.Dto;

public class AddInvoiceDto
{
    public Guid BookingId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

public class InvoiceResponseModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}

public class UpdatePaymentStatusDto
{
    public PaymentStatus PaymentStatus { get; set; }
}

public class UpdateInvoiceDto
{
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}