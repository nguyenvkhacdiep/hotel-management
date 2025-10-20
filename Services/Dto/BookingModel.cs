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
    
    public RoomResponseModel? Room { get; set; }
    public CustomerResponseModel? Customer { get; set; }
    public List<BookingServiceResponseModel> BookingServices { get; set; } = new();
    public List<InvoiceResponseModel> Invoices { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

public class AddBookingDto
{
    public List<Guid> RoomIds { get; set; }
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

public class AddBookingWebDto
{
    public List<Guid> RoomIds { get; set; }
    public string? RoomIdsString { get; set; }
    public Guid? CustomerId { get; set; }
    public CustomerInfoDto? CustomerInfo { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}