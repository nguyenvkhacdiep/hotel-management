namespace HotelManagement.Extensions;

public class FieldError
{
    public string Field { get; set; }
    public string Issue { get; set; }
}

public class ApiErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public List<FieldError> Errors { get; set; }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message, List<FieldError>? errors = null)
        : base(message)
    {
        ErrorList = errors;
    }

    public List<FieldError>? ErrorList { get; }
}