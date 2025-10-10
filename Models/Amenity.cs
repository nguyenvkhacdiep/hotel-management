using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class Amenity
{
    public Guid Id { get; set; }
    public string AmenityName { get; set; } = null!;
    public string? AmenityIcon { get; set; }
    public AmenityCategory? Category { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}