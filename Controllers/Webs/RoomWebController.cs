using System.Text.Json;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http.Headers;
using HotelManagement.Extensions;
using HotelManagement.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Controllers.Webs
{
    [Route("room")]
    public class RoomWebController : BaseWebController
    {

        public RoomWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        #region ListRoom
        [HttpGet("list-room")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListRoom()
        {
            ViewData["Title"] = "Room List";
            
            try
            {
                SetAuthorizationHeader(); ;
                
                var apiUrl = BaseApiUrl + $"Room/get-all-rooms";
                var response = await HttpClient.GetAsync(apiUrl);

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
                SetAuthorizationHeader();
                
                var roomTypeResponse = await HttpClient.GetAsync(BaseApiUrl + "RoomType/get-all-room-type");
                var roomTypes = new PageList<RoomTypeResponseModel>();
                if (roomTypeResponse.IsSuccessStatusCode)
                {
                    var content = await roomTypeResponse.Content.ReadAsStringAsync();
                    roomTypes = JsonSerializer.Deserialize<PageList<RoomTypeResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PageList<RoomTypeResponseModel>();
                }
                
                var amenityResponse = await HttpClient.GetAsync(BaseApiUrl + "Amenity/get-all-amenity");
                var amenities = new PageList<AmenityResponseModel>();
                if (amenityResponse.IsSuccessStatusCode)
                {
                    var content = await amenityResponse.Content.ReadAsStringAsync();
                    amenities = JsonSerializer.Deserialize<PageList<AmenityResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PageList<AmenityResponseModel>();
                }
                
                var floorsResponse = await HttpClient.GetAsync(BaseApiUrl + "Floor/get-all-floor");
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
                SetAuthorizationHeader();

                var response = await HttpClient.PostAsJsonAsync(BaseApiUrl + "Room/add-room", model);
                
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
        
        #region ListFloor
        [HttpGet("list-floor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListFloor(
            [FromQuery] string? searchKey,
            [FromQuery] string? orderBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            ViewData["Title"] = "Floor List";

            try
            {
                var queryParams = new QueryParameters
                {
                    SearchKey = searchKey,
                    OrderBy = orderBy,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                
                SetAuthorizationHeader();
                var apiUrl = BaseApiUrl + $"Floor/get-all-floor?{queryParams.ToQueryString()}";
                var response = await HttpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var floors = JsonSerializer.Deserialize<PageList<FloorResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    ViewBag.SearchKey = searchKey;
                    ViewBag.QueryParams = queryParams;
                    ViewData["SearchPlaceholder"] = "Search by floor number";
                    return View(floors ?? new PageList<FloorResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please login again.";
                    return RedirectToAction("Login", "AuthWeb");
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to load floor list.";
                    return View(new PageList<FloorResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new PageList<FloorResponseModel>());
            }
        }
        #endregion
        
        #region CreateFloor - GET
        [HttpGet("create-floor")]
        [Authorize(Roles = "Admin")]
        public IActionResult AddFloor()
        {
            ViewData["Title"] = "Add New Floor";
            return View();
        }
        #endregion
        
        #region CreateFloor - POST
        [HttpPost("create-floor")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFloor(AddFloorDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                SetAuthorizationHeader();

                var apiUrl = BaseApiUrl + "Floor/add-new-floor";
                var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
                var result = await response.Content.ReadAsStringAsync();
                

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Floor added successfully!";
                    return RedirectToAction("ListFloor");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Your session has expired. Please login again.";
                    return RedirectToAction("Login", "AuthWeb");
                }
                else
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
                    TempData["Error"] = errorObj?.Errors[0].Issue;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }
        #endregion
    }
}
