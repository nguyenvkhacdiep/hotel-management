using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IBookingService
{
    Task<string> AddBookingAsync(AddBookingDto addBookingDto);
    Task<PageList<BookingResponseModel>> GetAllBookings(BookingRequestParameters parameters);
    Task<BookingResponseModel> GetBookingById(Guid id);
    Task<string> EditBooking(Guid id, UpdateBookingDto updateBookingDto);
    Task<string> DeleteBooking(Guid id);
    Task<string> CancelBookingAsync(Guid id);
    Task<string> ConfirmBookingAsync(Guid id);
    Task<string> CheckInBookingAsync(Guid id);
    Task<string> CheckOutBookingAsync(Guid id);
    Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkIn, DateTime checkOut, Guid? excludeBookingId = null);
    Task<decimal> CalculateTotalAmountAsync(Guid roomId, DateTime checkIn, DateTime checkOut);
}