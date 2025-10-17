using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;


[ApiController]
[Route("api/[controller]")]

public class BookingController: ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("add-new-booking")]
    public async Task<IActionResult> AddBookingAsync([FromBody] AddBookingDto bookingDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _bookingService.AddBookingAsync(bookingDto);
        return Ok(new { message });
    }

    [HttpGet("get-all-bookings")]
    public async Task<IActionResult> GetAllBookings([FromQuery] BookingRequestParameters parameters)
    {
        var allBookings = await _bookingService.GetAllBookings(parameters);
        return Ok(allBookings);
    }

    [HttpGet("booking/{id:guid}")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var booking = await _bookingService.GetBookingById(id);
        return Ok(booking);
    }

    [HttpPut("edit-booking/{id:guid}")]
    public async Task<IActionResult> EditBooking(Guid id, [FromBody] UpdateBookingDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _bookingService.EditBooking(id, updateDto);
        return Ok(new { message });
    }

    [HttpDelete("delete-booking/{id:guid}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var message = await _bookingService.DeleteBooking(id);
        return Ok(new { message });
    }

    [HttpPost("cancel-booking/{id:guid}")]
    public async Task<IActionResult> CancelBookingAsync(Guid id)
    {
        var message = await _bookingService.CancelBookingAsync(id);
        return Ok(new { message });
    }

    [HttpPost("confirm-booking/{id:guid}")]
    public async Task<IActionResult> ConfirmBookingAsync(Guid id)
    {
        var message = await _bookingService.ConfirmBookingAsync(id);
        return Ok(new { message });
    }

    [HttpPost("checkin-booking/{id:guid}")]
    public async Task<IActionResult> CheckInBookingAsync(Guid id)
    {
        var message = await _bookingService.CheckInBookingAsync(id);
        return Ok(new { message });
    }

    [HttpPost("checkout-booking/{id:guid}")]
    public async Task<IActionResult> CheckOutBookingAsync(Guid id)
    {
        var message = await _bookingService.CheckOutBookingAsync(id);
        return Ok(new { message });
    }

    [HttpGet("check-room-availability/{roomId:guid}")]
    public async Task<IActionResult> IsRoomAvailableAsync(Guid roomId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut, [FromQuery] Guid? excludeBookingId = null)
    {
        if (checkIn >= checkOut)
        {
            return BadRequest(new { message = "Check-out date must be after check-in date." });
        }

        var isAvailable = await _bookingService.IsRoomAvailableAsync(roomId, checkIn, checkOut, excludeBookingId);
        return Ok(new { isAvailable, roomId, checkIn, checkOut });
    }

    [HttpGet("calculate-total-amount/{roomId:guid}")]
    public async Task<IActionResult> CalculateTotalAmountAsync(Guid roomId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
    {
        if (checkIn >= checkOut)
        {
            return BadRequest(new { message = "Check-out date must be after check-in date." });
        }

        var totalAmount = await _bookingService.CalculateTotalAmountAsync(roomId, checkIn, checkOut);
        return Ok(new { totalAmount, roomId, checkIn, checkOut, nights = (checkOut - checkIn).Days });
    }
}