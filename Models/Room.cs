using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelManagement.Models.Common;

namespace HotelManagement.Models;

public class Room
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string? RoomName { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid FloorId { get; set; }
    public string? Position { get; set; }
    public int Capacity { get; set; } = 2;
    public int NumberOfBeds { get; set; } = 1;
    public string? BedType { get; set; }
    public bool SmokingAllowed { get; set; } = false;
    public bool PetFriendly { get; set; } = false;
    public bool Accessible { get; set; } = false;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public DateTime? LastCleanedAt { get; set; }
    public DateTime? LastMaintenanceAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public RoomType RoomType { get; set; } = null!;
    public Floor Floor { get; set; } = null!;
    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    public ICollection<RoomPrice> RoomPrices { get; set; } = new List<RoomPrice>();
    public ICollection<RoomStatusHistory> StatusHistories { get; set; } = new List<RoomStatusHistory>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}