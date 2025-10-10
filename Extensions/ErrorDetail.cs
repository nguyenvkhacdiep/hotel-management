using Ecommerce.Base.Exceptions;

namespace HotelManagement.Extensions;


public class ErrorDetail
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<FieldError>? Errors { get; set; }
}