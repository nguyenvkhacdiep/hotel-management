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
                }).ToList();
                ViewBag.Floors = floors.Data?.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.FloorName
                }).ToList();

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
        
        #region RoomDetail - GET
        [HttpGet("room-detail/{id:guid}")]
        public async Task<IActionResult> RoomDetail(Guid id)
        {
            ViewData["Title"] = "Room Details";

            try
            {
                SetAuthorizationHeader();

                var response = await HttpClient.GetAsync($"{BaseApiUrl}Room/get-room/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }

                if (!response.IsSuccessStatusCode)
                {
                    SetErrorMessage("Room not found.");
                    return RedirectToAction("ListRoom");
                }

                var content = await response.Content.ReadAsStringAsync();
                var rooms = JsonSerializer.Deserialize<RoomDetailResponseModel>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rooms == null)
                {
                    SetErrorMessage("Room not found.");
                    return RedirectToAction("ListRoom");
                }

                return View(rooms);
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListRoom");
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
                    SearchKey = searchKey?.Trim(),
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
            if (TempData["CreateFloorData"] == null)
            {
                return View();
            }
            else
            {
                var json = TempData["CreateFloorData"] as string;
                var model = JsonSerializer.Deserialize<AddFloorDto>(json);
                return View(model);
            }
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
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("AddRoomPrice");
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
        
                    TempData["CreateFloorData"] = JsonSerializer.Serialize(model);;
                    TempData["Error"] = errorObj?.Errors[0].Issue;
                    return RedirectToAction("AddFloor");

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("AddFloor");

            }
        }
        #endregion
        
        #region DeleteFloor - POST
        [HttpPost("delete-floor/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFloor(Guid id)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await HttpClient.DeleteAsync($"{BaseApiUrl}Floor/{id}");

                if (response.IsSuccessStatusCode)
                {
                    SetSuccessMessage("Floor deleted successfully!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    SetErrorMessage(errorObj?.Message ??"Failed to delete floor.");
                }

                return RedirectToAction("ListFloor");
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListFloor");
            }
        }
        #endregion
        
        #region FloorDetails
        [HttpGet("floor-detail/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FloorDetail(Guid id)
        {
            ViewData["Title"] = "Floor Details";
            
            try
            {
                SetAuthorizationHeader(); ;
                
                var response = await HttpClient.GetAsync($"{BaseApiUrl}Floor/floor/{id}");


                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }

                if (!response.IsSuccessStatusCode)
                {
                    SetErrorMessage("Floor not found.");
                    return RedirectToAction("ListFloor");
                }

                var content = await response.Content.ReadAsStringAsync();
                var amenity = JsonSerializer.Deserialize<FloorResponseModel>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (amenity == null)
                {
                    SetErrorMessage("Floor not found.");
                    return RedirectToAction("ListFloor");
                }

                return View(amenity);
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListFloor");
            }
        }
        #endregion
        
        #region ToggleFloorStatus - POST

        [HttpPost("toggle-status/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFloorStatus(Guid id, string? returnTo)
        {
            try
            {
                SetAuthorizationHeader();

                var response =
                    await HttpClient.PatchAsync($"{BaseApiUrl}Floor/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    SetSuccessMessage("Floor status updated successfully!");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    SetErrorMessage("Failed to update floor status.");
                }
                
                if (returnTo == "detail")
                    return RedirectToAction("FloorDetail", new { id });
                else
                    return RedirectToAction("ListFloor");
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListFloor");
            }
        }
        #endregion
        
        #region EditFloor - POST
        [HttpPost("edit-floor/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFloor(Guid id, AddFloorDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("EditFloor", new { id });
            }

            try
            {
                SetAuthorizationHeader();

                Console.WriteLine($"[DEBUG] Updating floor: {id}");

                var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}Floor/edit-floor/{id}", model);

                if (response.IsSuccessStatusCode)
                {
                    SetSuccessMessage($"Floor '{model.FloorNumber}' updated successfully!");
                    return RedirectToAction("FloorDetail", new { id });
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

                    SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update floor.");
                    return RedirectToAction("FloorDetail", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Update floor exception: {ex.Message}");
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("FloorDetail", new { id });
            }
        }
        #endregion
        
        #region ListRoomType
        [HttpGet("list-room-type")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListRoomType( [FromQuery] string? searchKey)
        {
            ViewData["Title"] = "Room List Type";
            
            try
            {
                SetAuthorizationHeader(); ;
                
                var queryParams = new QueryParameters
                {
                    SearchKey = searchKey?.Trim(),
                    PageSize = 1000
                };
                
                var apiUrl = BaseApiUrl + $"RoomType/get-all-room-type?{queryParams.ToQueryString()}";
                var response = await HttpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roomTypes = JsonSerializer.Deserialize<PageList<RoomTypeResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ViewBag.SearchKey = searchKey;
                    ViewBag.QueryParams = queryParams;
                    ViewData["SearchPlaceholder"] = "Search by name";
                    
                    return View(roomTypes ?? new PageList<RoomTypeResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    SetErrorMessage("Unable to load amenity list.");
                    return View(new PageList<RoomTypeResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new PageList<RoomTypeResponseModel>());
            }
        }
        #endregion
        
        #region CreateRoomType - GET
        [HttpGet("add-room-type")]
        [Authorize(Roles = "Admin")]
        public IActionResult AddRoomType()
        {
            ViewData["Title"] = "Add New Room Type";
            
            if (TempData["Name"] == null &&
                TempData["Description"] == null &&
                TempData["MaxCapacity"] == null &&
                TempData["PricePerNight"] == null)
            {
                return View();
            }
            
            var model = new AddRoomTypeDto
            {
                Name = TempData["Name"] as string ?? string.Empty,
                Description = TempData["Description"] as string,
                MaxCapacity = TempData["MaxCapacity"] != null ? Convert.ToInt32(TempData["MaxCapacity"]) : 0,
                PricePerNight = TempData["PricePerNight"] != null ? Convert.ToDecimal(TempData["PricePerNight"]) : 0m
            };
            
            return View(model);
        }
        #endregion
        
        #region CreateRoomType - POST
        [HttpPost("add-room-type")]
        public async Task<IActionResult> AddRoomType(AddRoomTypeDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("AddRoomType");
            }
        
            try
            {
                SetAuthorizationHeader();
            
                var response = await HttpClient.PostAsJsonAsync(BaseApiUrl + "RoomType/add-new-room-type", model);
            
                if (response.IsSuccessStatusCode)
                {
                    SetSuccessMessage("User created successfully!");
                    return RedirectToAction("ListRoomType");
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
                    TempData["Name"] = model.Name;
                    TempData["Description"] = model.Description;
                    TempData["MaxCapacity"] = model.MaxCapacity.ToString();
                    TempData["PricePerNight"] = model.PricePerNight.ToString(System.Globalization.CultureInfo.InvariantCulture);
                
                    return RedirectToAction("AddRoomType");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("AddRoomType");
            }
        }
        #endregion
        
        #region RoomTypeDetails
        [HttpGet("room-type-detail/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RoomTypeDetail(Guid id)
        {
            ViewData["Title"] = "Room Type Details";
            
            try
            {
                SetAuthorizationHeader(); ;
                
                var response = await HttpClient.GetAsync($"{BaseApiUrl}RoomType/room-type/{id}");


                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }

                if (!response.IsSuccessStatusCode)
                {
                    SetErrorMessage("Room type not found.");
                    return RedirectToAction("ListRoomType");
                }

                var content = await response.Content.ReadAsStringAsync();
                var roomType = JsonSerializer.Deserialize<RoomTypeResponseModel>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (roomType == null)
                {
                    SetErrorMessage("Room type not found.");
                    return RedirectToAction("ListRoomType");
                }

                return View(roomType);
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListRoomType");
            }
        }
        #endregion
        
        #region DeleteRoomType - POST
        [HttpPost("delete-room-type/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomType(Guid id,string? returnTo)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await HttpClient.DeleteAsync($"{BaseApiUrl}RoomType/{id}");
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("ListRoomType");
                    
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    SetErrorMessage(errorObj?.Message ??"Failed to delete room type.");
                    if (returnTo == "detail")
                        return RedirectToAction("RoomTypeDetail", new { id  });
                    return RedirectToAction("ListRoomType");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                if (returnTo == "detail")
                    return RedirectToAction("RoomTypeDetail", new { id  });
                return RedirectToAction("ListRoomType");
            }
        }
        #endregion
        
        #region EditRoomType - POST
        [HttpPost("edit-room-type/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomType(Guid id, AddRoomTypeDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("RoomTypeDetail", new { id });
            }

            try
            {
                SetAuthorizationHeader();
                
                var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}RoomType/edit-room-type/{id}", model);

                if (response.IsSuccessStatusCode)
                {
                    SetSuccessMessage($"Type {model.Name} updated successfully!");
                    return RedirectToAction("RoomTypeDetail", new { id });
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

                    SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update room type.");
                    return RedirectToAction("RoomTypeDetail", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Update floor exception: {ex.Message}");
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("RoomTypeDetail", new { id });
            }
        }
        #endregion
        
        #region CreateRoomPrice - GET
        [HttpGet("add-room-price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoomPrice()
        {
            ViewData["Title"] = "Add New Room Price";
            
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

                ViewBag.RoomTypes = roomTypes.Data;
                
                if (TempData["AddRoomPriceData"] == null)
                {
                    return View();
                }

                var json = TempData["AddRoomPriceData"] as string;
                var model = JsonSerializer.Deserialize<AddRoomPriceDto>(json);
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }
        #endregion
        
        #region AddRoomPrice - POST
        [HttpPost("add-room-price")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoomPrice(AddRoomPriceDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("AddRoomPrice");
            }

            try
            {
                SetAuthorizationHeader();

                var apiUrl = BaseApiUrl + "RoomPrice";
                var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("ListRoomPrice");
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
        
                    TempData["AddRoomPriceData"] = JsonSerializer.Serialize(model);
                    TempData["Error"] = errorObj?.Errors[0].Issue;
                    return RedirectToAction("AddRoomPrice");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("AddRoomPrice");
            }
        }
        #endregion
        
        #region ListRoomPrice
        [HttpGet("list-room-price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListRoomPrice([FromQuery] RoomPriceRequestParameters parameters)
        {
            ViewData["Title"] = "Room List Type";
            
            try
            {
                SetAuthorizationHeader();
                
                var queryParams = new QueryParameters
                {
                    SearchKey = parameters?.SearchKey?.Trim(),
                    PageNumber = parameters?.PageNumber ?? 1,
                    PageSize = parameters?.PageSize ?? 10,
                };
                
                queryParams.AddFilter("RoomTypeId", parameters?.RoomTypeId.ToString());
                queryParams.AddFilter("SeasonName", parameters?.SeasonName.ToString());
                queryParams.AddFilter("DayType", parameters?.DayType.ToString());
                queryParams.AddFilter("IsActive", parameters?.IsActive.ToString());
                queryParams.AddFilter("Date", parameters?.Date.ToString());
                
                
                var responseRoomType = await HttpClient.GetAsync($"RoomType/get-all-room-type?PageSize=10000");
                var roomTypes = new PageList<RoomTypeResponseModel>();

                if (responseRoomType.IsSuccessStatusCode)
                {
                    var content = await responseRoomType.Content.ReadAsStringAsync();
                     roomTypes = JsonSerializer.Deserialize<PageList<RoomTypeResponseModel>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var apiUrl = BaseApiUrl + $"RoomPrice?{queryParams.ToQueryString()}";
                var response = await HttpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roomPrices = JsonSerializer.Deserialize<PageList<RoomPriceResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var filters = GetRoomPriceFilters(roomTypes);

                    ViewBag.Filters = filters;
                    ViewBag.SearchKey = parameters?.SearchKey?.Trim();
                    ViewBag.QueryParams = queryParams;
                    ViewData["SearchPlaceholder"] = "Search by name";
                    
                    return View(roomPrices ?? new PageList<RoomPriceResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    SetErrorMessage("Unable to load room price list.");
                    return View(new PageList<RoomPriceResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new PageList<RoomPriceResponseModel>());
            }
        }
        
        private List<FilterConfig> GetRoomPriceFilters(PageList<RoomTypeResponseModel> data)
        {
            return new List<FilterConfig>
            {
                new FilterConfig
                {
                    Name = "RoomTypeId",
                    Label = "Room Type",
                    Type = FilterType.Select,
                    Options = data.Data
                        ?.ToDictionary(rt => rt.Id.ToString(), rt => rt.Name) ?? new Dictionary<string, string>()
                },
                new FilterConfig
                {
                    Name = "SeasonName",
                    Label = "Season Name",
                    Type = FilterType.Select,
                    Options = new Dictionary<string, string>
                    {
                        { "0", "HighSeason" },
                        { "1", "LowSeason" },
                        { "2", "PeakSeason" },
                        { "3", "Holiday" }
                    }
                },
                new FilterConfig
                {
                    Name = "DayType",
                    Label = "Day Type",
                    Type = FilterType.Select,
                    Options = new Dictionary<string, string>
                    {
                        { "0", "Weekday" },
                        { "1", "Weekend" },
                        { "2", "Holiday" },
                        { "3", "All" }
                    }
                },
                new FilterConfig
                {
                    Name = "IsActive",
                    Label = "Active",
                    Type = FilterType.Select,
                    Options = new Dictionary<string, string>
                    {
                        { "true", "Active" },
                        { "false", "Inactive" },
                    }
                },
                new FilterConfig
                {
                    Name = "Date",
                    Label = "Date",
                    Type = FilterType.Date,
                },
            };
        }

        #endregion
        
        #region RoomPriceDetails
        [HttpGet("room-price-detail/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RoomPriceDetail(Guid id)
        {
            ViewData["Title"] = "Room Price Details";
            
            try
            {
                SetAuthorizationHeader(); ;
                
                var response = await HttpClient.GetAsync($"{BaseApiUrl}RoomPrice/{id}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }

                if (!response.IsSuccessStatusCode)
                {
                    SetErrorMessage("Room price not found.");
                    return RedirectToAction("ListRoomPrice");
                }

                var content = await response.Content.ReadAsStringAsync();
                var roomPrice = JsonSerializer.Deserialize<RoomPriceResponseModel>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (roomPrice == null)
                {
                    SetErrorMessage("Room price not found.");
                    return RedirectToAction("ListRoomPrice");
                }

                return View(roomPrice);
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ListRoomPrice");
            }
        }
        #endregion
        
        #region DeleteRoomPrice - POST
        [HttpPost("delete-room-price/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomPrice(Guid id,string? returnTo)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await HttpClient.DeleteAsync($"{BaseApiUrl}RoomPrice/{id}");
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("ListRoomPrice");
                    
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    SetErrorMessage(errorObj?.Message ??"Failed to delete room price.");
                    if (returnTo == "detail")
                        return RedirectToAction("RoomPriceDetail", new { id  });
                    return RedirectToAction("ListRoomPrice");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                if (returnTo == "detail")
                    return RedirectToAction("RoomPriceDetail", new { id  });
                return RedirectToAction("ListRoomPrice");
            }
        }
        #endregion
        
        #region ToggleRoomPriceStatus - POST

        [HttpPost("toggle-room-price-status/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRoomPriceStatus(Guid id, string? returnTo)
        {
            try
            {
                SetAuthorizationHeader();

                var response =
                    await HttpClient.PatchAsync($"{BaseApiUrl}RoomPrice/{id}/toggle", null);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    TempData["Error"] = errorObj?.Errors[0].Issue;
                }
                
                if (returnTo == "detail")
                    return RedirectToAction("RoomPriceDetail", new { id });
                else
                    return RedirectToAction("ListRoomPrice");
            }
            catch (Exception ex)
            {
                SetErrorMessage($"An error occurred: {ex.Message}");
                if (returnTo == "detail")
                    return RedirectToAction("RoomPriceDetail", new { id });
                else
                    return RedirectToAction("ListRoomPrice");
            }
        }
        #endregion
        
        #region EditRoomPrice - POST
        [HttpPost("edit-room-price/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomPrice(Guid id, UpdateRoomPriceDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("RoomPriceDetail", new { id });
            }

            try
            {
                SetAuthorizationHeader();
                
                var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}RoomPrice/{id}", model);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("RoomPriceDetail", new { id });
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

                    SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update room price.");
                    return RedirectToAction("RoomPriceDetail", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Update floor exception: {ex.Message}");
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("RoomPriceDetail", new { id });
            }
        }
        #endregion
    }
}
