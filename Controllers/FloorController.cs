using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class FloorController: ControllerBase
{
    private readonly IFloorService _floorService;

    public FloorController(IFloorService floorService)
    {
        _floorService = floorService;
    }
    
    [HttpPost("add-new-floor")]
    public async Task<IActionResult> AddFloorAsync([FromBody] AddFloorDto floor)
    {
        var message = await _floorService.AddFloorAsync(floor);
        return Ok(new { message });
    }
    
    [HttpGet("get-all-floor")]
    public async Task<IActionResult> GetAllAmenities([FromQuery] RequestParameters parameters)
    {
        var allFloors = await _floorService.GetAllFloors(parameters);
        return Ok(allFloors);
    }
    
    [HttpGet("floor/{id:guid}")]
    public async Task<IActionResult> GetAmenityById(Guid id)
    {
        var floor = await _floorService.GetFloorById(id);
        return Ok(floor);
    }
    
    [HttpPut("edit-floor/{id:guid}")]
    public async Task<IActionResult> EditAmenity(Guid id, [FromBody] AddFloorDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _floorService.EditFloor(id, updateDto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _floorService.DeleteFloor(id);

        return Ok(new { message });
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> ToggleActiveFloorAsync(Guid id)
    {
        var message = await _floorService.ToggleActiveFloorAsync(id);

        return Ok(new { message });
    }
}