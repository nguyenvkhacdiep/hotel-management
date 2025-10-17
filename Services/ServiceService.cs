using AutoMapper;
using Ecommerce.Base.Exceptions;
using HotelManagement.Data;
using HotelManagement.Extensions;
using HotelManagement.Helpers;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Models;

public class ServiceService:IServiceService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public ServiceService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    
    public async Task<string> AddServiceAsync(AddServiceDto addServiceDto)
    {
        var existingService = await _dbcontext.Services.Where(a => a.Name == addServiceDto.Name).FirstOrDefaultAsync();
       
        if (existingService != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "name",
                    Issue = "Service name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newService = new Service
        {
            Id = Guid.NewGuid(),
            Name = addServiceDto.Name,
            Price = addServiceDto.Price,
        };
        
        _dbcontext.Services.Add(newService);
        await _dbcontext.SaveChangesAsync();
        
        return "Service has been added successfully.";
    }
    
    public async Task<PageList<ServiceResponseModel>> GetAllServices(RequestParameters parameters)
    {
        var query = _dbcontext.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.Name != null && x.Name.Contains(parameters.SearchKey));

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.Name);
        else
            query = query.ApplySort(parameters.OrderBy);

        var services = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var response = _mapper.Map<List<ServiceResponseModel>>(services);

        return new PageList<ServiceResponseModel>(response, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<ServiceResponseModel> GetServiceById(Guid id)
    {
        var findService = await _dbcontext.Services.FirstOrDefaultAsync(a => a.Id == id);

        if (findService == null)
        {
            throw new NotFoundException("Service not found");
        }
        
        return _mapper.Map<ServiceResponseModel>(findService);
    }
    
    public async Task<string> EditService(Guid id, AddServiceDto serviceDto)
    {
        var findService = await _dbcontext.Services.FirstOrDefaultAsync(a => a.Id == id);

        if (findService == null)
        {
            throw new NotFoundException("Service not found");
        }
        
        var existingService = await _dbcontext.Services
            .FirstOrDefaultAsync(a => a.Name == serviceDto.Name && a.Id != id);

        if (existingService != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "name",
                    Issue = "Service Name is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        findService.Name = serviceDto.Name;
        findService.Price = serviceDto.Price;
        
        _dbcontext.Services.Update(findService);
        await _dbcontext.SaveChangesAsync();
        
        return "Service has been edited successfully.";
    }
    
    public async Task<string> DeleteService(Guid id)
    {
        var findService = await _dbcontext.Services.FirstOrDefaultAsync(a => a.Id == id);

        if (findService == null)
        {
            throw new NotFoundException("Service not found");
        }

        try
        {
            _dbcontext.Services.Remove(findService);
            await _dbcontext.SaveChangesAsync();
            return "Service has been deleted successfully.";
        }
        catch (Exception ex)
        {
            if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                throw new InvalidOperationException("Cannot delete this floor because it is being used in another table (e.g., Room).");
            }
            
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }
    }
}