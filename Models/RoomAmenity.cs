namespace HotelManagement.Models;

public class RoomAmenity
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid AmenityId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Room Room { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}