using HotelManagement.Models;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AmenityController: ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenityController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpPost("add-new-amenity")]
    public async Task<IActionResult> AddNewAmenity([FromBody] AddAmenityDto amenity)
    {
        var message = await _amenityService.AddAmenityAsync(amenity);
        return Ok(new { message });
    }
    
    [HttpGet("get-all-amenity")]
    public async Task<IActionResult> GetAllAmenities([FromQuery] RequestParameters parameters)
    {
        var allAmenities = await _amenityService.GetAllAmenities(parameters);
        return Ok(allAmenities);
    }
    
    [HttpGet("amenity/{id:guid}")]
    public async Task<IActionResult> GetAmenityById(Guid id)
    {
        var amenity = await _amenityService.GetAmenityById(id);
        return Ok(amenity);
    }
    
    [HttpPut("edit-amenity/{id:guid}")]
    public async Task<IActionResult> EditAmenity(Guid id, [FromBody] AddAmenityDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _amenityService.EditAmenity(id, updateDto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _amenityService.DeleteAmenity(id);

        return Ok(new { message });
    }
}