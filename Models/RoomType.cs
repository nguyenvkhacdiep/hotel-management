namespace HotelManagement.Models;

public class RoomType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int MaxCapacity { get; set; } = 2;
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<RoomPrice> RoomPrices { get; set; } = new List<RoomPrice>();
}
