using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

[Authorize]
public abstract class BaseWebController : Controller
{
    protected readonly HttpClient HttpClient;
    protected readonly IConfiguration Configuration;
    protected readonly string BaseApiUrl;

    protected BaseWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        HttpClient = httpClientFactory.CreateClient("HotelAPI");
        Configuration = configuration;
        BaseApiUrl = configuration["ApiSettings:BaseUrl"] ?? string.Empty;
    }
    
    protected void SetAuthorizationHeader()
    {
        var token = HttpContext.Request.Cookies["AuthToken"];
        if (!string.IsNullOrEmpty(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    protected string? GetAuthToken()
    {
        return HttpContext.Request.Cookies["AuthToken"];
    }
    
    protected IActionResult HandleUnauthorized(string message = "Your session has expired. Please login again.")
    {
        TempData["ErrorMessage"] = message;
        return RedirectToAction("Login", "AuthWeb");
    }
    
    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }
    
    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }
}