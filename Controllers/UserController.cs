using HotelManagement.Services.Common;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;


namespace HotelManagement.Controllers;

[ApiController]
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

    [HttpGet("get-all-user")]
    public async Task<IActionResult> GetAllUsers([FromQuery] RequestParameters parameters)
    {
        var users = await _userService.GetAllUsers(parameters);
        return Ok(users);
    }
    
    [HttpGet("user/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserById(id);
        return Ok(user);
    }
    
    [HttpPut("edit-user/{id:guid}")]
    public async Task<IActionResult> EditUser(Guid id, [FromBody] EditUserDto userUpdateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _userService.EditUserAsync(id, userUpdateDto);
        return Ok(new { message });
    }
    
    [HttpPatch("inactive-user/{id:guid}")]
    public async Task<IActionResult> InactiveUser(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _userService.InactiveUserAsync(id);
        return Ok(new { message });
    }
}