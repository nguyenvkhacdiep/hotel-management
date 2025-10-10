using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IUserService
{
    Task<string> AddUserAsync(AddUserDto addUserDto);
    Task<PageList<UserResponseModel>> GetAllUsers(RequestParameters parameters);
    Task<UserResponseModel> GetUserById(Guid id);
    Task<string> InactiveUserAsync(Guid id);
    Task<string> EditUserAsync(Guid id, EditUserDto editUserDto);
    Task<string> DeleteUserAsync(Guid id);
}