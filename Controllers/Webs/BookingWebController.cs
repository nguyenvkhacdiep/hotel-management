using System.Text.Json;
using HotelManagement.Extensions;
using HotelManagement.Models.Common;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers.Webs;

[Route("booking")]
[Authorize(Roles = "Admin")]

public class BookingWebController : BaseWebController
{
    public BookingWebController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        : base(httpClientFactory, configuration)
    {
    }
    
    #region ListRoomAvailable
        [HttpGet("list-room-available")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListRoomAvailable([FromQuery] AvailabelRoomParameters parameters)
        {
            ViewData["Title"] = "Room List Available";
            
            try
            {
                SetAuthorizationHeader();
                
                var queryParams = new QueryParameters {};
                
                queryParams.AddFilter("RoomTypeId", parameters?.RoomTypeId.ToString());
                queryParams.AddFilter("FloorId", parameters?.FloorId.ToString());
                queryParams.AddFilter("MinCapacity", parameters?.MinCapacity.ToString());
                queryParams.AddFilter("Checkin", parameters?.Checkin.ToString());
                queryParams.AddFilter("Checkout", parameters?.Checkout.ToString());
                
                
                var responseRoomType = await HttpClient.GetAsync($"RoomType/get-all-room-type?PageSize=10000");
                var roomTypes = new PageList<RoomTypeResponseModel>();

                if (responseRoomType.IsSuccessStatusCode)
                {
                    var content = await responseRoomType.Content.ReadAsStringAsync();
                    roomTypes = JsonSerializer.Deserialize<PageList<RoomTypeResponseModel>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                
                var responseFloor = await HttpClient.GetAsync($"Floor/get-all-floor?PageSize=10000");
                var floors = new PageList<FloorResponseModel>();

                if (responseFloor.IsSuccessStatusCode)
                {
                    var content = await responseFloor.Content.ReadAsStringAsync();
                    floors = JsonSerializer.Deserialize<PageList<FloorResponseModel>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                
                var apiUrl = BaseApiUrl + $"Room/available?{queryParams.ToQueryString()}";
                var response = await HttpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<RoomResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    var filters = GetRoomFilters(roomTypes, floors);
                    ViewBag.Filters = filters;

                    return View(rooms ?? new List<RoomResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to load room list.";
                    return View(new List<RoomResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new List<RoomResponseModel>());
            }
        }
        
        private List<FilterConfig> GetRoomFilters(PageList<RoomTypeResponseModel> roomTypes,PageList<FloorResponseModel> floors)
        {
            return new List<FilterConfig>
            {
                new FilterConfig
                {
                    Name = "RoomTypeId",
                    Label = "Room Type",
                    Type = FilterType.Select,
                    Options = roomTypes.Data
                        ?.ToDictionary(rt => rt.Id.ToString(), rt => rt.Name) ?? new Dictionary<string, string>()
                },
                new FilterConfig
                {
                    Name = "FloorId",
                    Label = "Floor",
                    Type = FilterType.Select,
                    Options = floors.Data
                        ?.ToDictionary(rt => rt.Id.ToString(), rt => rt.FloorNumber) ?? new Dictionary<string, string>()
                },
                new FilterConfig
                {
                    Name = "MinCapacity",
                    Label = "Min Capacity",
                    Type = FilterType.Number,
                },
                new FilterConfig
                {
                    Name = "Checkin",
                    Label = "Check In",
                    Type = FilterType.Date,
                },
                new FilterConfig
                {
                    Name = "Checkout",
                    Label = "Check Out",
                    Type = FilterType.Date,
                },
            };
        }
        #endregion
        
    #region CreateBooking - GET
    [HttpGet("add-booking")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBooking(DateTime? checkIn, DateTime? checkOut)
    {
        ViewData["Title"] = "Booking";
        
        try
        {
            var model = new AddBookingWebDto
            {
                CheckInDate = checkIn ?? DateTime.Today.AddDays(1),
                CheckOutDate = checkOut ?? DateTime.Today.AddDays(2),
            };
            SetAuthorizationHeader();
            ViewBag.CurrentStep = 1;
            
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View();
        }
    }
    #endregion
    
    #region AddBooking - POST
    [HttpPost("add-booking")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBooking(AddBookingWebDto model)
    {
            Console.WriteLine(JsonSerializer.Serialize(model));
        
        if (model.RoomIdsString != null)
        {
            var selectedRooms = JsonSerializer.Deserialize<List<Guid>>(model.RoomIdsString);
            model.RoomIds = selectedRooms;
        }
        
        ModelState.Clear();
        TryValidateModel(model);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("AddBooking");
        }

        try
        {
            SetAuthorizationHeader();
            
            var apiUrl = BaseApiUrl + "Booking/add-new-booking";
            var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                TempData["ClearSessionFlag"] = true;
                return RedirectToAction("ListBooking");
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
                return RedirectToAction("AddBooking");
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("AddBooking");
        }
    }
    #endregion
    
    #region SearchByPhone - GET
    [HttpGet("search-customer")]
    public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
    { 
        SetAuthorizationHeader();
        var apiUrl = BaseApiUrl + $"Customer/get-customer-by-phone?phone={phone}";
        var response = await HttpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        return StatusCode(500, new { message = "Internal server error." });
    }
    #endregion
    
    #region ListBooking
        [HttpGet("list-booking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListBooking([FromQuery] BookingRequestParameters parameters)
        {
            ViewData["Title"] = "Bookings";
            
            try
            {
                SetAuthorizationHeader();
                
                var queryParams = new QueryParameters
                {
                    SearchKey = parameters?.SearchKey?.Trim(),
                    PageNumber = parameters?.PageNumber ?? 1,
                    PageSize = parameters?.PageSize ?? 10,
                };
                
                queryParams.AddFilter("Status", parameters?.Status.ToString());
                queryParams.AddFilter("RoomId", parameters?.RoomId.ToString());
                queryParams.AddFilter("CheckInDateFrom", parameters?.CheckInDateFrom.ToString());
                queryParams.AddFilter("CheckInDateTo", parameters?.CheckInDateTo.ToString());
                queryParams.AddFilter("CheckOutDateFrom", parameters?.CheckOutDateFrom.ToString());
                queryParams.AddFilter("CheckOutDateTo", parameters?.CheckOutDateTo.ToString());
                
                
                var apiUrl = BaseApiUrl + $"Booking/get-all-bookings?{queryParams.ToQueryString()}";
                var response = await HttpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<PageList<BookingResponseModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                   
                    var filters = GetBookingFilters();
                    ViewBag.Filters = filters;

                    return View(bookings ?? new PageList<BookingResponseModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return HandleUnauthorized();
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to load bookings list.";
                    return View(new PageList<BookingResponseModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new PageList<BookingResponseModel>());
            }
        }
        
        private List<FilterConfig> GetBookingFilters()
        {
            return new List<FilterConfig>
            {
                new FilterConfig
                {
                    Name = "Status",
                    Label = "Status",
                    Type = FilterType.Select,
                    Options = new Dictionary<string, string>
                    {
                        { "0", "Pending" },
                        { "1", "Confirmed" },
                        { "2", "CheckedIn" },
                        { "3", "CheckedOut" },
                        { "4", "Canceled" },
                    }
                },
                new FilterConfig
                {
                    Name = "CheckInDateFrom",
                    Label = "Check In DateFrom",
                    Type = FilterType.Date,
                },
                new FilterConfig
                {
                    Name = "CheckInDateTo",
                    Label = "Check In DateTo",
                    Type = FilterType.Date,
                },
                new FilterConfig
                {
                    Name = "CheckOutDateFrom",
                    Label = "Check Out DateFrom",
                    Type = FilterType.Date,
                },
                new FilterConfig
                {
                    Name = "CheckOutDateTo",
                    Label = "Check Out DateTo",
                    Type = FilterType.Date,
                },
            };
        }
        #endregion
        
    #region DeleteBooking - POST
    [HttpPost("delete-booking/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(Guid id,string? returnTo)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.DeleteAsync($"{BaseApiUrl}Booking/delete-booking/{id}");
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListBooking");
                
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to delete booking.");
                if (returnTo == "detail")
                    return RedirectToAction("BookingDetail", new { id  });
                return RedirectToAction("ListBooking");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            if (returnTo == "detail")
                return RedirectToAction("BookingDetail", new { id  });
            return RedirectToAction("ListBooking");
        }
    }
    #endregion
    
    #region BookingDetails
    [HttpGet("booking-detail/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BookingDetail(Guid id)
    {
        ViewData["Title"] = "Booking Details";
            
        try
        {
            SetAuthorizationHeader(); ;
                
            var response = await HttpClient.GetAsync($"{BaseApiUrl}Booking/booking/{id}");
                
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }

            if (!response.IsSuccessStatusCode)
            {
                SetErrorMessage("Booking not found.");
                return RedirectToAction("ListBooking");
            }

            var content = await response.Content.ReadAsStringAsync();
            var booking = JsonSerializer.Deserialize<BookingResponseModel>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (booking == null)
            {
                SetErrorMessage("Booking not found.");
                return RedirectToAction("ListBooking");
            }

            return View(booking);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            return RedirectToAction("ListBooking");
        }
    }
    #endregion
    
    #region ConfirmBooking - POST
    [HttpPost("confirm-booking/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.PostAsync($"{BaseApiUrl}Booking/confirm-booking/{id}", null);
            Console.WriteLine(response);
            var result = await response.Content.ReadAsStringAsync();
            
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("BookingDetail",new { id  });
                
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to confirm booking.");
                return RedirectToAction("BookingDetail", new { id  });
      ;
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
          
            return RedirectToAction("BookingDetail", new { id  });
        }
    }
    #endregion
    
    #region CancelBooking - POST
    [HttpPost("cancel-booking/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.PostAsync($"{BaseApiUrl}Booking/cancel-booking/{id}", null);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("BookingDetail",new { id  });
                
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to cancel booking.");
                return RedirectToAction("BookingDetail", new { id  });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
          
            return RedirectToAction("BookingDetail", new { id  });
        }
    }
    #endregion
    
    #region CheckInBooking - POST
    [HttpPost("checkin-booking/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInBooking(Guid id)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.PostAsync($"{BaseApiUrl}Booking/checkin-booking/{id}", null);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("BookingDetail",new { id  });
                
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to cancel booking.");
                return RedirectToAction("BookingDetail", new { id  });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
          
            return RedirectToAction("BookingDetail", new { id  });
        }
    }
    #endregion
    
    #region CheckoutBooking - POST
    [HttpPost("checkout-booking/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckoutBooking(Guid id)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.PostAsync($"{BaseApiUrl}Booking/checkout-booking/{id}", null);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("BookingDetail",new { id  });
                
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to cancel booking.");
                return RedirectToAction("BookingDetail", new { id  });
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
          
            return RedirectToAction("BookingDetail", new { id  });
        }
    }
    #endregion
    
    #region CreateInvoice - GET
    [HttpGet("booking-detail/{bookingId:guid}/create-invoice")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateInvoice(Guid bookingId)
    {
        ViewData["Title"] = "Booking";
        
        try
        {
            SetAuthorizationHeader();
            
            var bookingResponse = await HttpClient.GetAsync($"{BaseApiUrl}Booking/booking/{bookingId}");
            var booking = new BookingResponseModel();
            
            if (bookingResponse.IsSuccessStatusCode)
            {
                var result = await bookingResponse.Content.ReadAsStringAsync();
                booking = JsonSerializer.Deserialize<BookingResponseModel>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            ViewBag.Booking = booking;
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View();
        }
    }
    #endregion
    
    #region CreateInvoice - POST
    [HttpPost("booking-detail/{bookingId:guid}/create-invoice")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice(AddInvoiceDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("CreateInvoice");
        }

        try
        {
            SetAuthorizationHeader();
            
            var apiUrl = BaseApiUrl + "Invoice";
            var response = await HttpClient.PostAsJsonAsync(apiUrl,model);
            Console.WriteLine(response);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListBooking");
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
                return RedirectToAction("CreateInvoice");
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("CreateInvoice");
        }
    }
    #endregion
    
    #region ListInvoice
    [HttpGet("list-invoice")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListInvoice([FromQuery] RequestParameters parameters)
    {
        ViewData["Title"] = "Invoices";
        
        try
        {
            SetAuthorizationHeader();
            
            var queryParams = new QueryParameters
            {
                SearchKey = parameters?.SearchKey?.Trim(),
                PageNumber = parameters?.PageNumber ?? 1,
                PageSize = parameters?.PageSize ?? 10,
            };
            
            var apiUrl = BaseApiUrl + $"Invoice?{queryParams.ToQueryString()}";
            var response = await HttpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var invoices = JsonSerializer.Deserialize<PageList<InvoiceResponseModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(invoices ?? new PageList<InvoiceResponseModel>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to load invoices list.";
                return View(new PageList<InvoiceResponseModel>());
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return View(new PageList<InvoiceResponseModel>());
        }
    }

    #endregion
    
    #region DeleteInvoice - POST
    [HttpPost("delete-invoice/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteInvoice(Guid id,string? returnTo)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await HttpClient.DeleteAsync($"{BaseApiUrl}Invoice/{id}");
            var result = await response.Content.ReadAsStringAsync();
                
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListInvoice");
                    
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to delete invoice.");
                if (returnTo == "detail")
                    return RedirectToAction("InvoiceDetail", new { id  });
                return RedirectToAction("ListInvoice");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            if (returnTo == "detail")
                return RedirectToAction("InvoiceDetail", new { id  });
            return RedirectToAction("ListInvoice");
        }
    }
    #endregion
    
    #region ChangeInvoicePaidStatus - POST
    [HttpPost("change-invoice-status/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInvoicePaidStatus(Guid id, string? returnTo)
    {
        try
        {
            Console.WriteLine("--------------------------------");
            SetAuthorizationHeader();
            var response = await HttpClient.PatchAsJsonAsync($"{BaseApiUrl}Invoice/{id}/payment-status", new {
                paymentStatus = 1
            });
            var result = await response.Content.ReadAsStringAsync();
                
            if (response.IsSuccessStatusCode)
            {
                var message = JsonSerializer.Deserialize<MessageResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SetSuccessMessage(message.Message);
                return RedirectToAction("ListInvoice");
                    
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return HandleUnauthorized();
            }
            else
            {
                var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(result, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SetErrorMessage(errorObj?.Message ??"Failed to change status invoice.");
                if (returnTo == "detail")
                    return RedirectToAction("InvoiceDetail", new { id  });
                return RedirectToAction("ListInvoice");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"An error occurred: {ex.Message}");
            if (returnTo == "detail")
                return RedirectToAction("InvoiceDetail", new { id  });
            return RedirectToAction("ListInvoice");
        }
    }
    #endregion
}