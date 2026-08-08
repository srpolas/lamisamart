using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class WidgetsModel : PageModel
{
    [BindProperty] public bool EnableWhatsappChatWidget { get; set; } = true;
    [BindProperty] public string WhatsappChatNumber { get; set; } = "8801700000000";
    [BindProperty] public string WhatsappChatGreeting { get; set; } = "Hello! 👋 Welcome to LamisaMart. How can we assist with your Saree order today?";
    [BindProperty] public bool EnableBackToTopButton { get; set; } = true;
    [BindProperty] public bool EnableLiveSalesToasts { get; set; } = true;
    [BindProperty] public int SalesToastIntervalSeconds { get; set; } = 15;
    [BindProperty] public bool EnableNewsletterModal { get; set; } = false;
    [BindProperty] public string NewsletterModalHeadline { get; set; } = "Get ৳500 OFF Your First Order!";

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        TempData["SuccessMessage"] = "Interactive UI Widgets saved and published successfully!";
        return RedirectToPage();
    }
}
