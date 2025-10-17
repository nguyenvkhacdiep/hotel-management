using AutoMapper;
using HotelManagement.Models;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseModel>();
        CreateMap<Role, RoleModel>();
        CreateMap<Amenity, AmenityResponseModel>();
        CreateMap<Floor, FloorResponseModel>();
        CreateMap<RoomType, RoomTypeResponseModel>();
        CreateMap<Service, ServiceResponseModel>();
        CreateMap<Room, RoomResponseModel>()
            .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType.Name))
            .ForMember(dest => dest.FloorNumber, opt => opt.MapFrom(src => src.Floor.FloorNumber))
            .ForMember(dest => dest.FloorId, opt => opt.MapFrom(src => src.Floor.Id))
            .ForMember(dest => dest.FloorName, opt => opt.MapFrom(src => src.Floor.FloorName))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.RoomType.PricePerNight))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => 
                src.RoomAmenities.Select(ra => ra.Amenity.AmenityName).ToList()));

        CreateMap<Room, RoomDetailResponseModel>()
            .IncludeBase<Room, RoomResponseModel>()
            .ForMember(dest => dest.ActivePrices, opt => opt.MapFrom(src => src.RoomType.RoomPrices))
            .ForMember(dest => dest.RecentStatusChanges, opt => opt.MapFrom(src => src.StatusHistories));

        CreateMap<RoomPrice, RoomPriceSummaryDto>()
            .ForMember(dest => dest.SeasonName, opt => opt.MapFrom(src => src.SeasonName.ToString()))
            .ForMember(dest => dest.DayType, opt => opt.MapFrom(src => src.DayType.ToString()));

        CreateMap<RoomStatusHistory, RoomStatusHistoryDto>()
            .ForMember(dest => dest.OldStatus, opt => opt.MapFrom(src => src.OldStatus.HasValue ? src.OldStatus.Value.ToString() : null))
            .ForMember(dest => dest.NewStatus, opt => opt.MapFrom(src => src.NewStatus.ToString()));
        CreateMap<RoomPrice, RoomPriceResponseModel>();
        CreateMap<Booking, BookingResponseModel>();
        CreateMap<BookingHotelService, BookingServiceResponseModel>();
    }
}