using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class FooterModel : PageModel
{
    [BindProperty] public string FooterAboutText { get; set; } = "LamisaMart is Bangladesh's premier online destination for authentic Dhakai Jamdani, Rajshahi Silk, and traditional handloom heritage.";
    [BindProperty] public string CopyrightText { get; set; } = "© 2026 LamisaMart Bangladesh. All Rights Reserved. Designed & Developed for Artisan Weavers.";
    [BindProperty] public string FacebookUrl { get; set; } = "https://facebook.com/lamisamart.bd";
    [BindProperty] public string InstagramUrl { get; set; } = "https://instagram.com/lamisamart.bd";
    [BindProperty] public string YoutubeUrl { get; set; } = "https://youtube.com/@lamisamartbd";
    [BindProperty] public string WhatsappNumber { get; set; } = "+8801700000000";
    [BindProperty] public bool ShowPaymentIconsBar { get; set; } = true;
    [BindProperty] public bool EnableNewsletterFooter { get; set; } = true;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        TempData["SuccessMessage"] = "Footer UI Customizations saved and published successfully!";
        return RedirectToPage();
    }
}
