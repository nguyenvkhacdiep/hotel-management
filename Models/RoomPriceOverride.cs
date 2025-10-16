using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class RoomPriceOverride
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public decimal PriceAdjustment { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Room Room { get; set; } = null!;
}