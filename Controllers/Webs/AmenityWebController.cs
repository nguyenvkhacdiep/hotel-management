using System.Text.Json;
using HotelManagement.Extensions;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

[Route("amenity")]
[Authorize(Roles = "Admin")]

public class AmenityWebController:BaseWebController
{
    public AmenityWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(httpClientFactory, configuration)
    {
    }
    
    #region ListAmenity
    [HttpGet("list-amenity")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListAmenity()
    {
        ViewData["Title"] = "Amenity List";
        
        try
        {
            SetAuthorizationHeader();

            var apiUrl = BaseApiUrl + "Amenity/get-all-amenity";
            var response = await HttpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var amenities = JsonSerializer.Deserialize<PageList<AmenityResponseModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(amenities ?? new PageList<AmenityResponseModel>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Unable to load amenity list.");
                return View(new PageList<AmenityResponseModel>());
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return View(new PageList<AmenityResponseModel>());
        }
    }
    #endregion
    
    #region ToggleAmenityStatus - POST

    [HttpPost("toggle-status/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAmenityStatus(Guid id, string? returnTo)
    {
        try
        {
            SetAuthorizationHeader();

            var response =
                await HttpClient.PatchAsync($"{BaseApiUrl}Amenity/toggle-status/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("Amenity status updated successfully!");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Failed to update amenity status.");
            }
            if (returnTo == "detail")
                return RedirectToAction("AmenityDetail", new { id });
            else
                return RedirectToAction("ListAmenity");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListAmenity");
        }
    }
    #endregion
    
    #region Delete amenity - POST

    [HttpPost("delete-amenity/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAmenity(Guid id)
    {
        try
        {
            SetAuthorizationHeader();

            var response =
                await HttpClient.DeleteAsync($"{BaseApiUrl}Amenity/{id}");

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("Amenity status deleted successfully!");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                SetErrorMessage($"Failed to delete amenity: {content}");
            }
            return RedirectToAction("ListAmenity");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListAmenity");
        }
    }
    #endregion
    
    #region CreateAmenity - GET
    [HttpGet("create")]
    public IActionResult AddAmenity()
    {
        ViewData["Title"] = "Add New Amenity";
        return View();
    }
    #endregion
    
    #region CreateAmenity - POST
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAmenity(AddAmenityDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("CreateAmenity");
        }

        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.PostAsJsonAsync($"{BaseApiUrl}Amenity/add-new-amenity", model);

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage($"Amenity '{model.AmenityName}' created successfully!");
                return RedirectToAction("ListAmenity");
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

                SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to create amenity.");
                return RedirectToAction("k");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Create amenity exception: {ex.Message}");
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("CreateAmenity");
        }
    }
    #endregion
    
    #region AmenityDetail - GET
    [HttpGet("detail/{id:guid}")]
    public async Task<IActionResult> AmenityDetail(Guid id)
    {
        ViewData["Title"] = "Amenity Details";

        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.GetAsync($"{BaseApiUrl}Amenity/amenity/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }

            if (!response.IsSuccessStatusCode)
            {
                SetErrorMessage("Amenity not found.");
                return RedirectToAction("ListAmenity");
            }

            var content = await response.Content.ReadAsStringAsync();
            var amenity = JsonSerializer.Deserialize<AmenityResponseModel>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (amenity == null)
            {
                SetErrorMessage("Amenity not found.");
                return RedirectToAction("ListAmenity");
            }

            return View(amenity);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListAmenity");
        }
    }
    #endregion
    
    #region UpdateAmenity - POST
    [HttpPost("update/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAmenity(Guid id, AddAmenityDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("AmenityDetail", new { id });
        }

        try
        {
            SetAuthorizationHeader();

            var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}Amenity/edit-amenity/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                SetSuccessMessage("Amenity updated successfully!");
                return RedirectToAction("AmenityDetail", new { id });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                SetErrorMessage($"Failed to update amenity: {content}");
                return RedirectToAction("AmenityDetail", new { id });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("AmenityDetail", new { id });
        }
    }
    #endregion
    
}