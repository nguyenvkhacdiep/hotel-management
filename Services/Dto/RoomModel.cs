using HotelManagement.Models.Common;

namespace HotelManagement.Services.Dto;

public class AddAmenityDto
{
    public string AmenityName { get; set; } = null!;
    public string? AmenityIcon { get; set; }
    public AmenityCategory Category { get; set; }
    public string? Description { get; set; }
}

public class AmenityResponseModel
{
    public Guid Id { get; set; }
    public string AmenityName { get; set; }
    public string? AmenityIcon { get; set; }
    public AmenityCategory? Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class FloorResponseModel
{
    public Guid Id { get; set; }
    public string? FloorName { get; set; }
    public int TotalRooms { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddFloorDto
{
    public string? FloorName { get; set; }
    public int TotalRooms { get; set; }
    public string? Description { get; set; }
}