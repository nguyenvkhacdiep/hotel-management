using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models;

public class Room
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    
    public RoomType RoomType { get; set; } = null!;
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public enum RoomStatus
{
    Available = 0,
    Occupied = 1,
    Maintenance = 2
}