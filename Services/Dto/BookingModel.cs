using HotelManagement.Models.Common;
using HotelManagement.Services.Common;

namespace HotelManagement.Services.Dto;

public class BookingResponseModel
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public int NumberOfNights { get; set; }
    
    public RoomSummaryDto? Room { get; set; }
    public CustomerSummaryDto? Customer { get; set; }
    public List<BookingServiceDto> BookingServices { get; set; } = new();
    public List<InvoiceSummaryDto> Invoices { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

public class RoomSummaryDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string FloorName { get; set; } = string.Empty;
}

public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime IssueDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class BookingServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddBookingDto
{
    public Guid RoomId { get; set; }
    public Guid? CustomerId { get; set; }
    public CustomerInfoDto? CustomerInfo { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}

public class CustomerInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IDCard { get; set; }
}

public class UpdateBookingDto
{
    public Guid RoomId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus Status { get; set; }
}

public class BookingRequestParameters : RequestParameters
{
    public BookingStatus? Status { get; set; }
    public Guid? RoomId { get; set; }
    public DateTime? CheckInDateFrom { get; set; }
    public DateTime? CheckInDateTo { get; set; }
    public DateTime? CheckOutDateFrom { get; set; }
    public DateTime? CheckOutDateTo { get; set; }
}

public class BookingServiceRequestParameters : RequestParameters
{
    public Guid? BookingId { get; set; }
    public Guid? ServiceId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

public class AddBookingServiceDto
{
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateBookingServiceDto
{
    public int Quantity { get; set; }
}

public class BookingServiceResponseModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string BookingReference { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal ServicePrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}