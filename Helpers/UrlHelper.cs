using HotelManagement.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Helpers
{
    public static class UrlHelperExtensions
    {
        public static string BuildPageUrl(this IUrlHelper urlHelper, 
            string action, 
            QueryParameters parameters, 
            int? page = null)
        {
            var queryParams = new QueryParameters
            {
                SearchKey = parameters.SearchKey,
                PageSize = parameters.PageSize,
                PageNumber = page ?? parameters.PageNumber,
                OrderBy = parameters.OrderBy,
            };

            return $"{urlHelper.Action(action)}?{queryParams.ToQueryString()}";
        }
    }
}