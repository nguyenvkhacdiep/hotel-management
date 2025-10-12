using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IRoomTypeService
{
    Task<string> AddRoomTypeAsync(AddRoomTypeDto addRoomTyperDto);
    Task<PageList<RoomTypeResponseModel>> GetAllRoomTypes(RequestParameters parameters);
    Task<RoomTypeResponseModel> GetRoomTypeById(Guid id);
    Task<string> EditRoomType(Guid id, AddRoomTypeDto updateDto);
    Task<string> DeleteRoomType(Guid id);
}