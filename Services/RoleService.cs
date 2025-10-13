using AutoMapper;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class RoleService: IRoleService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public RoleService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }

    public async Task<List<RoleModel>> GetAllRoles()
    {
        var allRoles = await _dbcontext.Roles.ToListAsync();
        return _mapper.Map<List<RoleModel>>(allRoles);
    }
}