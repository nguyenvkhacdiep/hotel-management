using AutoMapper;
using Ecommerce.Base.Exceptions;
using HotelManagement.Data;
using HotelManagement.Extensions;
using HotelManagement.Helpers;
using HotelManagement.Models;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class RoomTypeService: IRoomTypeService
{
       private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public RoomTypeService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    
    public async Task<string> AddRoomTypeAsync(AddRoomTypeDto addRoomTypeDto)
    {
        var existingRoomType = await _dbcontext.RoomTypes.Where(a => a.Name == addRoomTypeDto.Name).FirstOrDefaultAsync();
       
        if (existingRoomType != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "floor Name",
                    Issue = "Room Type name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newRoomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = addRoomTypeDto.Name,
            Description = addRoomTypeDto.Description,
            MaxCapacity = addRoomTypeDto.MaxCapacity,
            PricePerNight = addRoomTypeDto.PricePerNight,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        _dbcontext.RoomTypes.Add(newRoomType);
        await _dbcontext.SaveChangesAsync();
        
        return "RoomType has been added successfully.";
    }
    
    public async Task<PageList<RoomTypeResponseModel>> GetAllRoomTypes(RequestParameters parameters)
    {
        var query = _dbcontext.RoomTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.Name != null && x.Name.Contains(parameters.SearchKey));

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CreatedAt);
        else
            query = query.ApplySort(parameters.OrderBy);

        var roomTypes = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var roomTypeResponse = _mapper.Map<List<RoomTypeResponseModel>>(roomTypes);

        return new PageList<RoomTypeResponseModel>(roomTypeResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<RoomTypeResponseModel> GetRoomTypeById(Guid id)
    {
        var findRoomType = await _dbcontext.RoomTypes.FirstOrDefaultAsync(a => a.Id == id);

        if (findRoomType == null)
        {
            throw new NotFoundException("RoomType not found");
        }
        
        return _mapper.Map<RoomTypeResponseModel>(findRoomType);
    }
    
    public async Task<string> EditRoomType(Guid id, AddRoomTypeDto updateDto)
    {
        var findRoomType = await _dbcontext.RoomTypes.FirstOrDefaultAsync(a => a.Id == id);

        if (findRoomType == null)
        {
            throw new NotFoundException("RoomType not found");
        }
        
        var existingRoomType = await _dbcontext.RoomTypes
            .FirstOrDefaultAsync(a => a.Name == updateDto.Name && a.Id != id);

        if (existingRoomType != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "floorName",
                    Issue = "RoomType Name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        findRoomType.Name = updateDto.Name;
        findRoomType.Description = updateDto.Description;
        findRoomType.MaxCapacity = updateDto.MaxCapacity;
        findRoomType.PricePerNight = updateDto.PricePerNight;
        findRoomType.UpdatedAt = DateTime.UtcNow;
        
        _dbcontext.RoomTypes.Update(findRoomType);
        await _dbcontext.SaveChangesAsync();
        
        return "RoomType has been edited successfully.";
    }
    
    public async Task<string> DeleteRoomType(Guid id)
    {
        var findRoomType = await _dbcontext.RoomTypes.FirstOrDefaultAsync(a => a.Id == id);

        if (findRoomType == null)
        {
            throw new NotFoundException("RoomType not found");
        }
        
        _dbcontext.RoomTypes.Remove(findRoomType);
        await _dbcontext.SaveChangesAsync();
        
        return "RoomType has been deleted successfully.";
    }
}