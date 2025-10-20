using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;

namespace HotelManagement.Services.Interfaces;

public interface IInvoiceService
{
    Task<string> CreateInvoice(AddInvoiceDto addFloorDto);
    Task<string> EditInvoice(Guid id, UpdateInvoiceDto dto);
    Task<string> UpdatePaymentStatus(Guid id, UpdatePaymentStatusDto dto);
    Task<string> DeleteInvoice(Guid id);
    Task<PageList<InvoiceResponseModel>> GetAllInvoices(RequestParameters parameters);
    Task<InvoiceResponseModel> GetInvoice(Guid id);
    Task<InvoiceResponseModel> GetInvoiceByBooking(Guid bookingId);
}