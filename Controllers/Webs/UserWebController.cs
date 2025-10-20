using System.Text.Json;
using HotelManagement.Extensions;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Controllers.Webs;

[Route("user")]
[Authorize(Roles = "Admin")]
public class UserWebController : BaseWebController
{
    public UserWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(httpClientFactory, configuration)
    {
    }
    
    #region ListUsers
    [HttpGet("list-user")]
    public async Task<IActionResult> ListUser()
    {
        ViewData["Title"] = "User List";

        try
        {
            SetAuthorizationHeader();

            var apiUrl = BaseApiUrl + "User/get-all-user";
            var response = await HttpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<PageList<UserResponseModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(users ?? new PageList<UserResponseModel>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Unable to load user list.");
                return View(new PageList<UserResponseModel>());
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return View(new PageList<UserResponseModel>());
        }
    }
    #endregion
    
    #region CreateUser - GET
    [HttpGet("add-user")]
    public async Task<IActionResult> AddUser()
    {
        ViewData["Title"] = "Add New USER";
        
        try
        {
            SetAuthorizationHeader();
            
            var rolesResponse = await HttpClient.GetAsync(BaseApiUrl + "Role/get-all-role");
            
            if (rolesResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            
            var roles = new List<RoleModel>();
            if (rolesResponse.IsSuccessStatusCode)
            {
                var content = await rolesResponse.Content.ReadAsStringAsync();
                roles = JsonSerializer.Deserialize<List<RoleModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RoleModel>();
            }

            ViewBag.Roles = roles?.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();
            
            var model = new AddUserDto
            {
                Username = TempData["Username"] as string,
                Password = TempData["Password"] as string,
            };
           
            return View(model);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return View();
        }
    }
    #endregion
    
    #region CreateUser - POST
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser(AddUserDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("AddUser");
        }
        
        try
        {
            SetAuthorizationHeader();
            
            var response = await HttpClient.PostAsJsonAsync(BaseApiUrl + "User/add-user", model);
            
            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("User created successfully!");
                return RedirectToAction("ListUser");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                SetErrorMessage(errorObj?.Errors[0].Issue);
                TempData["Username"] = model.Username;
                TempData["Password"] = model.Password;
                
                return RedirectToAction("AddUser");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("AddUser");
        }
    }
    #endregion
    
    #region UserDetail - GET
    [HttpGet("detail/{id:guid}")]
    public async Task<IActionResult> UserDetail(Guid id)
    {
        ViewData["Title"] = "User Details";

        try
        {
            SetAuthorizationHeader();
           
            var userResponse = await HttpClient.GetAsync($"{BaseApiUrl}User/user/{id}");

            if (userResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }

            if (!userResponse.IsSuccessStatusCode)
            {
                SetErrorMessage("User not found.");
                return RedirectToAction("ListUser");
            }

            var userContent = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseModel>(userContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (user == null)
            {
                SetErrorMessage("User not found.");
                return RedirectToAction("ListUser");
            }
            
            var rolesResponse = await HttpClient.GetAsync($"{BaseApiUrl}Role/get-all-role");
            if (rolesResponse.IsSuccessStatusCode)
            {
                var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<RoleModel>>(rolesContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RoleModel>();

                ViewBag.Roles = roles.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name,
                    Selected = r.Id == user.Role.Id
                }).ToList();
            }

            return View(user);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListUser");
        }
    }
    #endregion
    
    #region UpdateUser - POST
    [HttpPost("update/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(Guid id, EditUserDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("UserDetail", new { id });
        }

        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}User/edit-user/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("User updated successfully!");
                return RedirectToAction("UserDetail", new { id });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update user.");
                return RedirectToAction("UserDetail", new { id });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("UserDetail", new { id });
        }
    }
    #endregion
    
    #region ToggleUserStatus - POST
    [HttpPost("toggle-status/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.PatchAsync($"{BaseApiUrl}User/inactive-user/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("User status updated successfully!");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Failed to update user status.");
            }

            return RedirectToAction("UserDetail", new { id });
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("UserDetail", new { id });
        }
    }
    #endregion

    #region DeleteUser - POST
    [HttpPost("delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.DeleteAsync($"{BaseApiUrl}User/delete-user/{id}");

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("User deleted successfully!");
                return RedirectToAction("ListUser");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                SetErrorMessage($"Failed to delete user: {content}");
                return RedirectToAction("UserDetail", new { id });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("UserDetail", new { id });
        }
    }
    #endregion
}