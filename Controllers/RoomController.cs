using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class RoomController: ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }
    
    [HttpPost("add-room")]
    public async Task<IActionResult> AddRoomAsync([FromBody] AddRoomDto addRoomDto)
    {
        var message = await _roomService.AddRoomAsync(addRoomDto);
        return Ok(new {message});
    }

    [HttpGet("get-all-rooms")]
    public async Task<IActionResult> GetAllRooms(
        [FromQuery] RoomRequestParameters roomRequestParameters)
    {
        var result = await _roomService.GetAllRooms(roomRequestParameters);
        return Ok(result);
    }

    
    [HttpGet("get-room/{id}")]
    public async Task<IActionResult> GetRoom(Guid id)
    {
        var result = await _roomService.GetRoomById(id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<string>> EditRoom(
        Guid id,
        [FromBody] UpdateRoomDto updateDto)
    {
        var message = await _roomService.UpdateRoom(id, updateDto);
        return Ok(new {message});
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<string>> DeleteRoom(Guid id)
    {
        var message = await _roomService.DeleteRoom(id);
        return Ok(new {message});
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> ChangeRoomStatus(
        Guid id,
        [FromBody] ChangeRoomStatusDto statusDto)
    {
        var message = await _roomService.ChangeRoomStatus(id, statusDto);
        return Ok(new {message});
    }

    [HttpPut("{id}/amenities")]
    public async Task<ActionResult> UpdateRoomAmenities(
        Guid id,
        [FromBody] UpdateRoomAmenitiesDto amenitiesDto)
    {
        var message = await _roomService.UpdateRoomAmenities(id, amenitiesDto);
        return Ok(new {message});
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RoomResponseModel>>> GetAvailableRooms(
        [FromQuery] AvailabelRoomParameters parameters)
    {
        var result = await _roomService.GetAvailableRooms(parameters);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<RoomStatisticsResponse>> GetRoomStatistics()
    {
        var result = await _roomService.GetRoomStatistics();
        return Ok(result);
    }

    [HttpGet("{id}/status-history")]
    public async Task<ActionResult<List<RoomStatusHistoryDto>>> GetRoomStatusHistory(Guid id)
    {
        var result = await _roomService.GetRoomStatusHistory(id);
        return Ok(result);
    }
}