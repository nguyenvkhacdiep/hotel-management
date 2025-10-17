using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IServiceService
{
    Task<string> AddServiceAsync(AddServiceDto addServicerDto);
    Task<PageList<ServiceResponseModel>> GetAllServices(RequestParameters parameters);
    Task<ServiceResponseModel> GetServiceById(Guid id);
    Task<string> EditService(Guid id, AddServiceDto updateDto);
    Task<string> DeleteService(Guid id);
}