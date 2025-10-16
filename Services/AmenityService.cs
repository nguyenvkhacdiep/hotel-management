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

public class AmenityService : IAmenityService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public AmenityService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }

    public async Task<string> AddAmenityAsync(AddAmenityDto addAmenityDto)
    {
        var existingAmenity = await _dbcontext.Amenities.Where(a => a.AmenityName == addAmenityDto.AmenityName).FirstOrDefaultAsync();
       
        if (existingAmenity != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "amenityName",
                    Issue = "Amenity name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newAmenity = new Amenity
        {
            Id = Guid.NewGuid(),
            AmenityName = addAmenityDto.AmenityName,
            AmenityIcon = addAmenityDto.AmenityIcon,
            Category = addAmenityDto.Category,
            Description = addAmenityDto.Description,
            IsActive = true,
        };
        
        _dbcontext.Amenities.Add(newAmenity);
        await _dbcontext.SaveChangesAsync();
        
        return "Amenity has been added successfully.";
    }
    
    public async Task<PageList<AmenityResponseModel>> GetAllAmenities(RequestParameters parameters)
    {
        var query = _dbcontext.Amenities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.AmenityName != null && x.AmenityName.Contains(parameters.SearchKey));

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.AmenityName);
        else
            query = query.ApplySort(parameters.OrderBy);

        var users = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var amenityResponse = _mapper.Map<List<AmenityResponseModel>>(users);

        return new PageList<AmenityResponseModel>(amenityResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<AmenityResponseModel> GetAmenityById(Guid id)
    {
        var findAmenity = await _dbcontext.Amenities.FirstOrDefaultAsync(a => a.Id == id);

        if (findAmenity == null)
        {
            throw new NotFoundException("Amenity not found");
        }
        
        return _mapper.Map<AmenityResponseModel>(findAmenity);
    }
    
    public async Task<string> EditAmenity(Guid id, AddAmenityDto amenityDto)
    {
        var findAmenity = await _dbcontext.Amenities.FirstOrDefaultAsync(a => a.Id == id);

        if (findAmenity == null)
        {
            throw new NotFoundException("Amenity not found");
        }
        
        var existingAmenity = await _dbcontext.Amenities
            .FirstOrDefaultAsync(a => a.AmenityName == amenityDto.AmenityName && a.Id != id);

        if (existingAmenity != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "amenityName",
                    Issue = "Amenity Name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        findAmenity.AmenityName = amenityDto.AmenityName;
        findAmenity.AmenityIcon = amenityDto.AmenityIcon;
        findAmenity.Category = amenityDto.Category;
        findAmenity.Description = amenityDto.Description;
        
        _dbcontext.Amenities.Update(findAmenity);
        await _dbcontext.SaveChangesAsync();
        
        return "Amenity has been edited successfully.";
    }
    
    public async Task<string> DeleteAmenity(Guid id)
    {
        var findAmenity = await _dbcontext.Amenities.FirstOrDefaultAsync(a => a.Id == id);

        if (findAmenity == null)
        {
            throw new NotFoundException("Amenity not found");
        }

        try
        {
            _dbcontext.Amenities.Remove(findAmenity);
            await _dbcontext.SaveChangesAsync();
        
            return "Amenity has been deleted successfully.";
        }
        catch (Exception ex)
        {
            if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                throw new InvalidOperationException("Cannot delete this amenity because it is being used in another table (e.g., Room).");
            }
            
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }
    }
    
    public async Task<string> ToggleStatus(Guid id)
    {
        var findAmenity = await _dbcontext.Amenities.FirstOrDefaultAsync(u => u.Id == id);
        
        if(findAmenity == null)
            throw new NotFoundException("User not found.");
        
        findAmenity.IsActive = !findAmenity.IsActive;
        
        _dbcontext.Amenities.Update(findAmenity);
        await _dbcontext.SaveChangesAsync();
        
        return "Amenity has been toggled successfully.";
    }
}