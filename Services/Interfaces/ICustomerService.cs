using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponseModel> GetCustomerByPhone(string phone);
}