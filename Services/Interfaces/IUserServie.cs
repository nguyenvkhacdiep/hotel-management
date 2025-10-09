using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IUserService
{
    Task<string> AddUserAsync(AddUserDto addUserDto);
}