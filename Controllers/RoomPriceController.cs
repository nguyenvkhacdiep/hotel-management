using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class RoomPriceController: ControllerBase
{
    private readonly IRoomPricesService _roomPricesService;

    public RoomPriceController(IRoomPricesService roomPricesService)
    {
        _roomPricesService = roomPricesService;
    }

    [HttpPost]
    public async Task<ActionResult> AddRoomPrice([FromBody] AddRoomPriceDto addDto)
    {
        var message = await _roomPricesService.AddRoomPrices(addDto);
        return Ok(new {  message });
    }

    [HttpGet]
    public async Task<ActionResult> GetAllRoomPrices(
        [FromQuery] RoomPriceRequestParameters parameters)
    {
        var result = await _roomPricesService.GetAllRoomPrices(parameters);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> EditRoomPrice(
        Guid id,
        [FromBody] UpdateRoomPriceDto updateDto)
    {
        var message = await _roomPricesService.EditRoomPrice(id, updateDto);
        return Ok(new {message});
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRoomPrice(Guid id)
    {
        var message = await _roomPricesService.DeleteRoomPrice(id);
        return Ok(new {message});
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult> ToggleRoomPriceStatus(Guid id)
    {
        var message = await _roomPricesService.ToggleRoomPriceStatus(id);
        return Ok(new {message});
    }

    [HttpGet("room/{roomId:guid}/current")]
    public async Task<ActionResult> GetCurrentRoomPrice(
        Guid roomId,
        [FromQuery] DateTime? date = null)
    {
        var response = await _roomPricesService.GetCurrentRoomPrice(roomId, date);
        return Ok(response);
    }

    [HttpGet("room/{roomId:guid}/calendar")]
    public async Task<ActionResult<List<PriceCalendarDto>>> GetRoomPriceCalendar(
        Guid roomId,
        PriceCalendarRequestParameters parameters)
    {
        var response = await _roomPricesService.GetRoomPriceCalendar(roomId, parameters);
        return Ok(response);
    }
}