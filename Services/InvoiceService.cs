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

public class InvoiceService :IInvoiceService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public InvoiceService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }

    public async Task<PageList<InvoiceResponseModel>> GetAllInvoices(RequestParameters parameters)
    {
        var query = _dbcontext.Invoices.Include(i => i.Booking)
            .ThenInclude(b => b.Customer).AsQueryable();

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CreatedAt);
        else
            query = query.ApplySort(parameters.OrderBy);

        var floors = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var floorResponse = _mapper.Map<List<InvoiceResponseModel>>(floors);

        return new PageList<InvoiceResponseModel>(floorResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }
    
    public async Task<InvoiceResponseModel> GetInvoice(Guid id)
    {
        var findInvoice = await _dbcontext.Invoices.Include(i => i.Booking)
            .ThenInclude(b => b.Customer)
            .Include(i => i.Booking)
            .ThenInclude(b => b.Room)
            .ThenInclude(r => r.RoomType).FirstOrDefaultAsync(i => i.Id == id);

        if (findInvoice == null)
        {
            throw new NotFoundException("Invoice not found");
        }
        
        return _mapper.Map<InvoiceResponseModel>(findInvoice);
    }
    
    public async Task<InvoiceResponseModel> GetInvoiceByBooking(Guid bookingId)
    {
        var invoice = await _dbcontext.Invoices
            .Include(i => i.Booking)
            .ThenInclude(b => b.Customer)
            .Include(i => i.Booking)
            .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(i => i.BookingId == bookingId);

        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found");
        }
        
        return _mapper.Map<InvoiceResponseModel>(invoice);
    }
    
    public async Task<string> CreateInvoice(AddInvoiceDto dto)
    {
        var booking = await _dbcontext.Bookings
            .Include(b => b.Room)
            .Include(b => b.BookingServices)
            .ThenInclude(bs => bs.Service)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);
        
        if (booking == null)
        {
            throw new NotFoundException("Booking not found");

        }
        
        var existingInvoice = await _dbcontext.Invoices
            .FirstOrDefaultAsync(i => i.BookingId == dto.BookingId);

        if (existingInvoice != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "bookingId",
                    Issue = "This booking already has an invoice. You cannot create another one."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        var totalService = booking.BookingServices?.Sum(s => s.Service.Price * s.Quantity) ?? 0;
        
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            Total = booking.TotalAmount + totalService,
            PaymentStatus = dto.PaymentStatus,
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
        };

        _dbcontext.Invoices.Add(invoice);
        await _dbcontext.SaveChangesAsync();

        return "Invoice has been created.";
    }

    public async Task<string> EditInvoice(Guid id, UpdateInvoiceDto dto)
    {
        var invoice = await _dbcontext.Invoices
            .Include(i => i.Booking)
            .ThenInclude(b => b.Customer)
            .Include(i => i.Booking)
            .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(i => i.Id == id);
        
        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found");

        }
        
        invoice.PaymentStatus = dto.PaymentStatus;
        invoice.PaymentMethod = dto.PaymentMethod;
        
        _dbcontext.Invoices.Update(invoice);
        await _dbcontext.SaveChangesAsync();
        
        return "Invoice has been updated.";
    }
    
    public async Task<string> UpdatePaymentStatus(Guid id, UpdatePaymentStatusDto dto)
    {
        var invoice = await _dbcontext.Invoices.FindAsync(id);
        
        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found");
        }
        
        invoice.PaymentStatus = dto.PaymentStatus;
        
        _dbcontext.Invoices.Update(invoice);
        await _dbcontext.SaveChangesAsync();
        
        return "Payment status has been updated.";
    }

    public async Task<string> DeleteInvoice(Guid id)
    {
        var invoice = await _dbcontext.Invoices.FindAsync(id);
        
        if (invoice == null)
        {
            throw new NotFoundException("Invoice not found");
        }
        
        if (invoice.PaymentStatus == PaymentStatus.Paid)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "paymentStatus",
                    Issue = "This invoice has already been paid and cannot be deleted.\n"
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        _dbcontext.Invoices.Remove(invoice);
        await _dbcontext.SaveChangesAsync();
        
        return "Invoice has been removed.";
    }
    
}