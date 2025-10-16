using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class RoomPrice
{ 
    public Guid Id { get; set; }
    public Guid RoomTypeId { get; set; }
    public SeasonType SeasonName { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DayType DayType { get; set; } = DayType.All;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public RoomType RoomType { get; set; } = null!;
}