using Microsoft.AspNetCore.Mvc;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;


namespace HotelManagement.Controllers;

[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] AddUserDto addUserDto)
    {
        var message = await _userService.AddUserAsync(addUserDto);
        return Ok(new { message });
    }
}