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

public class RoomPricesService : IRoomPricesService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public RoomPricesService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    
    public async Task<string> AddRoomPrices(AddRoomPriceDto dto)
    {
        var roomType = await _dbcontext.RoomTypes.FindAsync(dto.RoomTypeId);
        if (roomType == null)
        {
            throw new NotFoundException("Room type not found");
        }
        
        if (dto.EndDate <= dto.StartDate)
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
        
        if (dto.PricePerNight <= 0)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "PricePerNight",
                    Issue = "Price per night must be greater than 0."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var hasOverlap = await _dbcontext.RoomPrices
            .Where(rtp => rtp.RoomTypeId == dto.RoomTypeId)
            .Where(rtp => rtp.IsActive)
            .Where(rtp => rtp.DayType == dto.DayType || rtp.DayType == DayType.All || dto.DayType == DayType.All)
            .AnyAsync(rtp =>
                (dto.StartDate >= rtp.StartDate && dto.StartDate < rtp.EndDate) ||
                (dto.EndDate > rtp.StartDate && dto.EndDate <= rtp.EndDate) ||
                (dto.StartDate <= rtp.StartDate && dto.EndDate >= rtp.EndDate));

        if (hasOverlap)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "DateRange",
                    Issue = "Price period overlaps with existing active price for this room type."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newPrice = new RoomPrice
        {
            Id = Guid.NewGuid(),
            RoomTypeId = dto.RoomTypeId,
            SeasonName = dto.SeasonName,
            PricePerNight = dto.PricePerNight,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DayType = dto.DayType,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbcontext.RoomPrices.AddAsync(newPrice);
        await _dbcontext.SaveChangesAsync();

        return "Room type price added successfully.";
    }

    public async Task<string> EditRoomPrice(Guid id, UpdateRoomPriceDto dto)
    {
        var price = await _dbcontext.RoomPrices.FindAsync(id);
        if (price == null)
        {
            throw new NotFoundException("Room type price not found.");
        }

        if (dto.EndDate <= dto.StartDate)
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

        if (dto.PricePerNight <= 0)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "PricePerNight",
                    Issue = "Price per night must be greater than 0."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var hasOverlap = await _dbcontext.RoomPrices
            .Where(rtp => rtp.RoomTypeId == price.RoomTypeId && rtp.Id != id)
            .Where(rtp => rtp.IsActive)
            .Where(rtp => rtp.DayType == dto.DayType || rtp.DayType == DayType.All || dto.DayType == DayType.All)
            .AnyAsync(rtp =>
                (dto.StartDate >= rtp.StartDate && dto.StartDate < rtp.EndDate) ||
                (dto.EndDate > rtp.StartDate && dto.EndDate <= rtp.EndDate) ||
                (dto.StartDate <= rtp.StartDate && dto.EndDate >= rtp.EndDate));

        if (hasOverlap)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "DateRange",
                    Issue = "Price period overlaps with existing active price."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        price.SeasonName = dto.SeasonName;
        price.PricePerNight = dto.PricePerNight;
        price.StartDate = dto.StartDate;
        price.EndDate = dto.EndDate;
        price.DayType = dto.DayType;
        price.IsActive = dto.IsActive;
        price.Priority = dto.Priority;
        price.UpdatedAt = DateTime.UtcNow;

        await _dbcontext.SaveChangesAsync();

        return "Room type price updated successfully.";
    }

    public async Task<string> DeleteRoomPrice(Guid id)
    {
        var price = await _dbcontext.RoomPrices.FindAsync(id);
        if (price == null)
        {
            throw new NotFoundException("Room type price not found.");
        }

        _dbcontext.RoomPrices.Remove(price);
        await _dbcontext.SaveChangesAsync();

        return "Room type price deleted successfully.";
    }

    public async Task<List<RoomPriceResponseModel>> GetRoomPriceByRoomTypeId(Guid roomTypeId)
    {
        var prices = await _dbcontext.RoomPrices
            .Where(rtp => rtp.RoomTypeId == roomTypeId)
            .Include(rtp => rtp.RoomType)
            .OrderBy(rtp => rtp.StartDate)
            .ThenBy(rtp => rtp.Priority)
            .ToListAsync();

        return _mapper.Map<List<RoomPriceResponseModel>>(prices);
    }
    
    public async Task<RoomPriceResponseModel> GetRoomPriceById(Guid id)
    {
        var roomPrice = await _dbcontext.RoomPrices
            .Include(rtp => rtp.RoomType)
            .FirstOrDefaultAsync(rp => rp.Id == id);

        if (roomPrice == null)
        {
            throw new NotFoundException("Room price not found");
        }

        var response = _mapper.Map<RoomPriceResponseModel>(roomPrice);
        return response;
    }
    
    public async Task<PageList<RoomPriceResponseModel>> GetAllRoomPrices(
        RoomPriceRequestParameters parameters)
    {
        var query = _dbcontext.RoomPrices
            .Include(rtp => rtp.RoomType)
            .OrderBy(rtp => rtp.StartDate)
            .ThenBy(rtp => rtp.Priority)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.RoomType.Name != null && x.RoomType.Name.Contains(parameters.SearchKey));
        
        if (parameters.RoomTypeId.HasValue)
        {
            query = query.Where(rp => rp.RoomTypeId == parameters.RoomTypeId.Value);
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

    public async Task<string> ToggleRoomPriceStatus(Guid id)
    {
        var price = await _dbcontext.RoomPrices.FindAsync(id);
        if (price == null)
        {
            throw new NotFoundException("Room type price not found.");
        }

        price.IsActive = !price.IsActive;
        price.UpdatedAt = DateTime.UtcNow;

        await _dbcontext.SaveChangesAsync();

        return $"Room type price {(price.IsActive ? "activated" : "deactivated")} successfully.";
    }
    
    public async Task<string> AddRoomPriceOverride(AddRoomPriceOverrideDto dto)
    {
        var room = await _dbcontext.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            throw new NotFoundException("Room not found.");
        }
        
        var existingOverride = await _dbcontext.RoomPriceOverrides
            .Where(po => po.RoomId == dto.RoomId && po.IsActive)
            .Where(po => !po.EffectiveTo.HasValue || po.EffectiveTo >= DateTime.UtcNow)
            .FirstOrDefaultAsync();
    
        if (existingOverride != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "RoomId",
                    Issue = "Active price override already exists for this room."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
    
        var newOverride = new RoomPriceOverride
        {
            Id = Guid.NewGuid(),
            RoomId = dto.RoomId,
            PriceAdjustment = dto.PriceAdjustment,
            AdjustmentType = dto.AdjustmentType,
            Reason = dto.Reason,
            IsActive = dto.IsActive,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    
        await _dbcontext.RoomPriceOverrides.AddAsync(newOverride);
        await _dbcontext.SaveChangesAsync();
    
        return "Room price override added successfully.";
    }
    
    public async Task<string> UpdateRoomPriceOverride(Guid id, UpdateRoomPriceOverrideDto dto)
    {
        var priceOverride = await _dbcontext.RoomPriceOverrides.FindAsync(id);
        if (priceOverride == null)
        {
            throw new NotFoundException("Room price override not found.");
        }
    
        priceOverride.PriceAdjustment = dto.PriceAdjustment;
        priceOverride.AdjustmentType = dto.AdjustmentType;
        priceOverride.Reason = dto.Reason;
        priceOverride.IsActive = dto.IsActive;
        priceOverride.EffectiveFrom = dto.EffectiveFrom;
        priceOverride.EffectiveTo = dto.EffectiveTo;
        priceOverride.UpdatedAt = DateTime.UtcNow;
    
        await _dbcontext.SaveChangesAsync();
    
        return "Room price override updated successfully.";
    }
    
    public async Task<string> DeleteRoomPriceOverride(Guid id)
    {
        var priceOverride = await _dbcontext.RoomPriceOverrides.FindAsync(id);
        if (priceOverride == null)
        {
            throw new NotFoundException("Room price override not found.");
        }
    
        _dbcontext.RoomPriceOverrides.Remove(priceOverride);
        await _dbcontext.SaveChangesAsync();
    
        return "Room price override deleted successfully.";
    }
    
    public async Task<RoomPriceOverrideResponseModel?> GetRoomPriceOverride(Guid roomId)
    {
        var priceOverride = await _dbcontext.RoomPriceOverrides
            .Where(po => po.RoomId == roomId)
            .Where(po => po.IsActive)
            .Include(po => po.Room)
            .FirstOrDefaultAsync();
    
        return priceOverride == null ? null : _mapper.Map<RoomPriceOverrideResponseModel>(priceOverride);
    }

    public async Task<RoomPriceCalculationResponse> CalculateRoomPrice(GetRoomPriceRequest request)
    {
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId);
    
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
    
        var response = new RoomPriceCalculationResponse
        {
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            BasePrice = room.RoomType.PricePerNight
        };
        
        var currentDate = request.CheckInDate.Date;
        decimal totalPrice = 0;
    
        while (currentDate < request.CheckOutDate.Date)
        {
            var dayType = GetDayType(currentDate);
            var dailyPrice = await GetDailyPrice(room, currentDate, dayType);
    
            response.DailyBreakdown.Add(new DailyPriceBreakdown
            {
                Date = currentDate,
                Price = dailyPrice.FinalPrice,
                SeasonName = dailyPrice.SeasonName,
                DayType = dayType
            });
    
            totalPrice += dailyPrice.FinalPrice;
            currentDate = currentDate.AddDays(1);
        }
        
        var priceOverride = await _dbcontext.RoomPriceOverrides
            .Where(po => po.RoomId == room.Id)
            .Where(po => po.IsActive)
            .Where(po => !po.EffectiveFrom.HasValue || po.EffectiveFrom <= request.CheckInDate)
            .Where(po => !po.EffectiveTo.HasValue || po.EffectiveTo >= request.CheckOutDate)
            .FirstOrDefaultAsync();
    
        if (priceOverride != null)
        {
            response.OverrideAdjustment = priceOverride.PriceAdjustment;
            response.OverrideReason = priceOverride.Reason;
        }
    
        response.FinalPrice = totalPrice;
        response.SeasonalPrice = totalPrice;
    
        return response;
    }
    
    private async Task<(decimal FinalPrice, string? SeasonName)> GetDailyPrice(
        Room room, DateTime date, DayType dayType)
    {
        var seasonalPrice = await _dbcontext.RoomPrices
            .Where(rtp => rtp.RoomTypeId == room.RoomTypeId)
            .Where(rtp => rtp.IsActive)
            .Where(rtp => date >= rtp.StartDate && date < rtp.EndDate)
            .Where(rtp => rtp.DayType == dayType || rtp.DayType == DayType.All)
            .OrderBy(rtp => rtp.Priority)
            .ThenByDescending(rtp => rtp.DayType)
            .FirstOrDefaultAsync();
    
        decimal basePrice = seasonalPrice?.PricePerNight ?? room.RoomType.PricePerNight;
        string? seasonName = seasonalPrice?.SeasonName.ToString();
        
        var priceOverride = await _dbcontext.RoomPriceOverrides
            .Where(po => po.RoomId == room.Id)
            .Where(po => po.IsActive)
            .Where(po => !po.EffectiveFrom.HasValue || po.EffectiveFrom <= date)
            .Where(po => !po.EffectiveTo.HasValue || po.EffectiveTo >= date)
            .FirstOrDefaultAsync();
    
        if (priceOverride != null)
        {
            basePrice = priceOverride.AdjustmentType == AdjustmentType.Fixed
                ? basePrice + priceOverride.PriceAdjustment
                : basePrice * (1 + priceOverride.PriceAdjustment / 100);
        }
    
        return (basePrice, seasonName);
    }
    
    private DayType GetDayType(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            ? DayType.Weekend
            : DayType.Weekday;
    }
    
    public async Task<CurrentRoomPriceResponseModel> GetCurrentRoomPrice(Guid roomId, DateTime? date = null)
    {
        var checkDate = date ?? DateTime.UtcNow;
        var dayType = GetDayType(checkDate);
    
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        var (finalPrice, seasonName) = await GetDailyPrice(room, checkDate, dayType);
    
        return new CurrentRoomPriceResponseModel
        {
            Price = finalPrice,
            SeasonName = seasonName,
            DayType = dayType.ToString(),
            Source = seasonName != null ? "SeasonalPrice" : "BasePrice",
            HasOverride = await _dbcontext.RoomPriceOverrides
                .AnyAsync(po => po.RoomId == roomId && po.IsActive)
        };
    }
    
    public async Task<List<PriceCalendarDto>> GetRoomPriceCalendar(
        Guid roomId, 
        PriceCalendarRequestParameters parameters)
    {
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        if (parameters.EndDate <= parameters.StartDate)
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
        
        var calendar = new List<PriceCalendarDto>();
        var currentDate = parameters.StartDate;
        
        while (currentDate <= parameters.EndDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var dayType = GetDayType(currentDate);
    
            var (finalPrice, seasonName) = await GetDailyPrice(room, currentDate, dayType);
    
            calendar.Add(new PriceCalendarDto
            {
                Date = currentDate,
                DayOfWeek = currentDate.ToString("dddd"),
                Price = finalPrice,
                SeasonName = seasonName,
                IsWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday,
                IsBasePrice = seasonName == null
            });
    
            currentDate = currentDate.AddDays(1);
        }
    
        return calendar;
    }
}