using HotelManagement.Models;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleModel>> GetAllRoles();
}