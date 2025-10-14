using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IAmenityService
{
    Task<string> AddAmenityAsync(AddAmenityDto addAmenityDto);
    Task<PageList<AmenityResponseModel>> GetAllAmenities(RequestParameters parameters);
    Task<AmenityResponseModel> GetAmenityById(Guid id);
    Task<string> EditAmenity(Guid id, AddAmenityDto amenityDto);
    Task<string> DeleteAmenity(Guid id);
    Task<string> ToggleStatus(Guid id);
    
}