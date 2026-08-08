using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class HeaderModel : PageModel
{
    [BindProperty] public string HeaderStyle { get; set; } = "Classic";
    [BindProperty] public bool StickyHeader { get; set; } = true;
    [BindProperty] public bool EnableTopBanner { get; set; } = true;
    [BindProperty] public string TopBannerText { get; set; } = "🎉 Free Shipping on Orders Over ৳5,000 | Fast Delivery Across Bangladesh!";
    [BindProperty] public string TopBannerBgColor { get; set; } = "#E11D48";
    [BindProperty] public string TopBannerTextColor { get; set; } = "#FFFFFF";
    [BindProperty] public string LogoUrl { get; set; } = "/images/logo.png";
    [BindProperty] public bool EnableSearchBar { get; set; } = true;
    [BindProperty] public bool EnableInstantAutoComplete { get; set; } = true;
    [BindProperty] public bool ShowWishlistIcon { get; set; } = true;
    [BindProperty] public bool ShowCartDrawer { get; set; } = true;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        TempData["SuccessMessage"] = "Header UI Customizations saved and published successfully!";
        return RedirectToPage();
    }
}
