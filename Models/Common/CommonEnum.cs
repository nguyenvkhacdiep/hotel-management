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
public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    CheckedIn = 2,
    CheckedOut = 3,
    Canceled = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2
}