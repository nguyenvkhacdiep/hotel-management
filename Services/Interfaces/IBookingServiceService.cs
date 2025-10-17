using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IBookingServiceService
{
    Task<string> AddBookingServiceAsync(AddBookingServiceDto addBookingServiceDto);
    Task<PageList<BookingServiceResponseModel>> GetAllBookingServices(BookingServiceRequestParameters parameters);
    Task<BookingServiceResponseModel> GetBookingServiceById(Guid id);
    Task<string> EditBookingService(Guid id, UpdateBookingServiceDto amenityDto);
    Task<string> DeleteBookingService(Guid id);
}