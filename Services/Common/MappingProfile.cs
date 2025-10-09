using AutoMapper;
using HotelManagement.Models;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseModel>();
    }
}