using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IAuthService
{
    Task<UserLoginResponseModel> Login(UserLoginDto userLoginDto);
}