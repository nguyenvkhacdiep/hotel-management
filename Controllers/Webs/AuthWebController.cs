using System.Security.Claims;
using System.Text.Json;
using HotelManagement.Extensions;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

// [Route("[controller]/[action]")]
public class AuthWebController: Controller
{
    private readonly HttpClient _httpClient;

    public AuthWebController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("HotelAPI");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("Login");
        }

        var response = await _httpClient.PostAsJsonAsync("auth/login", model);
        var result = await response.Content.ReadAsStringAsync();
            
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = JsonSerializer.Deserialize<UserLoginResponseModel>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Response.Cookies.Append("AuthToken", loginResponse.Token, new CookieOptions
            {
                HttpOnly = true, 
                Secure = true,   
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            Response.Cookies.Append("User", JsonSerializer.Serialize(loginResponse.User), new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SameSite = SameSiteMode.None
            });
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                new Claim(ClaimTypes.Name, loginResponse.User.Username),
                new Claim(ClaimTypes.Role, loginResponse.User.Role.Name),
                new Claim("Token", loginResponse.Token)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, 
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            
            return RedirectToAction("Index", "Dashboard");
        }
        
        var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Console.WriteLine(result);
        
        TempData["Error"] = errorObj?.Errors[0].Issue;
        
        TempData["Username"] = model.Username;
        
        return RedirectToAction("Login"); 
    }

    [HttpGet]
    public IActionResult Login()
    {
        TempData.Remove("SuccessMessage");
        ModelState.Clear();
        
        var headers = Response.GetTypedHeaders();
        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "-1";
        
        var model = new UserLoginDto
        {
            Username = TempData["Username"] as string
        };
        
        return View(model);
    }
}