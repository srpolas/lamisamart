using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class SettingsModel : PageModel
{
    [BindProperty] public string PrimaryColor { get; set; } = "#E11D48";
    [BindProperty] public string SecondaryColor { get; set; } = "#0F172A";
    [BindProperty] public string BodyFontFamily { get; set; } = "Inter";
    [BindProperty] public bool EnableDarkModeToggle { get; set; } = true;
    [BindProperty] public string CustomCssCode { get; set; } = "/* Custom CSS Overrides */\n.btn-primary { border-radius: 50px; }";
    [BindProperty] public string HeaderScriptsCode { get; set; } = "<!-- Analytics & Tracking Scripts -->";

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        TempData["SuccessMessage"] = "UI Theme Settings saved and published successfully!";
        return RedirectToPage();
    }
}
