using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RequestParameters parameters)
    {
        var invoices = await _invoiceService.GetAllInvoices(parameters);
        return Ok(invoices);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var invoice = await _invoiceService.GetInvoice(id);
        return Ok(invoice);
    }
    
    [HttpGet("by-booking/{bookingId}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var invoice = await _invoiceService.GetInvoiceByBooking(bookingId);
        return Ok(invoice);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddInvoiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var message = await _invoiceService.CreateInvoice(dto);
        return Ok(new { message });
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] UpdateInvoiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var message = await _invoiceService.EditInvoice(id, dto);
        return Ok(new { message });
    }
    
    [HttpPatch("{id:guid}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var message = await _invoiceService.UpdatePaymentStatus(id, dto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _invoiceService.DeleteInvoice(id);
        return Ok(new { message });
    }
}