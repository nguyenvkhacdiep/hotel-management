using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class RoomTypeController: ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypeController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }
    
    [HttpPost("add-new-room-type")]
    public async Task<IActionResult> AddRoomTypeAsync([FromBody] AddRoomTypeDto roomType)
    {
        var message = await _roomTypeService.AddRoomTypeAsync(roomType);
        return Ok(new { message });
    }
    
    [HttpGet("get-all-room-type")]
    public async Task<IActionResult> GetAllAmenities([FromQuery] RequestParameters parameters)
    {
        var allRoomTypes = await _roomTypeService.GetAllRoomTypes(parameters);
        return Ok(allRoomTypes);
    }
    
    [HttpGet("room-type/{id:guid}")]
    public async Task<IActionResult> GetAmenityById(Guid id)
    {
        var roomType = await _roomTypeService.GetRoomTypeById(id);
        return Ok(roomType);
    }
    
    [HttpPut("edit-room-type/{id:guid}")]
    public async Task<IActionResult> EditAmenity(Guid id, [FromBody] AddRoomTypeDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _roomTypeService.EditRoomType(id, updateDto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _roomTypeService.DeleteRoomType(id);

        return Ok(new { message });
    }
}