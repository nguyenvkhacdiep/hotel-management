using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IRoomService
{
    Task<string> AddRoomAsync(AddRoomDto addRoomDto);
    Task<PageList<RoomResponseModel>> GetAllRooms(RoomRequestParameters parameters);
    Task<RoomDetailResponseModel> GetRoomById(Guid id);
    Task<string> UpdateRoom(Guid id, UpdateRoomDto updateRoomDto);
    Task<string> DeleteRoom(Guid id);
    Task<string> ChangeRoomStatus(Guid id, ChangeRoomStatusDto statusDto);
    Task<string> UpdateRoomAmenities(Guid id, UpdateRoomAmenitiesDto amenitiesDto);
    Task<List<RoomResponseModel>> GetAvailableRooms(AvailabelRoomParameters parameters);
    Task<RoomStatisticsResponse> GetRoomStatistics();
    Task<List<RoomStatusHistoryDto>> GetRoomStatusHistory(Guid id);
}