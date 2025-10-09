using HotelManagement.Models.Common;
using HotelManagement.Services.Common;

namespace HotelManagement.Services.Dto;

public class UserResponseModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; }
}

public class AddUserDto
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
}

public class UserParameters : RequestParameters
{
}