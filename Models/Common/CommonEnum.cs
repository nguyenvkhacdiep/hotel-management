namespace HotelManagement.Models.Common;

public enum RoomStatus
{
    Available,
    Occupied,
    Cleaning,
    Maintenance,
    Reserved,
    OutOfOrder
}
public enum RoomTypeCategory
{
    Standard,
    Deluxe,
    Suite,
    Presidential
}

public enum SeasonType
{
    HighSeason,
    LowSeason,
    PeakSeason,
    Holiday
}

public enum DayType
{
    Weekday,
    Weekend,
    Holiday,
    All
}

public enum AmenityCategory
{
    Technology,
    Comfort,
    View,
    Service,
    Entertainment,
    Others
}

public enum AdjustmentType
{
    Fixed = 0,
    Percentage = 1
}

