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

public class RoomPricesService: IRoomPricesService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public RoomPricesService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }

    public async Task<string> AddRoomPrices(AddRoomPriceDto addDto)
    {
        var room = await _dbcontext.Rooms.FindAsync(addDto.RoomId);
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        if (addDto.EndDate <= addDto.StartDate)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "EndDate",
                    Issue = "End date must be after start date."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var hasOverlap = await _dbcontext.RoomPrices
            .Where(rp => rp.RoomId == addDto.RoomId)
            .Where(rp => rp.IsActive)
            .Where(rp => rp.DayType == addDto.DayType || rp.DayType == DayType.All || addDto.DayType == DayType.All)
            .AnyAsync(rp => 
                (addDto.StartDate >= rp.StartDate && addDto.StartDate < rp.EndDate) ||
                (addDto.EndDate > rp.StartDate && addDto.EndDate <= rp.EndDate) ||
                (addDto.StartDate <= rp.StartDate && addDto.EndDate >= rp.EndDate));

        if (hasOverlap)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "DateRange",
                    Issue = "Price period overlaps with existing active price for this room."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var newRoomPrice = new RoomPrice
        {
            Id = Guid.NewGuid(),
            RoomId = addDto.RoomId,
            SeasonName = addDto.SeasonName,
            PricePerNight = addDto.PricePerNight,
            StartDate = addDto.StartDate,
            EndDate = addDto.EndDate,
            DayType = addDto.DayType,
            IsActive = addDto.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbcontext.RoomPrices.Add(newRoomPrice);
        await _dbcontext.SaveChangesAsync();

        return "Room price has been added successfully.";
    }

    public async Task<PageList<RoomPriceResponseModel>> GetAllRoomPrices(
        RoomPriceRequestParameters parameters)
    {
        var query = _dbcontext.RoomPrices
            .Include(rp => rp.Room)
            .ThenInclude(r => r.RoomType)
            .AsQueryable();
        
        if (parameters.RoomId.HasValue)
        {
            query = query.Where(rp => rp.RoomId == parameters.RoomId.Value);
        }
        
        if (parameters.SeasonName.HasValue)
        {
            query = query.Where(rp => rp.SeasonName == parameters.SeasonName.Value);
        }
        
        if (parameters.DayType.HasValue)
        {
            query = query.Where(rp => rp.DayType == parameters.DayType.Value);
        }
        
        if (parameters.IsActive.HasValue)
        {
            query = query.Where(rp => rp.IsActive == parameters.IsActive.Value);
        }
        
        if (parameters.Date.HasValue)
        {
            query = query.Where(rp => 
                rp.StartDate <= parameters.Date.Value && 
                rp.EndDate >= parameters.Date.Value);
        }
        
        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CreatedAt);
        else
            query = query.ApplySort(parameters.OrderBy);
        
        var roomPrices = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var response = _mapper.Map<List<RoomPriceResponseModel>>(roomPrices);

        return new PageList<RoomPriceResponseModel>(response, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<RoomPriceResponseModel> GetRoomPriceById(Guid id)
    {
        var roomPrice = await _dbcontext.RoomPrices
            .Include(rp => rp.Room)
            .ThenInclude(r => r.RoomType)
            .FirstOrDefaultAsync(rp => rp.Id == id);

        if (roomPrice == null)
        {
            throw new NotFoundException("Room price not found");
        }

        var response = _mapper.Map<RoomPriceResponseModel>(roomPrice);
        return response;
    }

    public async Task<string> EditRoomPrice(Guid id, UpdateRoomPriceDto updateDto)
    {
        var roomPrice = await _dbcontext.RoomPrices.FindAsync(id);

        if (roomPrice == null)
        {
            throw new NotFoundException("Room price not found");
        }
        
        if (updateDto.EndDate <= updateDto.StartDate)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "EndDate",
                    Issue = "End date must be after start date."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var hasOverlap = await _dbcontext.RoomPrices
            .Where(rp => rp.RoomId == roomPrice.RoomId && rp.Id != id)
            .Where(rp => rp.IsActive)
            .Where(rp => rp.DayType == updateDto.DayType || rp.DayType == DayType.All || updateDto.DayType == DayType.All)
            .AnyAsync(rp => 
                (updateDto.StartDate >= rp.StartDate && updateDto.StartDate < rp.EndDate) ||
                (updateDto.EndDate > rp.StartDate && updateDto.EndDate <= rp.EndDate) ||
                (updateDto.StartDate <= rp.StartDate && updateDto.EndDate >= rp.EndDate));

        if (hasOverlap)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "DateRange",
                    Issue = "Price period overlaps with existing active price for this room."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        roomPrice.SeasonName = updateDto.SeasonName;
        roomPrice.PricePerNight = updateDto.PricePerNight;
        roomPrice.StartDate = updateDto.StartDate;
        roomPrice.EndDate = updateDto.EndDate;
        roomPrice.DayType = updateDto.DayType;
        roomPrice.IsActive = updateDto.IsActive ?? roomPrice.IsActive;
        roomPrice.UpdatedAt = DateTime.UtcNow;

        _dbcontext.RoomPrices.Update(roomPrice);
        await _dbcontext.SaveChangesAsync();
        
        return "Room price has been updated successfully.";
    }

    public async Task<string> DeleteRoomPrice(Guid id)
    {
        var roomPrice = await _dbcontext.RoomPrices.FindAsync(id);
        
        if (roomPrice == null)
        {
            throw new NotFoundException("Room price not found");
        }

        _dbcontext.RoomPrices.Remove(roomPrice);
        await _dbcontext.SaveChangesAsync();
        
        return "Room price has been deleted successfully.";
    }

    public async Task<string> ToggleRoomPriceStatus(Guid id)
    {
        var roomPrice = await _dbcontext.RoomPrices.FindAsync(id);

        if (roomPrice == null)
        {
            throw new NotFoundException("Room price not found");
        }

        roomPrice.IsActive = !roomPrice.IsActive;
        roomPrice.UpdatedAt = DateTime.UtcNow;

        await _dbcontext.SaveChangesAsync();
        
        return "Room price has been toggled successfully.";
    }

    public async Task<CurrentRoomPriceResponseModel> GetCurrentRoomPrice(Guid roomId, DateTime? date = null)
    {
        var checkDate = date ?? DateTime.UtcNow;
        var dayOfWeek = checkDate.DayOfWeek;
        var dayType = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) 
            ? DayType.Weekend 
            : DayType.Weekday;
        
        var roomPrice = await _dbcontext.RoomPrices
            .Where(rp => rp.RoomId == roomId)
            .Where(rp => rp.IsActive)
            .Where(rp => rp.StartDate <= checkDate && rp.EndDate >= checkDate)
            .Where(rp => rp.DayType == DayType.All || rp.DayType == dayType)
            .OrderByDescending(rp => rp.PricePerNight)
            .FirstOrDefaultAsync();
        
        if (roomPrice != null)
        {
            var response =  new CurrentRoomPriceResponseModel
            { 
                price = roomPrice.PricePerNight,
                seasonName = roomPrice.SeasonName.ToString(),
                dayType = roomPrice.DayType.ToString(),
                source = "DynamicPrice"
            };
            
            return response;
        }
        
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }

        var responseBase = new CurrentRoomPriceResponseModel
        {
            price = room.RoomType.PricePerNight,
            source = "BasePrice"
        };
        
        return responseBase;
    }

    public async Task<List<PriceCalendarDto>> GetRoomPriceCalendar(Guid roomId,
        PriceCalendarRequestParameters parameters)
    {
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        if (parameters.StartDate <= parameters.EndDate)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "EndDate",
                    Issue = "End date must be after start date."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var prices = await _dbcontext.RoomPrices
            .Where(rp => rp.RoomId == roomId)
            .Where(rp => rp.IsActive)
            .Where(rp => rp.StartDate <= parameters.EndDate && rp.EndDate >= parameters.StartDate)
            .ToListAsync();
        
        var calendar = new List<PriceCalendarDto>();
        var currentDate = parameters.StartDate;
        
        while (currentDate <= parameters.EndDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var dayType = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) 
                ? DayType.Weekend 
                : DayType.Weekday;

            var applicablePrice = prices
                .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate)
                .Where(p => p.DayType == DayType.All || p.DayType == dayType)
                .OrderByDescending(p => p.PricePerNight)
                .FirstOrDefault();

            calendar.Add(new PriceCalendarDto
            {
                Date = currentDate,
                DayOfWeek = currentDate.ToString("dddd"),
                Price = applicablePrice?.PricePerNight ?? room.RoomType.PricePerNight,
                SeasonName = applicablePrice?.SeasonName.ToString(),
                IsWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday,
                IsBasePrice = applicablePrice == null
            });

            currentDate = currentDate.AddDays(1);
        }

        return calendar;
    }
}