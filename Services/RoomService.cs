using System.Text.Json;
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

public class RoomService: IRoomService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public RoomService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }

    public async Task<string> AddRoomAsync(AddRoomDto addRoomDto)
    {
        var roomType = await _dbcontext.RoomTypes.FindAsync(addRoomDto.RoomTypeId);
        if (roomType == null)
        {
            throw new NotFoundException("RoomType not found");
        }
        
        var floor = await _dbcontext.Floors.FindAsync(addRoomDto.FloorId);
        if (floor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        var existingRoom = await _dbcontext.Rooms
            .Where(r => r.RoomNumber == addRoomDto.RoomNumber)
            .FirstOrDefaultAsync();
        
        if (existingRoom != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "RoomNumber",
                    Issue = "Room number is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var newRoom = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = addRoomDto.RoomNumber,
            RoomName = addRoomDto.RoomName,
            RoomTypeId = addRoomDto.RoomTypeId,
            FloorId = addRoomDto.FloorId,
            Position = addRoomDto.Position,
            Capacity = addRoomDto.Capacity,
            NumberOfBeds = addRoomDto.NumberOfBeds,
            BedType = addRoomDto.BedType,
            SmokingAllowed = addRoomDto.SmokingAllowed,
            PetFriendly = addRoomDto.PetFriendly,
            Accessible = addRoomDto.Accessible,
            Status = RoomStatus.Available,
            Notes = addRoomDto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbcontext.Rooms.Add(newRoom);
        await _dbcontext.SaveChangesAsync();
        
        if (addRoomDto.AmenityIds != null && addRoomDto.AmenityIds.Any())
        {
            foreach (var amenityId in addRoomDto.AmenityIds)
            {
                var amenity = await _dbcontext.Amenities.FindAsync(amenityId);
                if (amenity != null)
                {
                    _dbcontext.RoomAmenities.Add(new RoomAmenity
                    {
                        Id = Guid.NewGuid(),
                        RoomId = newRoom.Id,
                        AmenityId = amenityId,
                        Quantity = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _dbcontext.SaveChangesAsync();
        }

        return "Room has been added successfully.";
    }

    public async Task<PageList<RoomResponseModel>> GetAllRooms(RoomRequestParameters parameters)
    {
        var query = _dbcontext.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.Floor)
            .Include(r => r.RoomAmenities)
            .ThenInclude(ra => ra.Amenity)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.RoomNumber != null && x.RoomName.Contains(parameters.SearchKey));
        
        if (parameters.RoomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == parameters.RoomTypeId.Value);
        
        if (parameters.FloorId.HasValue)
            query = query.Where(r => r.FloorId == parameters.FloorId.Value);
        
        if (parameters.Status.HasValue)
            query = query.Where(r => r.Status == parameters.Status.Value);
        
        if (parameters.IsActive.HasValue)
            query = query.Where(r => r.IsActive == parameters.IsActive.Value);
        
        if (parameters.SmokingAllowed.HasValue)
            query = query.Where(r => r.SmokingAllowed == parameters.SmokingAllowed.Value);

        if (parameters.PetFriendly.HasValue)
            query = query.Where(r => r.PetFriendly == parameters.PetFriendly.Value);

        if (parameters.Accessible.HasValue)
            query = query.Where(r => r.Accessible == parameters.Accessible.Value);
        
        if (parameters.MinCapacity.HasValue)
            query = query.Where(r => r.Capacity >= parameters.MinCapacity.Value);
        
        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.RoomNumber);
        else
            query = query.ApplySort(parameters.OrderBy);
        
        var rooms = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        var roomsResponse = _mapper.Map<List<RoomResponseModel>>(rooms);
        
        return new PageList<RoomResponseModel>(roomsResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<RoomDetailResponseModel> GetRoomById(Guid id)
    {
        var room = await _dbcontext.Rooms
            .Include(r => r.RoomType)
            .ThenInclude(r => r.RoomPrices.Where(rp => rp.IsActive))
            .Include(r => r.Floor)
            .Include(r => r.RoomAmenities)
            .ThenInclude(ra => ra.Amenity)
            .Include(r => r.StatusHistories.OrderByDescending(sh => sh.ChangedAt).Take(10))
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }

        var response = _mapper.Map<RoomDetailResponseModel>(room);
        return response;
    }

    public async Task<string> UpdateRoom(Guid id, UpdateRoomDto updateRoomDto)
    {
        var room = await _dbcontext.Rooms.FindAsync(id);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }

        var roomType = await _dbcontext.RoomTypes.FindAsync(updateRoomDto.RoomTypeId);
        if (roomType == null)
        {
            throw new NotFoundException("RoomType not found");
        }
        
        var floor = await _dbcontext.Floors.FindAsync(updateRoomDto.FloorId);
        if (floor == null)
        {
            throw new NotFoundException("Floor not found");
        }
        
        var existingRoom = await _dbcontext.Rooms
            .FirstOrDefaultAsync(r => r.RoomNumber == updateRoomDto.RoomNumber && r.Id != id);

        if (existingRoom != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "RoomNumber",
                    Issue = "Room number is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        room.RoomNumber = updateRoomDto.RoomNumber;
        room.RoomName = updateRoomDto.RoomName;
        room.RoomTypeId = updateRoomDto.RoomTypeId;
        room.FloorId = updateRoomDto.FloorId;
        room.Position = updateRoomDto.Position;
        room.Capacity = updateRoomDto.Capacity;
        room.NumberOfBeds = updateRoomDto.NumberOfBeds;
        room.BedType = updateRoomDto.BedType;
        room.SmokingAllowed = updateRoomDto.SmokingAllowed;
        room.PetFriendly = updateRoomDto.PetFriendly;
        room.Accessible = updateRoomDto.Accessible;
        room.Notes = updateRoomDto.Notes;
        room.UpdatedAt = DateTime.UtcNow;

        _dbcontext.Rooms.Update(room);
        await _dbcontext.SaveChangesAsync();
        
        return "Room has been updated successfully.";
    }

    public async Task<string> DeleteRoom(Guid id)
    {
        var room = await _dbcontext.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        if (room.Bookings.Count > 0)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "Room",
                    Issue = $"Cannot delete room because it has {room.Bookings.Count} booking(s)."
                }
            };
            throw new BadRequestException("INVALID_OPERATION", errors);
        }

        await _dbcontext.SaveChangesAsync();

        return "Room has been deleted successfully.";
    }

    public async Task<string> ChangeRoomStatus(Guid id, ChangeRoomStatusDto statusDto)
    {
        var room = await _dbcontext.Rooms.FindAsync(id);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        var oldStatus = room.Status;
        room.Status = statusDto.NewStatus;
        room.UpdatedAt = DateTime.UtcNow;
        
        if (statusDto.NewStatus == RoomStatus.Cleaning)
        {
            room.LastCleanedAt = DateTime.UtcNow;
        }
        else if (statusDto.NewStatus == RoomStatus.Maintenance)
        {
            room.LastMaintenanceAt = DateTime.UtcNow;
        }
        
        _dbcontext.RoomStatusHistories.Add(new RoomStatusHistory
        {
            Id = Guid.NewGuid(),
            RoomId = id,
            OldStatus = oldStatus,
            NewStatus = statusDto.NewStatus,
            ChangedBy = statusDto.ChangedBy ?? "System",
            ChangeReason = statusDto.Reason,
            ChangedAt = DateTime.UtcNow
        });

        await _dbcontext.SaveChangesAsync();
        
        return  "Room status has been updated.";
    }

    public async Task<string> UpdateRoomAmenities(Guid id, UpdateRoomAmenitiesDto amenitiesDto)
    {
        var room = await _dbcontext.Rooms.FindAsync(id);
        
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }
        
        foreach (var amenityId in amenitiesDto.AmenityIds)
        {
            var amenity = await _dbcontext.Amenities.FindAsync(amenityId);
            if (amenity != null && amenity.IsActive)
            {
                _dbcontext.RoomAmenities.Add(new RoomAmenity
                {
                    Id = Guid.NewGuid(),
                    RoomId = id,
                    AmenityId = amenityId,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _dbcontext.SaveChangesAsync();

        return "Room amenities have been updated successfully.";
    }

    public async Task<List<RoomResponseModel>> GetAvailableRooms(AvailabelRoomParameters parameters)
    {
        var query = _dbcontext.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.Floor)
            .Where(r => r.IsActive)
            .Where(r => r.Status == RoomStatus.Available);
        
        if (parameters.MinCapacity.HasValue)
            query = query.Where(r => r.Capacity >= parameters.MinCapacity.Value);
        
        if (parameters.RoomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == parameters.RoomTypeId.Value);
        
        if (parameters.FloorId.HasValue)
            query = query.Where(r => r.FloorId == parameters.FloorId.Value);
        
        if (parameters.Checkin.HasValue && parameters.Checkout.HasValue)
        {
            var bookedRoomIds = await _dbcontext.Bookings
                .Where(b => b.Status != BookingStatus.Canceled)
                .Where(b => 
                    (parameters.Checkin.Value >= b.CheckInDate && parameters.Checkin.Value < b.CheckOutDate) ||
                    (parameters.Checkout.Value > b.CheckInDate && parameters.Checkout.Value <= b.CheckOutDate) ||
                    (parameters.Checkin.Value <= b.CheckInDate && parameters.Checkout.Value >= b.CheckOutDate))
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => !bookedRoomIds.Contains(r.Id));
        }
        
        var rooms = await query.OrderBy(r => r.RoomNumber).ToListAsync();
        var response = _mapper.Map<List<RoomResponseModel>>(rooms);
        return response;
    }
    
    public async Task<RoomStatisticsResponse> GetRoomStatistics()
    {
        var totalRooms = await _dbcontext.Rooms.CountAsync(r => r.IsActive);
        
        var statusCounts = await _dbcontext.Rooms
            .Where(r => r.IsActive)
            .GroupBy(r => r.Status)
            .Select(g => new StatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
        
        var floorStats = await _dbcontext.Floors
            .Select(f => new FloorStatDto
            {
                FloorId = f.Id,
                FloorNumber = f.FloorNumber,
                FloorName = f.FloorName,
                TotalRooms = f.Rooms.Count(r => r.IsActive),
                AvailableRooms = f.Rooms.Count(r => r.IsActive && r.Status == RoomStatus.Available),
                OccupiedRooms = f.Rooms.Count(r => r.IsActive && r.Status == RoomStatus.Occupied)
            })
            .ToListAsync();

        var response = new RoomStatisticsResponse
        {
            TotalRooms = totalRooms,
            StatusCounts = statusCounts,
            FloorStatistics = floorStats,
            OccupancyRate = totalRooms > 0 
                ? (double)statusCounts.FirstOrDefault(s => s.Status == RoomStatus.Occupied)?.Count / totalRooms * 100 
                : 0
        };

        return response;
    }

    public async Task<List<RoomStatusHistoryDto>> GetRoomStatusHistory(Guid id)
    {
        var room = await _dbcontext.Rooms.FindAsync(id);
        if (room == null)
        {
            throw new NotFoundException("Room not found");
        }

        var history = await _dbcontext.RoomStatusHistories
            .Where(h => h.RoomId == id)
            .OrderByDescending(h => h.ChangedAt)
            .Take(50)
            .ToListAsync();

        var response = _mapper.Map<List<RoomStatusHistoryDto>>(history);
        return response;
    }
}