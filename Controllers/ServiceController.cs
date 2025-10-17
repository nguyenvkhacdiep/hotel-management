using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceController: ControllerBase
{
    private readonly IServiceService  _serviceService;
    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }
    
    [HttpPost("add-new-service")]
    public async Task<IActionResult> AddServiceAsync([FromBody] AddServiceDto service)
    {
        var message = await _serviceService.AddServiceAsync(service);
        return Ok(new { message });
    }
    
    [HttpGet("get-all-service")]
    public async Task<IActionResult> GetAllAmenities([FromQuery] RequestParameters parameters)
    {
        var allServices = await _serviceService.GetAllServices(parameters);
        return Ok(allServices);
    }
    
    [HttpGet("service/{id:guid}")]
    public async Task<IActionResult> GetServiceById(Guid id)
    {
        var service = await _serviceService.GetServiceById(id);
        return Ok(service);
    }
    
    [HttpPut("edit-service/{id:guid}")]
    public async Task<IActionResult> EditService(Guid id, [FromBody] AddServiceDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _serviceService.EditService(id, updateDto);
        return Ok(new { message });
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var message = await _serviceService.DeleteService(id);

        return Ok(new { message });
    }
}