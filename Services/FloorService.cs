using AutoMapper;
using Ecommerce.Base.Exceptions;
using HotelManagement.Data;
using HotelManagement.Extensions;
using HotelManagement.Helpers;
using HotelManagement.Models;
using HotelManagement.Models.Common;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class FloorService: IFloorService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public FloorService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    
    public async Task<string> AddFloorAsync(AddFloorDto addFloorDto)
    {
        var existingFloor = await _dbcontext.Floors.Where(a => a.FloorName == addFloorDto.FloorName).FirstOrDefaultAsync();
       
        if (existingFloor != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "floor Name",
                    Issue = "Floor name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newFloor = new Floor
        {
            Id = Guid.NewGuid(),
            FloorName = addFloorDto.FloorName,
            FloorNumber = addFloorDto.FloorNumber,
            Description = addFloorDto.Description,
            TotalRooms = addFloorDto.TotalRooms,
        };
        
        _dbcontext.Floors.Add(newFloor);
        await _dbcontext.SaveChangesAsync();
        
        return "Floor has been added successfully.";
    }
    
    public async Task<PageList<FloorResponseModel>> GetAllFloors(RequestParameters parameters)
    {
        var query = _dbcontext.Floors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.FloorName != null && x.FloorName.Contains(parameters.SearchKey));

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.FloorName);
        else
            query = query.ApplySort(parameters.OrderBy);

        var floors = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var floorResponse = _mapper.Map<List<FloorResponseModel>>(floors);

        return new PageList<FloorResponseModel>(floorResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<FloorResponseModel> GetFloorById(Guid id)
    {
        var findFloor = await _dbcontext.Floors.FirstOrDefaultAsync(a => a.Id == id);

        if (findFloor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        return _mapper.Map<FloorResponseModel>(findFloor);
    }
    
    public async Task<string> EditFloor(Guid id, AddFloorDto amenityDto)
    {
        var findFloor = await _dbcontext.Floors.FirstOrDefaultAsync(a => a.Id == id);

        if (findFloor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        var existingFloor = await _dbcontext.Floors
            .FirstOrDefaultAsync(a => a.FloorName == amenityDto.FloorName && a.Id != id);

        if (existingFloor != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "floorName",
                    Issue = "Floor Name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        findFloor.FloorName = amenityDto.FloorName;
        findFloor.Description = amenityDto.Description;
        findFloor.TotalRooms = amenityDto.TotalRooms;
        findFloor.UpdatedAt = DateTime.Now;
        
        _dbcontext.Floors.Update(findFloor);
        await _dbcontext.SaveChangesAsync();
        
        return "Floor has been edited successfully.";
    }
    
    public async Task<string> DeleteFloor(Guid id)
    {
        var findFloor = await _dbcontext.Floors.FirstOrDefaultAsync(a => a.Id == id);

        if (findFloor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        _dbcontext.Floors.Remove(findFloor);
        await _dbcontext.SaveChangesAsync();
        
        return "Floor has been deleted successfully.";
    }
    
    public async Task<string> ToggleActiveFloorAsync(Guid id)
    {
        var findFloor = await _dbcontext.Floors.FirstOrDefaultAsync(a => a.Id == id);

        if (findFloor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        findFloor.IsActive= !findFloor.IsActive;
        findFloor.UpdatedAt = DateTime.Now;
        
        _dbcontext.Floors.Update(findFloor);
        await _dbcontext.SaveChangesAsync();
        
        return "Floor has been toggled successfully.";
    }
    
}