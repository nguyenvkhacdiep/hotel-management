using System.Text.Json;
using HotelManagement.Extensions;
using HotelManagement.Models.Common;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

[Route("service")]
[Authorize(Roles = "Admin")]

public class ServiceWebController : BaseWebController
{
    public ServiceWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(httpClientFactory, configuration)
    {
    }
    
    #region CreateService - GET
    [HttpGet("add-service")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddService()
    {
        ViewData["Title"] = "Add New Service";

        try
        {
            SetAuthorizationHeader();

            if (TempData["AddServiceData"] == null)
            {
                return View();
            }

            var json = TempData["AddServiceData"] as string;
            var model = JsonSerializer.Deserialize<AddServiceDto>(json);
            
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View();
        }
    }
    #endregion
        
    #region AddService - POST
    [HttpPost("add-service")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddService(AddServiceDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("AddService");
        }

        try
        {
            SetAuthorizationHeader();

            var apiUrl = BaseApiUrl + "Service/add-new-service";
            var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListService");
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
    
                TempData["AddServiceData"] = JsonSerializer.Serialize(model);
                SetErrorMessage(errorObj?.Errors[0].Issue);
                return RedirectToAction("AddService");
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("AddService");
        }
    }
    #endregion
    
    #region ListService
    [HttpGet("list-service")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListService([FromQuery] RequestParameters parameters)
    {
        ViewData["Title"] = "Service List";
        
        try
        {
            SetAuthorizationHeader();
            
            var queryParams = new QueryParameters
            {
                SearchKey = parameters?.SearchKey?.Trim(),
                PageNumber = parameters?.PageNumber ?? 1,
                PageSize = parameters?.PageSize ?? 10,
            };

            var apiUrl = BaseApiUrl + $"Service/get-all-service?{queryParams.ToQueryString()}";
            var response = await HttpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<PageList<ServiceResponseModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                ViewBag.QueryParams = queryParams;
                ViewData["SearchPlaceholder"] = "Search by name";
                
                
                return View(services ?? new PageList<ServiceResponseModel>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Unable to load room price list.");
                return View(new PageList<ServiceResponseModel>());
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View(new PageList<ServiceResponseModel>());
        }
    }
    #endregion
    
    #region ServiceDetails
    [HttpGet("service-detail/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ServiceDetail(Guid id)
    {
        ViewData["Title"] = "Service Details";
            
        try
        {
            SetAuthorizationHeader(); ;
                
            var response = await HttpClient.GetAsync($"{BaseApiUrl}Service/service/{id}");
                
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }

            if (!response.IsSuccessStatusCode)
            {
                SetErrorMessage("Service not found.");
                return RedirectToAction("ListService");
            }

            var content = await response.Content.ReadAsStringAsync();
            var service = JsonSerializer.Deserialize<ServiceResponseModel>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (service == null)
            {
                SetErrorMessage("Service not found.");
                return RedirectToAction("ListService");
            }

            return View(service);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListService");
        }
    }
    #endregion
    
    #region DeleteService - POST
    [HttpPost("delete-service/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(Guid id,string? returnTo)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.DeleteAsync($"{BaseApiUrl}Service/{id}");
            var result = await response.Content.ReadAsStringAsync();
                
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListService");
                    
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to delete service.");
                if (returnTo == "detail")
                    return RedirectToAction("ServiceDetail", new { id  });
                return RedirectToAction("ListService");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            if (returnTo == "detail")
                return RedirectToAction("ServiceDetail", new { id  });
            return RedirectToAction("ListService");
        }
    }
    #endregion
    
    #region EditService - POST
        [HttpPost("edit-service/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(Guid id, AddServiceDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("ServiceDetail", new { id });
            }

            try
            {
                SetAuthorizationHeader();
                
                var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}Service/edit-service/{id}", model);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("ServiceDetail", new { id });
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

                    SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update service.");
                    return RedirectToAction("ServiceDetail", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Update floor exception: {ex.Message}");
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("ServiceDetail", new { id });
            }
        }
        #endregion
        
    #region AddBookingService - GET
    [HttpGet("order-service")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBookingService(Guid? bookingId)
    {
        ViewData["Title"] = "Order Service";

        try
        {
            SetAuthorizationHeader();
            
            var apiUrlService = BaseApiUrl + $"Service/get-all-service?pageSize=1000";
            var serviceResponse = await HttpClient.GetAsync(apiUrlService);
            var services = new PageList<ServiceResponseModel>();
            
            if (serviceResponse.IsSuccessStatusCode)
            {
                var result = await serviceResponse.Content.ReadAsStringAsync();
                
                services = JsonSerializer.Deserialize<PageList<ServiceResponseModel>>(
                    result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
          
            ViewBag.Services = services;
            ViewBag.BookingId = bookingId.ToString();
            
            if (TempData["AddServiceData"] == null)
            {
                return View();
            }

            var json = TempData["AddServiceData"] as string;
            var model = JsonSerializer.Deserialize<AddServiceDto>(json);
           
            
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View();
        }
    }
    #endregion
    
    #region AddOrderService - POST
    [HttpPost("add-order-service")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrderService(AddBookingServiceDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("AddBookingService");
        }

        try
        {
            SetAuthorizationHeader();

            var apiUrl = BaseApiUrl + "BookingService";
            var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListBookingService");
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
    
                
                SetErrorMessage(errorObj?.Errors[0].Issue);
                return RedirectToAction("AddBookingService");
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("AddBookingService");
        }
    }
    #endregion
    
    #region ListBookingService
    [HttpGet("list-booking-service")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListBookingService([FromQuery] BookingServiceRequestParameters parameters)
    {
        ViewData["Title"] = "Service List";
        
        try
        {
            SetAuthorizationHeader();
            
            var responseRoom = await HttpClient.GetAsync($"Booking/get-all-bookings?PageSize=10000");
            var rooms = new PageList<RoomResponseModel>();

            if (responseRoom.IsSuccessStatusCode)
            {
                var content = await responseRoom.Content.ReadAsStringAsync();
                rooms = JsonSerializer.Deserialize<PageList<RoomResponseModel>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            var responseService = await HttpClient.GetAsync($"Service/get-all-service?PageSize=10000");
            var servicesFilter = new PageList<ServiceResponseModel>();

            if (responseService.IsSuccessStatusCode)
            {
                var content = await responseService.Content.ReadAsStringAsync();
                servicesFilter = JsonSerializer.Deserialize<PageList<ServiceResponseModel>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            var queryParams = new QueryParameters
            {
                SearchKey = parameters?.SearchKey?.Trim(),
                PageNumber = parameters?.PageNumber ?? 1,
                PageSize = parameters?.PageSize ?? 10,
            };
            
            queryParams.AddFilter("BookingId", parameters?.BookingId.ToString());
            queryParams.AddFilter("ServiceId", parameters?.ServiceId.ToString());

            var apiUrl = BaseApiUrl + $"BookingService?{queryParams.ToQueryString()}";
            var response = await HttpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<PageList<BookingServiceResponseModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var filters = GetBookingServiceFilters(rooms,  servicesFilter);

                ViewBag.Filters = filters;
                
                ViewBag.QueryParams = queryParams;
                ViewData["SearchPlaceholder"] = "Search by name";
                ViewBag.QueryParams = queryParams;
                
                return View(services ?? new PageList<BookingServiceResponseModel>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                SetErrorMessage("Unable to load.");
                return View(new PageList<BookingServiceResponseModel>());
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View(new PageList<BookingServiceResponseModel>());
        }
    }
    
    private List<FilterConfig> GetBookingServiceFilters(PageList<RoomResponseModel> rooms,PageList<ServiceResponseModel> services)
        {
            return new List<FilterConfig>
            {
                new FilterConfig
                {
                    Name = "BookingId",
                    Label = "Booking",
                    Type = FilterType.Select,
                    Options = rooms.Data
                        ?.ToDictionary(rt => rt.Id.ToString(), rt => rt.RoomNumber) ?? new Dictionary<string, string>()
                },
                new FilterConfig
                {
                    Name = "ServiceId",
                    Label = "Service",
                    Type = FilterType.Select,
                    Options = services.Data
                        ?.ToDictionary(rt => rt.Id.ToString(), rt => rt.Name) ?? new Dictionary<string, string>()
                },
            };
        }
    #endregion
    
    #region DeleteBookingService - POST
    [HttpPost("delete-booking-service/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBookingService(Guid id,string? returnTo)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.DeleteAsync($"{BaseApiUrl}BookingService/{id}");
            var result = await response.Content.ReadAsStringAsync();
                
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListBookingService");
                    
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to delete service.");
                if (returnTo == "detail")
                    return RedirectToAction("ServiceDetail", new { id  });
                return RedirectToAction("ListBookingService");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            if (returnTo == "detail")
                return RedirectToAction("ServiceDetail", new { id  });
            return RedirectToAction("ListBookingService");
        }
    }
    #endregion
    
    #region BookingServiceDetails
    [HttpGet("booking-service-detail/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BookingServiceDetail(Guid id)
    {
        ViewData["Title"] = "Booking Service Details";
            
        try
        {
            SetAuthorizationHeader(); ;
                
            var response = await HttpClient.GetAsync($"{BaseApiUrl}BookingService/{id}");
                
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }

            if (!response.IsSuccessStatusCode)
            {
                SetErrorMessage("BookingService not found.");
                return RedirectToAction("ListBookingService");
            }

            var content = await response.Content.ReadAsStringAsync();
            var service = JsonSerializer.Deserialize<BookingServiceResponseModel>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (service == null)
            {
                SetErrorMessage("BookingService not found.");
                return RedirectToAction("ListBookingService");
            }

            return View(service);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListService");
        }
    }
    #endregion
    
    #region EditBookingService - POST
        [HttpPost("edit-booking-service/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBookingService(Guid id, UpdateBookingServiceDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
                return RedirectToAction("BookingServiceDetail", new { id });
            }

            try
            {
                SetAuthorizationHeader();
                
                var response = await HttpClient.PutAsJsonAsync($"{BaseApiUrl}BookingService/{id}", model);
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                if (response.IsSuccessStatusCode)
                {
                    var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    SetSuccessMessage(message.Message);
                    return RedirectToAction("BookingServiceDetail", new { id });
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

                    SetErrorMessage(errorObj?.Errors?[0]?.Issue ?? "Failed to update.");
                    return RedirectToAction("BookingServiceDetail", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Update floor exception: {ex.Message}");
                SetErrorMessage($"An error occurred: {ex.Message}");
                return RedirectToAction("BookingServiceDetail", new { id });
            }
        }
        #endregion
}