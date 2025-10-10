using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? UrlAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    
}

