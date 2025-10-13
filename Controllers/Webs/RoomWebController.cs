using System.Text.Json;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Controllers.Webs
{
    [Route("room")]
    public class RoomWebController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RoomWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("HotelAPI");
            _configuration = configuration;
        }

        #region ListRoom
        [HttpGet("list-room")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListRoom()
        {
            ViewData["Title"] = "Room List";

            try
            {
                var token = Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "Room/get-all-rooms";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<PageList<RoomResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(rooms ?? new PageList<RoomResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please login again.";
                    return RedirectToAction("Login", "AuthWeb");
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to load room list.";
                    return View(new PageList<RoomResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new PageList<RoomResponseModel>());
            }
        }
        #endregion

        #region CreateRoom - GET
        [HttpGet("create-room")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoom()
        {
            ViewData["Title"] = "Add New Room";

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                var roomTypeResponse = await _httpClient.GetAsync(baseUrl + "RoomType/get-all-room-type");
                var roomTypes = new PageList<RoomTypeResponseModel>();
                if (roomTypeResponse.IsSuccessStatusCode)
                {
                    var content = await roomTypeResponse.Content.ReadAsStringAsync();
                    roomTypes = JsonSerializer.Deserialize<PageList<RoomTypeResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PageList<RoomTypeResponseModel>();
                }
                
                var amenityResponse = await _httpClient.GetAsync(baseUrl + "Amenity/get-all-amenity");
                var amenities = new PageList<AmenityResponseModel>();
                if (amenityResponse.IsSuccessStatusCode)
                {
                    var content = await amenityResponse.Content.ReadAsStringAsync();
                    amenities = JsonSerializer.Deserialize<PageList<AmenityResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PageList<AmenityResponseModel>();
                }
                
                var floorsResponse = await _httpClient.GetAsync(baseUrl + "Floor/get-all-floor");
                var floors = new PageList<FloorResponseModel>();
                if (floorsResponse.IsSuccessStatusCode)
                {
                    var content = await floorsResponse.Content.ReadAsStringAsync();
                    floors = JsonSerializer.Deserialize<PageList<FloorResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PageList<FloorResponseModel>();
                }


                ViewBag.RoomTypes = roomTypes.Data?.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                }).ToList();;
                ViewBag.Floors = floors.Data?.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.FloorName
                }).ToList();;

                ViewBag.Amenities = amenities.Data;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }
        #endregion

        #region CreateRoom - POST
        [HttpPost("create-room")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(AddRoomDto model)
        {
            ViewData["Title"] = "Add New Room";

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data.";
                return await CreateRoom();
            }

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var jsonContent = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(baseUrl + "Room/add-room", content);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Room created successfully!";
                    return RedirectToAction("ListRoom");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create room.";
                    return await CreateRoom();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return await CreateRoom();
            }
        }
        #endregion
    }
}
