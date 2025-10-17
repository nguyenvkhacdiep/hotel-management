using HotelManagement.Models.Common;
using HotelManagement.Services.Common;

namespace HotelManagement.Services.Dto;

public class AddAmenityDto
{
    public string AmenityName { get; set; } = null!;
    public string? AmenityIcon { get; set; }
    public AmenityCategory Category { get; set; }
    public string? Description { get; set; }
}

public class AmenityResponseModel
{
    public Guid Id { get; set; }
    public string AmenityName { get; set; }
    public string? AmenityIcon { get; set; }
    public AmenityCategory? Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class FloorResponseModel
{
    public Guid Id { get; set; }
    public string? FloorName { get; set; }
    public string FloorNumber { get; set; }
    public int TotalRooms { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddFloorDto
{
    public string? FloorName { get; set; }
    public string FloorNumber { get; set; }
    public int TotalRooms { get; set; }
    public string? Description { get; set; }
}

public class RoomTypeResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int MaxCapacity { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddRoomTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int MaxCapacity { get; set; }
    public decimal PricePerNight { get; set; }
}

public class AddRoomDto
{
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
    public string? Notes { get; set; }
    public List<Guid>? AmenityIds { get; set; }
}

public class RoomResponseModel
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string? RoomName { get; set; }
    public string RoomTypeName { get; set; } = null!;
    public string FloorNumber { get; set; }
    public string FloorId { get; set; }
    public string? FloorName { get; set; }
    public int Capacity { get; set; }
    public RoomStatus Status { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<string> Amenities { get; set; } = new();
}

public class RoomRequestParameters : RequestParameters
{
    public Guid? RoomTypeId { get; set; }
    public Guid? FloorId { get; set; }
    public RoomStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? SmokingAllowed { get; set; }
    public bool? PetFriendly { get; set; }
    public bool? Accessible { get; set; }
    public int? MinCapacity { get; set; }
}

public class RoomDetailResponseModel : RoomResponseModel
{
    public string? Position { get; set; }
    public int NumberOfBeds { get; set; }
    public string? BedType { get; set; }
    public bool SmokingAllowed { get; set; }
    public bool PetFriendly { get; set; }
    public bool Accessible { get; set; }
    public DateTime? LastCleanedAt { get; set; }
    public DateTime? LastMaintenanceAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RoomPriceSummaryDto> ActivePrices { get; set; } = new();
    public List<RoomStatusHistoryDto> RecentStatusChanges { get; set; } = new();
}

public class RoomPriceSummaryDto
{
    public Guid Id { get; set; }
    public string SeasonName { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DayType { get; set; } = null!;
}

public class RoomStatusHistoryDto
{
    public Guid Id { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = null!;
    public string ChangedBy { get; set; } = null!;
    public string? ChangeReason { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class UpdateRoomDto
{
    public string RoomNumber { get; set; } = null!;
    public string? RoomName { get; set; }
    public Guid RoomTypeId { get; set; }
    public string? Position { get; set; }
    public int Capacity { get; set; }
    public int NumberOfBeds { get; set; }
    public string? BedType { get; set; }
    public bool SmokingAllowed { get; set; }
    public bool PetFriendly { get; set; }
    public bool Accessible { get; set; }
    public string? Notes { get; set; }
}

public class ChangeRoomStatusDto
{
    public RoomStatus NewStatus { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}

public class UpdateRoomAmenitiesDto
{
    public List<Guid> AmenityIds { get; set; } = new();
}

public class AvailabelRoomParameters
{
    public DateTime? Checkin { get; set; }
    public DateTime? Checkout { get; set; }
    public int? MinCapacity { get; set; }
    public Guid? RoomTypeId { get; set; }
    public Guid? FloorId { get; set; }
}

public class RoomStatisticsResponse
{
    public int TotalRooms { get; set; }
    public List<StatusCountDto> StatusCounts { get; set; } = new();
    public List<FloorStatDto> FloorStatistics { get; set; } = new();
    public double OccupancyRate { get; set; }
}

public class StatusCountDto
{
    public RoomStatus Status { get; set; }
    public int Count { get; set; }
}

public class FloorStatDto
{
    public Guid FloorId { get; set; }
    public string FloorNumber { get; set; }
    public string? FloorName { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
}

public class AddRoomPriceDto
{
    public Guid RoomTypeId { get; set; }
    public SeasonType SeasonName { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DayType DayType { get; set; } = DayType.All;
    public bool IsActive { get; set; }
    public int Priority { get; set; } = 0;
}

public class RoomPriceResponseModel
{
    public Guid Id { get; set; }
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = null!;
    public SeasonType SeasonName { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DayType DayType { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RoomPriceRequestParameters : RequestParameters
{
    public Guid? RoomTypeId { get; set; }
    public SeasonType? SeasonName { get; set; }
    public DayType? DayType { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? Date { get; set; }
}

public class UpdateRoomPriceDto
{
    public SeasonType SeasonName { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DayType DayType { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}

public class CurrentRoomPriceResponseModel
{
    public decimal Price { get; set; }
    public string? SeasonName   { get; set; }
    public string? DayType {get; set; }
    public string Source {get; set; }
    public bool HasOverride {get; set; }
}

public class PriceCalendarDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = null!;
    public decimal Price { get; set; }
    public string? SeasonName { get; set; }
    public bool IsWeekend { get; set; }
    public bool IsBasePrice { get; set; }
}

public class PriceCalendarRequestParameters
{
    public DateTime StartDate {get; set; }
    public DateTime EndDate {get; set; }
}

public class AddRoomPriceOverrideDto
{
    public Guid RoomId { get; set; }
    public decimal PriceAdjustment { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateRoomPriceOverrideDto
{
    public decimal PriceAdjustment { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class RoomPriceOverrideResponseModel
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetRoomPriceRequest
{
    public Guid RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public DayType? DayType { get; set; }
}

public class RoomPriceCalculationResponse
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal SeasonalPrice { get; set; }
    public decimal? OverrideAdjustment { get; set; }
    public decimal FinalPrice { get; set; }
    public string? SeasonName { get; set; }
    public string? OverrideReason { get; set; }
    public List<DailyPriceBreakdown> DailyBreakdown { get; set; } = new();
}

public class DailyPriceBreakdown
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string? SeasonName { get; set; }
    public DayType DayType { get; set; }
}