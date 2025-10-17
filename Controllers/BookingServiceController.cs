using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BookingServiceController : ControllerBase
{
    private readonly IBookingServiceService _bookingServiceService;

    public BookingServiceController(IBookingServiceService bookingService)
    {
        _bookingServiceService = bookingService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBookingService([FromBody] AddBookingServiceDto dto)
    {
        var message = await _bookingServiceService.AddBookingServiceAsync(dto);
        return Ok(new { message });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllBookingServices([FromQuery] BookingServiceRequestParameters parameters)
    {
        var result = await _bookingServiceService.GetAllBookingServices(parameters);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBookingServiceById(Guid id)
    {
        var result = await _bookingServiceService.GetBookingServiceById(id);
        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> EditBookingService(Guid id, [FromBody] UpdateBookingServiceDto dto)
    {
        var message = await _bookingServiceService.EditBookingService(id, dto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBookingService(Guid id)
    {
        var message = await _bookingServiceService.DeleteBookingService(id);
        return Ok(new { message });
    }
}