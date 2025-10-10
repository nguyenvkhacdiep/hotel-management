using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IFloorService
{
    Task<string> AddFloorAsync(AddFloorDto addFloorDto);
    Task<PageList<FloorResponseModel>> GetAllFloors(RequestParameters parameters);
    Task<FloorResponseModel> GetFloorById(Guid id);
    Task<string> EditFloor(Guid id, AddFloorDto amenityDto);
    Task<string> DeleteFloor(Guid id);
    Task<string> ToggleActiveFloorAsync(Guid id);
    
}