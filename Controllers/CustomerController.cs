using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet("get-customer-by-phone")]
    public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone)
    {
        var customer = await _customerService.GetCustomerByPhone(phone);
        return Ok(customer);
    }
}