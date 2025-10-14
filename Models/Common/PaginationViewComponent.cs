using HotelManagement.Models.Common;
using Microsoft.AspNetCore.Mvc;


namespace HotelManagement.Web.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            IPaginationInfo pageList, 
            QueryParameters queryParams,
            string actionName = "Index")
        {
            var model = new PaginationViewModel
            {
                PageList = pageList,
                QueryParams = queryParams,
                ActionName = actionName
            };

            return View(model);
        }
    }

    public class PaginationViewModel
    {
        public IPaginationInfo PageList { get; set; }
        public QueryParameters QueryParams { get; set; }
        public string ActionName { get; set; }
    }
    
    public interface IPaginationInfo
    {
        int PageIndex { get; }
        int TotalPages { get; }
        int TotalItems { get; }
        int PageSize { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}