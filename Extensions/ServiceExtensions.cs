using AutoMapper.EquivalencyExpression;
using HotelManagement.Models;
using HotelManagement.Services;
using HotelManagement.Services.Common;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;



namespace HotelManagement.Extensions;

public static class ServiceExtensions
{
    public static void ServiceConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(cfg => { cfg.AddCollectionMappers(); },
            typeof(MappingProfile));
        
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAmenityService, AmenityService>();
        services.AddScoped<IFloorService, FloorService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomPricesService, RoomPricesService>();
    }
    
}