using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Business;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class StatusModel : PageModel
{
    public decimal TotalRevenue { get; set; } = 485900m;
    public int TotalOrders { get; set; } = 1420;
    public decimal AverageOrderValue { get; set; } = 3420m;
    public int ActiveCustomers { get; set; } = 980;
    public double ConversionRate { get; set; } = 3.65;

    public void OnGet()
    {
    }
}
