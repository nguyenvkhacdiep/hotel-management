using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class RoomStatusHistory
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public RoomStatus? OldStatus { get; set; }
    public RoomStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string? ChangeReason { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    public Room Room { get; set; } = null!;
}