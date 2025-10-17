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

public class BookingServiceService : IBookingServiceService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public BookingServiceService(AppDbContext context, IMapper mapper)
    {
        _dbContext = context;
        _mapper = mapper;
    }

    public async Task<string> AddBookingServiceAsync(AddBookingServiceDto dto)
    {
        var booking = await _dbContext.Bookings.FindAsync(dto.BookingId);
        if (booking == null)
            throw new NotFoundException("Booking not found.");

        var service = await _dbContext.Services.FindAsync(dto.ServiceId);
        if (service == null)
            throw new NotFoundException("Service not found or inactive.");

        if (dto.Quantity <= 0)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "quantity",
                    Issue = "Quantity must be greater than 0."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var bookingService = new BookingService
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            ServiceId = dto.ServiceId,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.BookingServices.Add(bookingService);
        await _dbContext.SaveChangesAsync();


        return $"New booking service added: {dto.Quantity} x {service.Name}";
    }

    public async Task<PageList<BookingServiceResponseModel>> GetAllBookingServices(
        BookingServiceRequestParameters parameters)
    {

        var query = _dbContext.BookingServices
            .Include(bs => bs.Booking)
            .ThenInclude(b => b.Customer)
            .Include(bs => bs.Service)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
        {
            var searchLower = parameters.SearchKey.ToLower();
            query = query.Where(bs =>
                bs.Service.Name.Contains(searchLower) ||
                bs.Booking.Customer.FullName.Contains(searchLower));
        }

        if (parameters.BookingId.HasValue)
        {
            query = query.Where(bs => bs.BookingId == parameters.BookingId);
        }

        if (parameters.ServiceId.HasValue)
        {
            query = query.Where(bs => bs.ServiceId == parameters.ServiceId);
        }

        if (parameters.CreatedAfter.HasValue)
        {
            query = query.Where(bs => bs.CreatedAt >= parameters.CreatedAfter);
        }

        if (parameters.CreatedBefore.HasValue)
        {
            query = query.Where(bs => bs.CreatedAt <= parameters.CreatedBefore);
        }

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CreatedAt);
        else
            query = query.ApplySort(parameters.OrderBy);

        var results = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var response = _mapper.Map<List<BookingServiceResponseModel>>(results);

        return new PageList<BookingServiceResponseModel>(response, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<BookingServiceResponseModel> GetBookingServiceById(Guid id)
    {
        var bookingService = await _dbContext.BookingServices
            .Include(bs => bs.Booking)
            .ThenInclude(b => b.Customer)
            .Include(bs => bs.Service)
            .FirstOrDefaultAsync(bs => bs.Id == id);

        if (bookingService == null)
            throw new NotFoundException("Booking service not found");

        return _mapper.Map<BookingServiceResponseModel>(bookingService);
    }

    public async Task<string> EditBookingService(Guid id, UpdateBookingServiceDto bookingServiceDto)
    {
        
        var bookingService = await _dbContext.BookingServices
            .Include(bs => bs.Service)
            .FirstOrDefaultAsync(bs => bs.Id == id);

        if (bookingService == null)
            throw new NotFoundException("Booking service not found");

        if (bookingServiceDto.Quantity <= 0)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "quantity",
                    Issue = "Quantity must be greater than 0."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }


        bookingService.Quantity = bookingServiceDto.Quantity;
        bookingService.UpdatedAt = DateTime.UtcNow;

        _dbContext.BookingServices.Update(bookingService);
        await _dbContext.SaveChangesAsync();

        return "Booking service updated successfully";
    }
    
    public async Task<string> DeleteBookingService(Guid id)
    {
        var bookingService = await _dbContext.BookingServices
            .FirstOrDefaultAsync(bs => bs.Id == id);

        if (bookingService == null)
            return "Booking service not found";
        
        bookingService.UpdatedAt = DateTime.UtcNow;

        _dbContext.BookingServices.Update(bookingService);
        await _dbContext.SaveChangesAsync();
        
        return "Booking service deleted successfully";
    }
}

