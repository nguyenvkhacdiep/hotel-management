using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HotelManagement.Helpers;

[HtmlTargetElement("page-size-selector")]
public class PageSizeSelectorTagHelper: TagHelper
{
    public int CurrentPageSize { get; set; } = 10;
    public int[] Sizes { get; set; } = new[] { 5, 10, 25, 50, 100 };

    public override  void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "d-flex align-items-center");

        var html = @"
                <label class=""me-2"">Show:</label>
                <select class=""form-select form-select-sm w-auto"" onchange=""changePageSize(this.value)"">";

        foreach (var size in Sizes)
        {
            var selected = size == CurrentPageSize ? "selected" : "";
            html += $"<option value=\"{size}\" {selected}>{size}</option>";
        }

        html += @"
                </select>
                <span class=""ms-2 text-muted"">entries</span>";

        output.Content.SetHtmlContent(html);
    }
}
