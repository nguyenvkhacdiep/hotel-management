namespace HotelManagement.Models;

public class Floor
{
    public Guid Id { get; set; }
    public string? FloorName { get; set; }
    public int TotalRooms { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}