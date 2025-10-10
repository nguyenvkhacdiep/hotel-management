using Ecommerce.Base.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace HotelManagement.Extensions;


public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var contextFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            httpContext.Response.StatusCode = contextFeature.Error switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var badRequest = contextFeature.Error as BadRequestException;

            await httpContext.Response.WriteAsJsonAsync(new ErrorDetail
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = contextFeature.Error.Message,
                Errors = badRequest?.ErrorList
            }, cancellationToken);
        }

        return true;
    }
}