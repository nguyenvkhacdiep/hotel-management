using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

public class DashboardController: Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}