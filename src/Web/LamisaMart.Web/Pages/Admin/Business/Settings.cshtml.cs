using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Business;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class SettingsModel : PageModel
{
    [BindProperty]
    public string StoreName { get; set; } = "LamisaMart Bangladesh";

    [BindProperty]
    public string SupportEmail { get; set; } = "support@lamisamart.bd";

    [BindProperty]
    public string SupportPhone { get; set; } = "+880 9610-123456";

    [BindProperty]
    public decimal DefaultDeliveryFee { get; set; } = 120m;

    [BindProperty]
    public decimal VatTaxRate { get; set; } = 5.0m;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        TempData["SuccessMessage"] = "Business Settings updated successfully!";
        return RedirectToPage();
    }
}
