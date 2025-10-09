namespace HotelManagement.Models;

public class RoomType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
