using HotelManagement.Models;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IRoomPricesService
{
    Task<string> AddRoomPrices(AddRoomPriceDto addDto);
    Task<PageList<RoomPriceResponseModel>> GetAllRoomPrices(RoomPriceRequestParameters parameters);
    Task<RoomPriceResponseModel> GetRoomPriceById(Guid id);
    Task<string> EditRoomPrice(Guid id, UpdateRoomPriceDto updateDto);
    Task<string> DeleteRoomPrice(Guid id);
    Task<string> ToggleRoomPriceStatus(Guid id);
    Task<CurrentRoomPriceResponseModel> GetCurrentRoomPrice(Guid roomId, DateTime? date = null );
    Task<List<PriceCalendarDto>> GetRoomPriceCalendar(Guid roomId, PriceCalendarRequestParameters parameters);
}