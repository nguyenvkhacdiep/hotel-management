using HotelManagement.Models.Common;
using HotelManagement.Services.Common;

namespace HotelManagement.Services.Dto;

public class UserResponseModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public bool IsActive { get; set; }
    public string UrlAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public RoleModel Role { get; set; }
}

public class RoleModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddUserDto
{
    public string Username { get; set; } = null!;
    public string Password{ get; set; } = null!;
    public Guid RoleId { get; set; }
}

public class EditUserDto
{
    public string Username { get; set; } = null!;
    public string? UrlAvatar { get; set; } = null!;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; }
}