using HotelManagement.Models;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IRoomPricesService
{
    Task<string> AddRoomPrices(AddRoomPriceDto addDto);
    Task<PageList<RoomPriceResponseModel>> GetAllRoomPrices(RoomPriceRequestParameters parameters);
    Task<List<RoomPriceResponseModel>> GetRoomPriceByRoomTypeId(Guid roomTyped);
    Task<RoomPriceResponseModel> GetRoomPriceById(Guid id);
    Task<string> EditRoomPrice(Guid id, UpdateRoomPriceDto updateDto);
    Task<string> DeleteRoomPrice(Guid id);
    Task<string> ToggleRoomPriceStatus(Guid id);
    Task<CurrentRoomPriceResponseModel> GetCurrentRoomPrice(Guid roomId, DateTime? date = null );
    Task<List<PriceCalendarDto>> GetRoomPriceCalendar(Guid roomId, PriceCalendarRequestParameters parameters);
    Task<RoomPriceCalculationResponse> CalculateRoomPrice(GetRoomPriceRequest request);
    Task<string> AddRoomPriceOverride(AddRoomPriceOverrideDto dto);
    Task<string> UpdateRoomPriceOverride(Guid id, UpdateRoomPriceOverrideDto dto);
    Task<string> DeleteRoomPriceOverride(Guid id);
    Task<RoomPriceOverrideResponseModel?> GetRoomPriceOverride(Guid roomId);
}