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

public class BookingHotelService: IBookingService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    private readonly IRoomPricesService _roomPricesService;
    
    public BookingHotelService(AppDbContext context, IMapper mapper, IRoomPricesService roomPricesService)
    {
        _dbcontext = context;
        _mapper = mapper;
        _roomPricesService = roomPricesService;
    }

    public async Task<string> AddBookingAsync(AddBookingDto addBookingDto) 
    {
        if (addBookingDto.CheckInDate >= addBookingDto.CheckOutDate)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "checkOutDate",
                    Issue = "Check-in date must be before check-out date."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        if (addBookingDto.CheckInDate < DateTime.Now.Date)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "checkInDate",
                    Issue = "Check-in date cannot be in the past."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        if (addBookingDto.RoomIds == null || !addBookingDto.RoomIds.Any())
        {
            throw new BadRequestException("INVALID_FIELD", new List<FieldError>
            {
                new() { Field = "roomIds", Issue = "At least one room must be selected." }
            });
        }
        
        var rooms = await _dbcontext.Rooms
            .Where(r => addBookingDto.RoomIds.Contains(r.Id))
            .ToListAsync();

        if (rooms.Count != addBookingDto.RoomIds.Count)
        {
            var missingRoomIds = addBookingDto.RoomIds.Except(rooms.Select(r => r.Id)).ToList();
            throw new NotFoundException($"Rooms not found: {string.Join(", ", missingRoomIds)}");
        }
        
        Guid customerId;
        
        if (addBookingDto.CustomerId.HasValue)
        {
            var existingCustomer = await _dbcontext.Customers.FindAsync(addBookingDto.CustomerId.Value);
            if (existingCustomer == null)
            {
                throw new NotFoundException("Customer not found.");
            }
            customerId = addBookingDto.CustomerId.Value;
        }
        else if (addBookingDto.CustomerInfo != null)
        {
            var existingCustomer = await _dbcontext.Customers
                .FirstOrDefaultAsync(c => 
                    c.Phone == addBookingDto.CustomerInfo.Phone ||
                    c.Email == addBookingDto.CustomerInfo.Email);

            if (existingCustomer != null)
            {
                customerId = existingCustomer.Id;
            }
            else
            {
                var newCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = addBookingDto.CustomerInfo.FullName,
                    Email = addBookingDto.CustomerInfo.Email,
                    Phone = addBookingDto.CustomerInfo.Phone,
                    IDCard = addBookingDto.CustomerInfo.IDCard,
                };

                _dbcontext.Customers.Add(newCustomer);
                await _dbcontext.SaveChangesAsync();
                customerId = newCustomer.Id;
            }
        }
        else
        {
            throw new BadRequestException("INVALID_FIELD", new List<FieldError>
            {
                new() { Field = "customer", Issue = "Please provide either CustomerId or CustomerInfo." }
            });
        }
        
        var createdBookings = new List<Booking>();
        var unavailableRooms = new List<string>();

        foreach (var roomId in addBookingDto.RoomIds)
        {
            var room = rooms.First(r => r.Id == roomId);
            
            var isAvailable = await IsRoomAvailableAsync(
                roomId,
                addBookingDto.CheckInDate,
                addBookingDto.CheckOutDate
            );

            if (!isAvailable)
            {
                unavailableRooms.Add(room.RoomNumber);
                continue;
            }
            
            var totalAmount = await CalculateTotalAmountAsync(
                roomId,
                addBookingDto.CheckInDate,
                addBookingDto.CheckOutDate
            );
            
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                CustomerId = customerId,
                CheckInDate = addBookingDto.CheckInDate,
                CheckOutDate = addBookingDto.CheckOutDate,
                TotalAmount = totalAmount,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
            };

            _dbcontext.Bookings.Add(booking);
            createdBookings.Add(booking);
        }
        
        if (unavailableRooms.Any())
        {
            if (createdBookings.Count == 0)
            {
                throw new BadRequestException("INVALID_FIELD", new List<FieldError>
                {
                    new() { Field = "room", Issue = $"All selected rooms are not available for the selected dates: {string.Join(", ", unavailableRooms)}" }
                });
            }
            else
            {
                await _dbcontext.SaveChangesAsync();
                throw new BadRequestException("INVALID_FIELD", new List<FieldError>
                {
                    new() { Field = "room", Issue = $"Booking created for {createdBookings.Count} room(s). " +
                                                        $"The following rooms were not available: {string.Join(", ", unavailableRooms)}" }
                });
            };
        }
        
        
        await _dbcontext.SaveChangesAsync();

        return "Booking has been created successfully.";
    }
    
    public async Task<PageList<BookingResponseModel>> GetAllBookings(BookingRequestParameters parameters)
    {
        var query = _dbcontext.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Floor)
            .Include(b => b.Customer)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
            .Include(b => b.Invoices)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(b =>
                b.Room.RoomNumber.Contains(parameters.SearchKey) ||
                b.Customer.FullName.Contains(parameters.SearchKey) ||
                b.Customer.Email != null && b.Customer.Email.Contains(parameters.SearchKey));
        
        if (parameters.Status.HasValue)
        {
            query = query.Where(b => b.Status == parameters.Status.Value);
        }

        if (parameters.RoomId.HasValue)
        {
            query = query.Where(b => b.RoomId == parameters.RoomId.Value);
        }

        if (parameters.CheckInDateFrom.HasValue)
        {
            query = query.Where(b => b.CheckInDate >= parameters.CheckInDateFrom.Value);
        }

        if (parameters.CheckInDateTo.HasValue)
        {
            query = query.Where(b => b.CheckInDate <= parameters.CheckInDateTo.Value);
        }

        if (parameters.CheckOutDateFrom.HasValue)
        {
            query = query.Where(b => b.CheckOutDate >= parameters.CheckOutDateFrom.Value);
        }

        if (parameters.CheckOutDateTo.HasValue)
        {
            query = query.Where(b => b.CheckOutDate <= parameters.CheckOutDateTo.Value);
        }
        
        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CheckInDate);
        else
            query = query.ApplySort(parameters.OrderBy);

        var bookings = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var bookingResponse = _mapper.Map<List<BookingResponseModel>>(bookings);

        return new PageList<BookingResponseModel>(
            bookingResponse,
            query.Count(),
            parameters.PageNumber,
            parameters.PageSize
        );
    }
    
    public async Task<BookingResponseModel> GetBookingById(Guid id)
    {
        var booking = await _dbcontext.Bookings
            .Include(b => b.Room)
            .ThenInclude(r => r.RoomType)
            .Include(b => b.Room)
            .ThenInclude(r => r.Floor)
            .Include(b => b.Customer)
            .Include(b => b.BookingServices)
            .ThenInclude(bs => bs.Service)
            .Include(b => b.Invoices)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }

        return _mapper.Map<BookingResponseModel>(booking);
    }
    
    public async Task<string> EditBooking(Guid id, UpdateBookingDto updateBookingDto)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }
        
        if (updateBookingDto.CheckInDate >= updateBookingDto.CheckOutDate)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "checkInDate",
                    Issue = "Check-in date must be before check-out date."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var datesOrRoomChanged = booking.RoomId != updateBookingDto.RoomId ||
                                 booking.CheckInDate != updateBookingDto.CheckInDate ||
                                 booking.CheckOutDate != updateBookingDto.CheckOutDate;

        if (datesOrRoomChanged)
        {
            var isAvailable = await IsRoomAvailableAsync(
                updateBookingDto.RoomId,
                updateBookingDto.CheckInDate,
                updateBookingDto.CheckOutDate,
                id
            );

            if (!isAvailable)
            {
                return "Room is not available for the selected dates.";
            }
            
            booking.TotalAmount = await CalculateTotalAmountAsync(
                updateBookingDto.RoomId,
                updateBookingDto.CheckInDate,
                updateBookingDto.CheckOutDate
            );
        }
        
        booking.UpdatedAt = DateTime.Now;
        
        _dbcontext.Bookings.Update(booking);
        await _dbcontext.SaveChangesAsync();
        
        return "Booking updated successfully.";
    }
    
    public async Task<string> DeleteBooking(Guid id)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }

        _dbcontext.Bookings.Remove(booking);
        await _dbcontext.SaveChangesAsync();
        
        return "Booking deleted successfully.";
    }
    
    public async Task<string> CancelBookingAsync(Guid id)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }

        if (booking.Status == BookingStatus.CheckedOut || booking.Status == BookingStatus.Canceled)
        {
            return $"Cannot cancel a booking with status {booking.Status}.";
        }

        booking.Status = BookingStatus.Canceled;
        booking.UpdatedAt = DateTime.Now;
        
        _dbcontext.Bookings.Update(booking);
        await _dbcontext.SaveChangesAsync();
        
        return "Booking cancelled successfully.";
    }
    
    public async Task<string> ConfirmBookingAsync(Guid id)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return "Only pending bookings can be confirmed.";
        }

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.Now;
        
        _dbcontext.Bookings.Update(booking);
        await _dbcontext.SaveChangesAsync();
        return "Booking confirmed successfully.";
    }
    
    public async Task<string> CheckInBookingAsync(Guid id)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return "Only confirmed bookings can be checked in.";
        }

        if (booking.CheckInDate.Date > DateTime.Now.Date)
        {
            return "Cannot check in before the check-in date.";
        }

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.Now;
        
        _dbcontext.Bookings.Update(booking);
        await _dbcontext.SaveChangesAsync();
        return "Checked in successfully.";
    }
    
    public async Task<string> CheckOutBookingAsync(Guid id)
    {
        var booking = await _dbcontext.Bookings.FindAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");

        }

        if (booking.Status != BookingStatus.CheckedIn)
        {
            return "Only checked-in bookings can be checked out.";
        }

        booking.Status = BookingStatus.CheckedOut;
        booking.UpdatedAt = DateTime.Now;
        
        _dbcontext.Bookings.Update(booking);
        await _dbcontext.SaveChangesAsync();
        return "Checked out successfully.";
    }
    
    public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkIn, DateTime checkOut, Guid? excludeBookingId = null)
    {
        var overlappingBookings = await _dbcontext.Bookings
            .Where(b => b.RoomId == roomId
                        && b.Status != BookingStatus.Canceled
                        && b.Status != BookingStatus.CheckedOut
                        && b.Id != excludeBookingId
                        && (
                            (b.CheckInDate < checkOut && b.CheckOutDate > checkIn) ||
                            (checkIn < b.CheckOutDate && checkOut > b.CheckInDate)
                        ))
            .AnyAsync();

        return !overlappingBookings;
    }
    
    public async Task<decimal> CalculateTotalAmountAsync(Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
        {
            throw new BadRequestException("Check-out date must be after check-in date.");
        }

        if (checkIn.Date < DateTime.UtcNow.Date)
        {
            throw new BadRequestException("Check-in date cannot be in the past.");
        }
        
        var room = await _dbcontext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId);
    
        if (room == null)
        {
            throw new NotFoundException($"Room with ID {roomId} not found.");
        }

        decimal totalAmount = 0m;
        
        for (var date = checkIn.Date; date < checkOut.Date; date = date.AddDays(1))
        {
            var dailyPriceInfo = await _roomPricesService.GetCurrentRoomPrice(roomId, date);
            totalAmount += dailyPriceInfo.Price;
        }

        return totalAmount;
    }
}